using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

/// <summary>Per-context backoff of the error log: 1 min, 10 min, then 60 min.</summary>
public sealed class ErrorThrottleTests
{
    private readonly ErrorThrottle _throttle = new();

    [Fact]
    public void TryReport_First_ReportsWithZeroRepeats()
    {
        bool reported = _throttle.TryReport("A", 0, out int repeats);

        Assert.True(reported);
        Assert.Equal(0, repeats);
    }

    [Fact]
    public void TryReport_RepeatBeforeOneMinute_NotReported()
    {
        _throttle.TryReport("A", 0, out _);

        Assert.False(_throttle.TryReport("A", 1_000, out _));
        Assert.False(_throttle.TryReport("A", 59_999, out _));
    }

    [Fact]
    public void TryReport_RepeatAtOneMinute_ReportsCountIncludingCurrent()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("A", 1_000, out _);
        _throttle.TryReport("A", 2_000, out _);

        bool reported = _throttle.TryReport("A", 60_000, out int repeats);

        Assert.True(reported);
        Assert.Equal(3, repeats);
    }

    [Fact]
    public void TryReport_SecondGap_IsTenMinutes()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("A", 60_000, out _);

        bool early = _throttle.TryReport("A", 659_999, out _);
        bool reported = _throttle.TryReport("A", 660_000, out int repeats);

        Assert.False(early);
        Assert.True(reported);
        Assert.Equal(2, repeats);
    }

    [Fact]
    public void TryReport_ThirdGap_IsSixtyMinutes()
    {
        ReportThroughSecondGap();

        bool early = _throttle.TryReport("A", 4_259_999, out _);
        bool reported = _throttle.TryReport("A", 4_260_000, out _);

        Assert.False(early);
        Assert.True(reported);
    }

    [Fact]
    public void TryReport_AfterThirdGap_StaysSixtyMinutes()
    {
        ReportThroughSecondGap();
        _throttle.TryReport("A", 4_260_000, out _);

        bool early = _throttle.TryReport("A", 7_859_999, out _);
        bool reported = _throttle.TryReport("A", 7_860_000, out _);

        Assert.False(early);
        Assert.True(reported);
    }

    [Fact]
    public void TryReport_SteadySpam_ReportsOnBackoffSchedule()
    {
        long[] expected = { 0, 60_000, 660_000, 4_260_000, 7_860_000 };
        var reportedAt = new List<long>();
        long previous = 0;

        for (long t = 0; t <= 8_000_000; t += 1_000)
        {
            if (!_throttle.TryReport("A", t, out int repeats))
            {
                continue;
            }

            reportedAt.Add(t);
            Assert.Equal(t == 0 ? 0 : (t - previous) / 1_000, repeats);
            previous = t;
        }

        Assert.Equal(expected, reportedAt);
    }

    [Fact]
    public void TryReport_LateSingleRepeat_ReportsOne()
    {
        _throttle.TryReport("A", 0, out _);

        bool reported = _throttle.TryReport("A", 5_000_000, out int repeats);

        Assert.True(reported);
        Assert.Equal(1, repeats);
    }

    [Fact]
    public void TryReport_LateReport_NextGapCountsFromIt()
    {
        _throttle.TryReport("A", 0, out _);

        bool late = _throttle.TryReport("A", 5_000_000, out int lateRepeats);
        bool early = _throttle.TryReport("A", 5_599_999, out _);
        bool next = _throttle.TryReport("A", 5_600_000, out int nextRepeats);

        Assert.True(late);
        Assert.Equal(1, lateRepeats);
        Assert.False(early);
        Assert.True(next);
        Assert.Equal(2, nextRepeats);
    }

    [Fact]
    public void TryReport_DifferentContexts_KeepOwnBackoff()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("B", 0, out _);
        _throttle.TryReport("A", 30_000, out _);
        _throttle.TryReport("A", 60_000, out _);

        bool reported = _throttle.TryReport("B", 60_000, out int repeats);

        Assert.True(reported);
        Assert.Equal(1, repeats);
    }

    [Fact]
    public void TryReport_DifferentContexts_Independent()
    {
        _throttle.TryReport("A", 0, out _);

        bool repeatA = _throttle.TryReport("A", 1, out _);
        bool firstB = _throttle.TryReport("B", 2, out int repeats);

        Assert.False(repeatA);
        Assert.True(firstB);
        Assert.Equal(0, repeats);
    }

    [Fact]
    public void TryReport_NullContext_SameAsEmpty()
    {
        bool first = _throttle.TryReport(null, 0, out _);
        bool repeat = _throttle.TryReport("", 1, out _);

        Assert.True(first);
        Assert.False(repeat);
    }

    private void ReportThroughSecondGap()
    {
        _throttle.TryReport("A", 0, out _);
        _throttle.TryReport("A", 60_000, out _);
        _throttle.TryReport("A", 660_000, out _);
    }
}
