using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterGrowthReapplyTests
{
    private readonly HeadhunterStackState _stacks = new(2);

    [Fact]
    public void Check_RowExpired_ReAddsLiveWithRemaining()
    {
        HeadhunterGrowthReapply reapply = Create(new HeadhunterGrowthCurve(10f, 25f));

        BuffAction action = Assert.Single(reapply.Check([5f, 0f]));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(1, action.StatId);
        Assert.Equal(2, action.Stacks);
        Assert.Equal(5f, action.DurationSeconds);
        Assert.Equal(0.22f, action.Increased, 0.0001f);
    }

    [Fact]
    public void Check_SameTotal_NoActions()
    {
        HeadhunterGrowthReapply reapply = Create(new HeadhunterGrowthCurve(10f, 25f));
        reapply.Check([5f, 0f]);

        Assert.Empty(reapply.Check([4f, 0f]));
    }

    [Fact]
    public void Check_GrowthOff_SyncsButNoActions()
    {
        HeadhunterGrowthReapply reapply = Create(HeadhunterGrowthCurve.None);

        Assert.Empty(reapply.Check([5f, 0f]));
        Assert.Equal(0, _stacks.Get(1));
    }

    [Fact]
    public void Check_TwoLiveRows_EachKeepsOwnRemaining()
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.ConfigWithGrowth(
                new HeadhunterGrowthCurve(10f, 25f),
                HeadhunterTestData.Entry("FakeA", 0f, 10f),
                HeadhunterTestData.Entry("FakeB", 5f),
                HeadhunterTestData.Entry("FakeC", 5f)
            )
        );
        var stacks = new HeadhunterStackState(3);
        StacksA2B1(stacks);
        stacks.TryAdd(2, 5);
        var growth = new HeadhunterValueGrowth(config.ValueGrowth);
        growth.Update(4);
        var reapply = new HeadhunterGrowthReapply(config, stacks, growth);

        BuffAction[] actions = reapply.Check([5f, 0f, 7f]).ToArray();

        Assert.Equal(
            new[] { (1, 2, 5f), (3, 1, 7f) },
            actions.Select(action => (action.StatId, action.Stacks, action.DurationSeconds))
        );
        Assert.Equal(0.24f, actions[0].Increased, 0.0001f);
        Assert.Equal(6f, actions[1].Added, 0.0001f);
    }

    private static void StacksA2B1(HeadhunterStackState stacks)
    {
        stacks.TryAdd(0, 5);
        stacks.TryAdd(0, 5);
        stacks.TryAdd(1, 5);
    }

    private HeadhunterGrowthReapply Create(HeadhunterGrowthCurve curve)
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.GrowthConfig(curve);
        StacksA2B1(_stacks);
        var growth = new HeadhunterValueGrowth(config.ValueGrowth);
        growth.Update(3);
        return new HeadhunterGrowthReapply(config, _stacks, growth);
    }
}
