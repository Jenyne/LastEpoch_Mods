using LastEpoch_Hud.Scripts.Core.BuildImport;

namespace LastEpoch_Hud.Tests.Core.BuildImport;

public sealed class MaxrollPreviewSessionTests
{
    const string Link = "https://maxroll.gg/last-epoch/planner/test1";

    static MaxrollBuild Build(string name) =>
        MaxrollBuildParser.Parse("{\"name\":\"" + name + "\",\"items\":{}}");

    [Fact]
    public async Task Results_ArePublishedOnlyWhenPolled()
    {
        var completed = new TaskCompletionSource<MaxrollBuild>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var session = new MaxrollPreviewSession(
            (_, _) =>
            {
                started.SetResult();
                return completed.Task;
            }
        );
        session.Load(Link);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.True(session.IsLoading);
        Assert.Null(session.Build);
        completed.SetResult(Build("First"));
        await Poll(session);
        Assert.False(session.IsLoading);
        Assert.NotNull(session.Build);
        Assert.Null(session.Error);
        Assert.False(session.Poll());
    }

    [Fact]
    public async Task Closing_DiscardsEvenAnUncooperativeRequest()
    {
        var completed = new TaskCompletionSource<MaxrollBuild>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken token = default;
        var session = new MaxrollPreviewSession(
            (_, cancellation) =>
            {
                token = cancellation;
                started.SetResult();
                return completed.Task;
            }
        );
        session.Load(Link);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        session.Cancel();
        Assert.True(token.IsCancellationRequested);
        completed.SetResult(Build("Abandoned"));
        await Task.Yield();
        Assert.False(session.Poll());
        Assert.False(session.IsLoading);
        Assert.Null(session.Build);
    }

    [Fact]
    public async Task ReplacingRequest_IgnoresLateFailureAndKeepsNewBuild()
    {
        var old = new TaskCompletionSource<MaxrollBuild>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var next = Build("New");
        int calls = 0;
        var session = new MaxrollPreviewSession(
            (_, _) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                {
                    started.SetResult();
                    return old.Task;
                }
                return Task.FromResult(next);
            }
        );
        session.Load(Link);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        session.Load(Link);
        await Poll(session);
        old.SetException(new InvalidOperationException("late failure"));
        await Task.Yield();
        Assert.Same(next, session.Build);
        Assert.Null(session.Error);
        Assert.False(session.Poll());
    }

    [Fact]
    public async Task Failure_CanBeRetriedWithoutKeepingStaleData()
    {
        int calls = 0;
        var session = new MaxrollPreviewSession(
            (_, _) =>
                Interlocked.Increment(ref calls) == 1
                    ? Task.FromException<MaxrollBuild>(new InvalidOperationException("denied"))
                    : Task.FromResult(Build("Retry"))
        );
        session.Load(Link);
        await Poll(session);
        Assert.Equal("denied", session.Error);
        Assert.Null(session.Build);
        session.Load(Link);
        Assert.Null(session.Error);
        await Poll(session);
        Assert.NotNull(session.Build);
        Assert.Null(session.Error);
        session.Cancel();
        Assert.NotNull(session.Build);
    }

    [Fact]
    public void InvalidLink_DoesNotCallTransport()
    {
        int calls = 0;
        var session = new MaxrollPreviewSession(
            (_, _) =>
            {
                calls++;
                return Task.FromResult(Build("Unexpected"));
            }
        );
        Assert.Throws<FormatException>(() => session.Load("https://example.com"));
        Assert.Equal(0, calls);
        Assert.False(session.IsLoading);
    }

    static async Task Poll(MaxrollPreviewSession session)
    {
        using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!session.Poll())
            await Task.Delay(1, budget.Token);
    }
}
