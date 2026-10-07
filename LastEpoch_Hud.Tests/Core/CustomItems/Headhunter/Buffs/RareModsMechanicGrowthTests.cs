using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class RareModsMechanicGrowthTests
{
    private const int StatA = 1;
    private const int StatB = 2;
    private static readonly HeadhunterGrowthCurve _curve = new(10f, 25f);

    [Fact]
    public void OnKill_FirstKill_PlainBaseValue()
    {
        RareModsMechanic mechanic = Create(_curve, out _);

        BuffAction action = Assert.Single(Kill(mechanic, StatA));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(1, action.Stacks);
        Assert.Equal(0.1f, action.Increased, 0.0001f);
    }

    [Fact]
    public void OnKill_FactorRises_ReAddsOtherLiveWithGrownValue()
    {
        RareModsMechanic mechanic = Create(_curve, out _);
        Kill(mechanic, StatA);

        BuffAction[] actions = Kill(mechanic, StatB, 0).ToArray();

        Assert.Equal(2, actions.Length);
        BuffAction b = Assert.Single(actions, action => action.StatId == StatB);
        BuffAction a = Assert.Single(actions, action => action.StatId == StatA);
        Assert.Equal(BuffActionKind.Add, b.Kind);
        Assert.Equal(BuffActionKind.Add, a.Kind);
        Assert.Equal(HeadhunterTestData.Duration, a.DurationSeconds);
        Assert.Equal(1, a.Stacks);
        Assert.Equal(0.11f, a.Increased, 0.0001f);
        Assert.Equal(5.5f, b.Added, 0.0001f);
    }

    [Fact]
    public void OnKill_FactorKept_KeepsRefresh()
    {
        RareModsMechanic grown = Create(new HeadhunterGrowthCurve(10f, 10f), out _);
        RareModsMechanic plain = Create(HeadhunterGrowthCurve.None, out _);
        Kill(grown, StatA);
        Kill(grown, StatB, 0);
        Kill(plain, StatA);
        Kill(plain, StatB, 0);

        BuffAction[] grownLast = Kill(grown, StatA, 0, 1).ToArray();
        BuffAction[] plainLast = Kill(plain, StatA, 0, 1).ToArray();

        Assert.Equal(
            plainLast.Select(action => (action.StatId, action.Kind)),
            grownLast.Select(action => (action.StatId, action.Kind))
        );
        BuffAction other = Assert.Single(grownLast, action => action.StatId == StatB);
        Assert.Equal(BuffActionKind.Refresh, other.Kind);
    }

    [Fact]
    public void OnKill_FactorKept_ValuesStillGrown()
    {
        RareModsMechanic mechanic = Create(new HeadhunterGrowthCurve(10f, 10f), out _);
        Kill(mechanic, StatA);
        Kill(mechanic, StatB, 0);

        BuffAction[] actions = Kill(mechanic, StatA, 0, 1).ToArray();

        BuffAction a = Assert.Single(actions, action => action.StatId == StatA);
        BuffAction b = Assert.Single(actions, action => action.StatId == StatB);
        Assert.Equal(BuffActionKind.Add, a.Kind);
        Assert.Equal(2, a.Stacks);
        Assert.Equal(0.22f, a.Increased, 0.0001f);
        Assert.Equal(BuffActionKind.Refresh, b.Kind);
        Assert.Equal(5.5f, b.Added, 0.0001f);
    }

    [Fact]
    public void OnKill_GrowthOff_MatchesOldValues()
    {
        RareModsMechanic mechanic = Create(HeadhunterGrowthCurve.None, out _);
        Kill(mechanic, StatA);

        BuffAction[] actions = Kill(mechanic, StatB, 0).ToArray();

        BuffAction a = Assert.Single(actions, action => action.StatId == StatA);
        BuffAction b = Assert.Single(actions, action => action.StatId == StatB);
        Assert.Equal(BuffActionKind.Refresh, a.Kind);
        Assert.Equal(0.1f, a.Increased, 0.0001f);
        Assert.Equal(5f, b.Added, 0.0001f);
    }

    [Fact]
    public void Reset_ResetsGrowth()
    {
        RareModsMechanic mechanic = Create(_curve, out HeadhunterValueGrowth growth);
        Kill(mechanic, StatA);
        Kill(mechanic, StatB, 0);

        mechanic.Reset();

        Assert.Equal(0, growth.AppliedTotal);
        Assert.Equal(1f, growth.Factor);
    }

    private static RareModsMechanic Create(
        HeadhunterGrowthCurve curve,
        out HeadhunterValueGrowth growth
    )
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.GrowthConfig(curve);
        growth = new HeadhunterValueGrowth(config.ValueGrowth);
        return HeadhunterTestData.Mechanic(
            config,
            new HeadhunterStackState(config.Stats.Count),
            new FakeHeadhunterRandom(0),
            growth
        );
    }

    private static IReadOnlyList<BuffAction> Kill(
        RareModsMechanic mechanic,
        int statId,
        params int[] liveRows
    )
    {
        return mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(statId)),
            liveRows.ToHashSet()
        );
    }
}
