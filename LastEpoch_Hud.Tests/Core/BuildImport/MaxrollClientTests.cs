using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.BuildImport;

namespace LastEpoch_Hud.Tests.Core.BuildImport;

public sealed class MaxrollClientTests
{
    sealed class UnknownLengthContent : HttpContent
    {
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
            throw new NotSupportedException();

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(
                new MemoryStream(new byte[MaxrollBuildParser.MaximumBytes + 1])
            );
    }

    sealed class Transport : HttpMessageHandler
    {
        public readonly List<Uri> Requests = new();
        public Func<HttpRequestMessage, HttpResponseMessage> Respond;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request.RequestUri);
            return Task.FromResult(Respond(request));
        }
    }

    static HttpResponseMessage Reply(HttpStatusCode status, string json = "{}") =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task Retrieve_UsesCanonicalPublicRouteAndLinkSelection()
    {
        var handler = new Transport
        {
            Respond = _ =>
                Reply(
                    HttpStatusCode.OK,
                    JsonSerializer.Serialize(new { id = "abcd", data = MaxrollBuildTests.Data })
                ),
        };
        using var client = new MaxrollBuildClient(handler);
        var build = await client.RetrieveAsync(
            "https://www.maxroll.gg/last-epoch/planner/abcd?tracking=ignored#1",
            TestContext.Current.CancellationToken
        );
        Assert.Equal("Starter", build.SelectedVariant.Name);
        Assert.Equal(
            "https://planners.maxroll.gg/profiles/le/abcd",
            Assert.Single(handler.Requests).ToString()
        );
    }

    [Fact]
    public async Task Retrieval_FallsBackToLoaderForUnavailablePrimary()
    {
        var handler = new Transport();
        handler.Respond = _ =>
            handler.Requests.Count == 1
                ? Reply(HttpStatusCode.ServiceUnavailable)
                : Reply(
                    HttpStatusCode.OK,
                    JsonSerializer.Serialize(
                        new { profile = new { id = "abcd", data = MaxrollBuildTests.Data } }
                    )
                );
        using var client = new MaxrollBuildClient(handler);
        Assert.Equal(
            "Endgame Gear",
            (
                await client.RetrieveAsync(
                    "https://maxroll.gg/last-epoch/planner/abcd",
                    TestContext.Current.CancellationToken
                )
            )
                .SelectedVariant
                .Name
        );
        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("last-epoch-planner-by-id", handler.Requests[1].Query);
    }

    [Fact]
    public async Task HtmlResponse_FallsBackInsteadOfTreatingItAsBuildData()
    {
        var handler = new Transport();
        handler.Respond = _ =>
            handler.Requests.Count == 1
                ? Reply(HttpStatusCode.OK, "<html>Unavailable</html>")
                : Reply(
                    HttpStatusCode.OK,
                    JsonSerializer.Serialize(
                        new { profile = new { id = "abcd", data = MaxrollBuildTests.Data } }
                    )
                );
        using var client = new MaxrollBuildClient(handler);
        var build = await client.RetrieveAsync(
            "https://maxroll.gg/last-epoch/planner/abcd#2",
            TestContext.Current.CancellationToken
        );
        Assert.Equal("Endgame Gear", build.SelectedVariant.Name);
        Assert.Equal(build.Link.LoaderEndpoint, build.SourceEndpoint);
    }

    [Fact]
    public async Task WrongBuildId_IsRejectedOnBothRoutes()
    {
        var handler = new Transport
        {
            Respond = _ =>
                Reply(
                    HttpStatusCode.OK,
                    JsonSerializer.Serialize(new { id = "wrong", data = MaxrollBuildTests.Data })
                ),
        };
        using var client = new MaxrollBuildClient(handler);
        await Assert.ThrowsAsync<FormatException>(() =>
            client.RetrieveAsync(
                "https://maxroll.gg/last-epoch/planner/abcd",
                TestContext.Current.CancellationToken
            )
        );
        Assert.Equal(2, handler.Requests.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task DenialRateLimitAndRedirect_StopWithoutAnotherRequest(HttpStatusCode status)
    {
        var handler = new Transport { Respond = _ => Reply(status) };
        using var client = new MaxrollBuildClient(handler);
        await Assert.ThrowsAsync<MaxrollAccessException>(() =>
            client.RetrieveAsync(
                "https://maxroll.gg/last-epoch/planner/abcd",
                TestContext.Current.CancellationToken
            )
        );
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Cancellation_StopsRetrieval()
    {
        var handler = new Transport { Respond = _ => Reply(HttpStatusCode.OK) };
        using var client = new MaxrollBuildClient(handler);
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.RetrieveAsync("https://maxroll.gg/last-epoch/planner/abcd", cancel.Token)
        );
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task OversizedResponse_IsNotParsedOrRetried()
    {
        var handler = new Transport
        {
            Respond = _ =>
                Reply(HttpStatusCode.OK, new string('a', MaxrollBuildParser.MaximumBytes + 1)),
        };
        using var client = new MaxrollBuildClient(handler);
        await Assert.ThrowsAsync<MaxrollAccessException>(() =>
            client.RetrieveAsync(
                "https://maxroll.gg/last-epoch/planner/abcd",
                TestContext.Current.CancellationToken
            )
        );
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task UnknownContentLength_StillEnforcesStreamLimit()
    {
        var handler = new Transport
        {
            Respond = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new UnknownLengthContent(),
            },
        };
        using var client = new MaxrollBuildClient(handler);
        await Assert.ThrowsAsync<MaxrollAccessException>(() =>
            client.RetrieveAsync(
                "https://maxroll.gg/last-epoch/planner/abcd",
                TestContext.Current.CancellationToken
            )
        );
        Assert.Single(handler.Requests);
    }
}
