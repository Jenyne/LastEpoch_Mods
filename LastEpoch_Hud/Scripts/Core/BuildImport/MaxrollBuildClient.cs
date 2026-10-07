using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

public sealed class MaxrollBuildClient : IDisposable
{
    readonly HttpClient http;

    public MaxrollBuildClient()
        : this(new HttpClientHandler { AllowAutoRedirect = false }) { }

    // Injectable transport for deterministic tests. The default transport never follows redirects.
    public MaxrollBuildClient(HttpMessageHandler handler)
    {
        http = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }

    public async Task<MaxrollBuild> RetrieveAsync(
        string input,
        CancellationToken cancellation = default
    )
    {
        var link = MaxrollLink.Parse(input);
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        budget.CancelAfter(TimeSpan.FromSeconds(30));
        Exception firstError = null;
        foreach (var endpoint in new[] { link.ProfileEndpoint, link.LoaderEndpoint })
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Accept.ParseAdd("application/json");
                using var response = await http.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        budget.Token
                    )
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new MaxrollAccessException(
                        "Maxroll is rate limiting requests. Try later or use clipboard JSON."
                    );
                if (
                    response.StatusCode == HttpStatusCode.Unauthorized
                    || response.StatusCode == HttpStatusCode.Forbidden
                )
                    throw new MaxrollAccessException(
                        "Maxroll denied access. Use a public planner or clipboard export."
                    );
                if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                    throw new MaxrollAccessException(
                        "Maxroll redirected the request; redirects are not followed."
                    );
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        "Maxroll returned HTTP " + (int)response.StatusCode + "."
                    );
                if (response.Content.Headers.ContentLength > MaxrollBuildParser.MaximumBytes)
                    throw new MaxrollAccessException(
                        "The Maxroll response exceeds the 4 MiB limit."
                    );
                using var source = await response
                    .Content.ReadAsStreamAsync(budget.Token)
                    .ConfigureAwait(false);
                using var bytes = new MemoryStream();
                var buffer = new byte[8192];
                int read;
                while (
                    (
                        read = await source
                            .ReadAsync(buffer, 0, buffer.Length, budget.Token)
                            .ConfigureAwait(false)
                    ) > 0
                )
                {
                    if (bytes.Length + read > MaxrollBuildParser.MaximumBytes)
                        throw new MaxrollAccessException(
                            "The Maxroll response exceeds the 4 MiB limit."
                        );
                    bytes.Write(buffer, 0, read);
                }
                string json = new UTF8Encoding(false, true).GetString(bytes.ToArray());
                var build = MaxrollBuildParser.Parse(json, link);
                build.SourceEndpoint = endpoint;
                return build;
            }
            catch (Exception ex)
                when (ex is HttpRequestException || ex is JsonException || ex is FormatException)
            {
                if (firstError == null)
                    firstError = ex;
                else
                    throw new FormatException(
                        "Neither Maxroll build route returned usable data. "
                            + firstError.Message
                            + " "
                            + ex.Message,
                        ex
                    );
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            {
                throw new TimeoutException(
                    "Maxroll retrieval timed out. Try later or use clipboard JSON."
                );
            }
        }
        throw new FormatException("No Maxroll build was retrieved.", firstError);
    }

    public void Dispose() => http.Dispose();
}

public sealed class MaxrollAccessException : Exception
{
    public MaxrollAccessException(string message)
        : base(message) { }
}
