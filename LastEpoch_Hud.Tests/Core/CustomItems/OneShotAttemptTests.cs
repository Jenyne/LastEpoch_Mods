using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class OneShotAttemptTests
{
    [Fact]
    public void New_IsNotDone()
    {
        var attempt = new OneShotAttempt();

        Assert.False(attempt.IsDone);
    }

    [Fact]
    public void MarkDone_IsDone()
    {
        var attempt = new OneShotAttempt();

        attempt.MarkDone();

        Assert.True(attempt.IsDone);
    }

    [Fact]
    public void ShouldReportFailure_OnlyFirstTime()
    {
        var attempt = new OneShotAttempt();

        Assert.True(attempt.ShouldReportFailure());
        Assert.False(attempt.ShouldReportFailure());
    }

    [Fact]
    public void ShouldReportFailure_KeepsAttemptOpen()
    {
        var attempt = new OneShotAttempt();

        attempt.ShouldReportFailure();

        Assert.False(attempt.IsDone);
    }
}
