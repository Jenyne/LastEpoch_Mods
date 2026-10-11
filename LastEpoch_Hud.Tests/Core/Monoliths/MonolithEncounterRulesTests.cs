using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithEncounterRulesTests
{
    [Theory]
    [InlineData("WE502", false, false, false, true)]
    [InlineData("we502", false, false, false, true)]
    [InlineData("WE501", false, false, false, false)]
    [InlineData("WE502", true, false, false, true)]
    [InlineData("EchoArena", true, false, false, true)]
    [InlineData("EchoArena", false, true, false, true)]
    [InlineData("EchoArena", false, false, true, true)]
    [InlineData("EchoArena", false, false, false, false)]
    [InlineData(null, false, false, false, false)]
    public void OnlyProtectedEncountersBypassAutomatedMonolithChanges(
        string scene,
        bool quest,
        bool harbinger,
        bool timelineBoss,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            MonolithEncounterRules.IsProtectedEncounter(scene, quest, harbinger, timelineBoss)
        );
    }
}
