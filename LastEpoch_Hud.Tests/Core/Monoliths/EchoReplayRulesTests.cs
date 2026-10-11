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
    public void YrunReplay_AllowsCompletedEncounterWithRequirementsMet() =>
        Assert.True(EchoReplayRules.CanReplayYrun(true, true, true, true, 100, 100, false));

    [Theory]
    [InlineData(false, true, true, true, 100, 100, false)]
    [InlineData(true, false, true, true, 100, 100, false)]
    [InlineData(true, true, false, true, 100, 100, false)]
    [InlineData(true, true, true, false, 100, 100, false)]
    [InlineData(true, true, true, true, 99, 100, false)]
    [InlineData(true, true, true, true, 100, -1, false)]
    [InlineData(true, true, true, true, 100, 100, true)]
    public void YrunReplay_PreservesEachRestriction(
        bool enabled,
        bool isYrun,
        bool completed,
        bool waveCompleted,
        int storedKills,
        int requiredKills,
        bool enteringEcho
    ) =>
        Assert.False(
            EchoReplayRules.CanReplayYrun(
                enabled,
                isYrun,
                completed,
                waveCompleted,
                storedKills,
                requiredKills,
                enteringEcho
            )
        );
}
