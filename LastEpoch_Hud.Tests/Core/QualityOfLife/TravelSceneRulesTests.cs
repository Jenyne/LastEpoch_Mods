using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class TravelSceneRulesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("EoT ")]
    [InlineData("ClientSplash")]
    [InlineData("PersistentUI")]
    [InlineData("CharacterSelectScene")]
    [InlineData("Login")]
    [InlineData("MonolithHub")]
    [InlineData("A_Reward")]
    [InlineData("Mastery")]
    [InlineData("Neutral")]
    [InlineData("PCG_Dev")]
    [InlineData("Dun1Q10")]
    [InlineData("Dun2Q10")]
    [InlineData("Dun3Q10")]
    [InlineData("dun1q11")]
    [InlineData("Dun2Boss")]
    [InlineData("PCG_EchoForest")]
    [InlineData("a_pcg_zone")]
    [InlineData("ArenaWave")]
    [InlineData("arena")]
    [InlineData("../Scene")]
    [InlineData("Scenes\\Scene")]
    [InlineData("EoT\n")]
    public void MenusUtilityScenesAndGeneratedInstancesAreExcluded(string scene)
    {
        Assert.False(TravelSceneRules.IsDestination(scene));
    }

    [Theory]
    [InlineData("EoT")]
    [InlineData("Bazaar")]
    [InlineData("CampaignAreaWithoutWaypoint")]
    [InlineData("NewAreaWithNoTranslation")]
    [InlineData("Dunes")]
    public void OrdinaryAreaNamesDoNotRequireWaypointsOrLocalizedNames(string scene)
    {
        Assert.True(TravelSceneRules.IsDestination(scene));
    }
}
