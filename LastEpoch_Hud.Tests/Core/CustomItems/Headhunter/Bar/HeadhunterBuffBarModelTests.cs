using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

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
            null,
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
            null,
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
            null,
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
            null,
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
            null,
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
            null,
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
            null,
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
            null,
            60f
        );

        Assert.Equal(new[] { 1, 2 }, result.Select(e => e.StatId));
    }

    [Fact]
    public void Build_ReusesList_NoLeftovers()
    {
        var model = new HeadhunterBuffBarModel();
        model.Build(_stats, new[] { 10f, 10f, 10f }, null, 60f);

        IReadOnlyList<HeadhunterBarEntry> result = model.Build(
            _stats,
            new[] { 0f, 10f, 0f },
            null,
            60f
        );

        Assert.Equal(new[] { 2 }, result.Select(e => e.StatId));
    }

    [Fact]
    public void Build_CarriesRowAndStacks()
    {
        var stacks = new HeadhunterStackState(3);
        for (int i = 0; i < 4; i++)
        {
            stacks.TryAdd(1, 10);
        }

        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { 0f, 10f, 0f },
            stacks,
            60f
        );

        HeadhunterBarEntry entry = Assert.Single(result);
        Assert.Equal(1, entry.Row);
        Assert.Equal(4, entry.Stacks);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Build_NullOrZeroStacks_ShowsOne(bool useNull)
    {
        HeadhunterStackState stacks = useNull ? null : new HeadhunterStackState(3);

        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            _stats,
            new[] { 10f, 0f, 0f },
            stacks,
            60f
        );

        Assert.Equal(1, Assert.Single(result).Stacks);
    }

    [Fact]
    public void Build_TaggedRow_EntryCarriesTags()
    {
        HeadhunterBuffStat[] stats = [new(1, "HH_FakeA_FakeTag", 0, 0, 8)];

        IReadOnlyList<HeadhunterBarEntry> result = new HeadhunterBuffBarModel().Build(
            stats,
            new[] { 10f },
            null,
            60f
        );

        Assert.Equal(8, Assert.Single(result).Tags);
    }
}
