using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class EchoReplayRulesTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void IslandReplay_RequiresCompletion(bool enabled, bool completed, bool expected) =>
        Assert.Equal(expected, EchoReplayRules.CanReplayIsland(enabled, completed));

    [Fact]
    public void QuestReplay_AllowsCompletedNormalNonBossWithRequirementsMet() =>
        Assert.True(EchoReplayRules.CanReplayQuest(true, true, false, false, true, true, true));

    [Theory]
    [InlineData(false, true, false, false, true, true, true)]
    [InlineData(true, false, false, false, true, true, true)]
    [InlineData(true, true, true, false, true, true, true)]
    [InlineData(true, true, false, true, true, true, true)]
    [InlineData(true, true, false, false, false, true, true)]
    [InlineData(true, true, false, false, true, false, true)]
    [InlineData(true, true, false, false, true, true, false)]
    public void QuestReplay_PreservesEachRestriction(
        bool enabled,
        bool completed,
        bool empowered,
        bool boss,
        bool completionRequirementMet,
        bool branchRequirementMet,
        bool enoughStability
    ) =>
        Assert.False(
            EchoReplayRules.CanReplayQuest(
                enabled,
                completed,
                empowered,
                boss,
                completionRequirementMet,
                branchRequirementMet,
                enoughStability
            )
        );
}
