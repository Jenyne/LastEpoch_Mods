using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterClearRuleTests
{
    [Fact]
    public void RemoveAll_EmitsRemove_ForEveryTableStat_InTableOrder()
    {
        HeadhunterResolvedConfig config = ThreeStatConfig();

        IReadOnlyList<BuffAction> actions = new HeadhunterClearRule().RemoveAll(config);

        Assert.Equal(3, actions.Count);
        Assert.All(actions, action => Assert.Equal(BuffActionKind.Remove, action.Kind));
        Assert.Equal(["HH_FakeA", "HH_FakeB", "HH_FakeC"], actions.Select(a => a.BuffName));
        Assert.Equal(config.Stats.Select(s => s.StatId), actions.Select(a => a.StatId));
    }

    [Fact]
    public void RemoveAll_StacksZero()
    {
        IReadOnlyList<BuffAction> actions = new HeadhunterClearRule().RemoveAll(ThreeStatConfig());

        Assert.All(actions, action => Assert.Equal(0, action.Stacks));
    }

    [Fact]
    public void RemoveAll_EmptyTable_ReturnsEmpty()
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(HeadhunterTestData.AllTriggers)
        );

        IReadOnlyList<BuffAction> actions = new HeadhunterClearRule().RemoveAll(config);

        Assert.Empty(actions);
    }

    [Fact]
    public void RemoveAll_ReusesList()
    {
        var rule = new HeadhunterClearRule();
        rule.RemoveAll(ThreeStatConfig());
        HeadhunterResolvedConfig small = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA")
            )
        );

        IReadOnlyList<BuffAction> actions = rule.RemoveAll(small);

        Assert.Single(actions);
        Assert.Equal("HH_FakeA", actions[0].BuffName);
    }

    [Fact]
    public void RemoveAll_TaggedRow_CarriesTags()
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Tagged("FakeA", "FakeTag")
            )
        );

        IReadOnlyList<BuffAction> actions = new HeadhunterClearRule().RemoveAll(config);

        Assert.Equal(8, Assert.Single(actions).Tags);
    }

    private static HeadhunterResolvedConfig ThreeStatConfig()
    {
        return HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Entry("FakeB"),
                HeadhunterTestData.Entry("FakeC")
            )
        );
    }
}
