using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

public sealed class MaxrollCatalogClient : IDisposable
{
    public const int MaximumBytes = 16 * 1024 * 1024;
    public static readonly Uri CatalogUri = new(
        "https://assets-ng.maxroll.gg/leplanner/game/data.json"
    );
    readonly HttpClient client;

    public MaxrollCatalogClient()
        : this(new HttpClientHandler { AllowAutoRedirect = false }) { }

    public MaxrollCatalogClient(HttpMessageHandler handler)
    {
        client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("LastEpoch_Hud/MaxrollPreview");
    }

    public async Task<MaxrollPlannerCatalog> RetrieveAsync(CancellationToken token)
    {
        using var response = await client
            .GetAsync(CatalogUri, HttpCompletionOption.ResponseHeadersRead, token)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                "Planner catalog returned HTTP " + (int)response.StatusCode + "."
            );
        if (response.Content.Headers.ContentLength > MaximumBytes)
            throw new FormatException("Planner catalog exceeds the 16 MiB limit.");
        using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        using var bytes = new MemoryStream();
        var buffer = new byte[8192];
        int count;
        while (
            (count = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false))
            > 0
        )
        {
            if (bytes.Length + count > MaximumBytes)
                throw new FormatException("Planner catalog exceeds the 16 MiB limit.");
            bytes.Write(buffer, 0, count);
        }
        token.ThrowIfCancellationRequested();
        return MaxrollPlannerCatalog.Parse(System.Text.Encoding.UTF8.GetString(bytes.ToArray()));
    }

    public void Dispose() => client.Dispose();
}
