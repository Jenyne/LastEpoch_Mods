using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterBuffBarModelTests
{
    private static readonly HeadhunterBuffStat[] _stats =
    {
        new(1, "HH_FakeA", 0, 0),
        new(2, "HH_FakeB", 0, 0),
        new(3, "HH_FakeC", 0, 0),
    };

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Build_NullInputs_Empty(bool nullStats, bool nullRemaining)
    {
        var model = new HeadhunterBuffBarModel();

        IReadOnlyList<HeadhunterBarEntry> result = model.Build(
            nullStats ? null : _stats,
            nullRemaining ? null : new[] { 10f },
            60f
        );

        Assert.Empty(result);
    }

    [Fact]
    public void Build_SkipsExpired_KeepsTableOrder()
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { 10f, 0f, 50f },
            60f
        );

        Assert.Equal(new[] { 1, 3 }, result.Select(e => e.StatId));
    }

    [Fact]
    public void Build_NegativeRemaining_Skipped()
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { -1f, 5f, -0.5f },
            60f
        );

        Assert.Equal(new[] { 2 }, result.Select(e => e.StatId));
    }

    [Theory]
    [InlineData(59.2f, 60)]
    [InlineData(0.1f, 1)]
    [InlineData(42f, 42)]
    public void Build_SecondsLeft_RoundsUp(float remaining, int expected)
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { remaining, 0f, 0f },
            60f
        );

        Assert.Equal(expected, Assert.Single(result).SecondsLeft);
    }

    [Fact]
    public void Build_Elapsed_FromDuration()
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { 30f, 0f, 0f },
            60f
        );

        Assert.Equal(0.5f, Assert.Single(result).Elapsed, 3);
    }

    [Theory]
    [InlineData(90f, 60f)]
    [InlineData(30f, 0f)]
    [InlineData(30f, -5f)]
    public void Build_Elapsed_Clamped(float remaining, float duration)
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { remaining, 0f, 0f },
            duration
        );

        Assert.Equal(0f, Assert.Single(result).Elapsed, 3);
    }

    [Fact]
    public void Build_LengthMismatch_UsesShorter()
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { 10f, 10f },
            60f
        );

        Assert.Equal(new[] { 1, 2 }, result.Select(e => e.StatId));
    }

    [Fact]
    public void Build_MoreRemainingThanStats_UsesStats()
    {
        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats.Take(2).ToArray(),
            new[] { 10f, 10f, 10f },
            60f
        );

        Assert.Equal(new[] { 1, 2 }, result.Select(e => e.StatId));
    }

    [Fact]
    public void Build_ReusesList_NoLeftovers()
    {
        var model = new HeadhunterBuffBarModel();
        model.Build(_stats, new[] { 10f, 10f, 10f }, 60f);

        IReadOnlyList<HeadhunterBarEntry> result = model.Build(_stats, new[] { 0f, 10f, 0f }, 60f);

        Assert.Equal(new[] { 2 }, result.Select(e => e.StatId));
    }
}
