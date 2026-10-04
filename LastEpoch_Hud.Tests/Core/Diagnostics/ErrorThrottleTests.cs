using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

/// <summary>Per-context rate limit of the error log.</summary>
public sealed class ErrorThrottleTests
{
    private const long WindowMs = 60_000;

    private readonly ErrorThrottle _throttle = new(WindowMs);

    [Fact]
    public void TryReport_FirstReport_LogsWithZeroSuppressed()
    {
        bool logged = _throttle.TryReport("A", 0, out int suppressed);

        Assert.True(logged);
        Assert.Equal(0, suppressed);
    }

    [Fact]
    public void TryReport_RepeatInsideWindow_NotLogged()
    {
        _throttle.TryReport("A", 0, out _);

        bool first = _throttle.TryReport("A", 1_000, out int s);

        Assert.False(first);
        Assert.Equal(0, s);
        Assert.False(_throttle.TryReport("A", 59_999, out _));
    }

    [Fact]
    public void TryReport_RepeatAtWindowEnd_LogsWithCount()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("A", 1_000, out _);
        _throttle.TryReport("A", 2_000, out _);

        bool logged = _throttle.TryReport("A", 60_000, out int suppressed);

        Assert.True(logged);
        Assert.Equal(2, suppressed);
    }

    [Fact]
    public void TryReport_AfterLoggedRepeat_CountResetsAndWindowRestarts()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("A", 1_000, out _);
        _throttle.TryReport("A", 2_000, out _);
        _throttle.TryReport("A", 60_000, out _);

        bool insideNewWindow = _throttle.TryReport("A", 60_500, out _);
        bool atNewWindowEnd = _throttle.TryReport("A", 120_000, out int suppressed);

        Assert.False(insideNewWindow);
        Assert.True(atNewWindowEnd);
        Assert.Equal(1, suppressed);
    }

    [Fact]
    public void TryReport_SteadySpam_LogsOncePerWindow()
    {
        for (long t = 0; t <= 180_000; t += 16)
        {
            bool logged = _throttle.TryReport("A", t, out int suppressed);

            Assert.Equal(IsWindowStart(t), logged);
            Assert.True(t == 0 || !logged || suppressed == 3749);
        }
    }

    [Fact]
    public void TryReport_DifferentContexts_Independent()
    {
        _throttle.TryReport("A", 0, out _);

        bool repeatA = _throttle.TryReport("A", 1, out _);
        bool firstB = _throttle.TryReport("B", 2, out int suppressed);

        Assert.False(repeatA);
        Assert.True(firstB);
        Assert.Equal(0, suppressed);
    }

    [Fact]
    public void TryReport_NullContext_SameAsEmpty()
    {
        bool first = _throttle.TryReport(null, 0, out _);
        bool repeat = _throttle.TryReport("", 1, out _);

        Assert.True(first);
        Assert.False(repeat);
    }

    private static bool IsWindowStart(long t)
    {
        return t is 0 or 60_000 or 120_000 or 180_000;
    }
}
