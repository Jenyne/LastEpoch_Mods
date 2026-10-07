using LastEpoch_Hud.Scripts.Core.Login;

namespace LastEpoch_Hud.Tests.Core.Login;

public sealed class OfflineStartupAttemptTests
{
    [Fact]
    public void WaitsForBothAnotherFrameAndSettlingTime()
    {
        var attempt = new OfflineStartupAttempt();
        Assert.False(attempt.TryBegin(true, 10));
        Assert.False(attempt.TryBegin(true, 10.01));
        Assert.True(attempt.TryBegin(true, 10.25));
    }

    [Fact]
    public void TimeBeforeReadinessDoesNotCount()
    {
        var attempt = new OfflineStartupAttempt();
        Assert.False(attempt.TryBegin(false, 0));
        Assert.False(attempt.TryBegin(true, 100));
        Assert.True(attempt.TryBegin(true, 100.25));
    }

    [Fact]
    public void InterruptedReadinessMustSettleAgain()
    {
        var attempt = new OfflineStartupAttempt();
        Assert.False(attempt.TryBegin(true, 0));
        Assert.False(attempt.TryBegin(false, 0.2));
        Assert.False(attempt.TryBegin(true, 1));
        Assert.False(attempt.TryBegin(true, 1.1));
        Assert.True(attempt.TryBegin(true, 1.25));
    }

    [Fact]
    public void NoRetryAfterDispatchEvenIfScreenStaysReady()
    {
        var attempt = new OfflineStartupAttempt();
        attempt.TryBegin(true, 0);
        Assert.True(attempt.TryBegin(true, 1));
        Assert.False(attempt.TryBegin(true, 60));
        Assert.False(attempt.TryBegin(false, 61));
        Assert.False(attempt.TryBegin(true, 62));
    }

    [Fact]
    public void ManualClickConsumesPendingAutomaticAttempt()
    {
        var attempt = new OfflineStartupAttempt();
        attempt.TryBegin(true, 0);
        attempt.MarkAttempted();
        Assert.False(attempt.TryBegin(true, 1));
    }

    [Fact]
    public void ReturningToLandingGetsANewSettledAttempt()
    {
        var attempt = new OfflineStartupAttempt();
        attempt.MarkAttempted();
        attempt.BeginVisit();
        Assert.False(attempt.TryBegin(true, 10));
        Assert.True(attempt.TryBegin(true, 10.25));
    }
}
