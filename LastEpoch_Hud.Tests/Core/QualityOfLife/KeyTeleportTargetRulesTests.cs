using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class KeyTeleportTargetRulesTests
{
    [Theory]
    [InlineData(1, "Dun1Q10", "Temporal Sanctum")]
    [InlineData(2, "Dun2Q10", "Lightless Arbor")]
    [InlineData(3, "Dun3Q10", "Soulfire Bastion")]
    [InlineData(2, "PCG_Dungeon", "The Shrouded Ridge")]
    [InlineData(2, "ArenaWave", "The Surface")]
    public void DungeonAndGeneratedTargetsCannotMatchPresets(int preset, string scene, string label)
    {
        Assert.Equal(-1, KeyTeleportTargetRules.MatchPriority(preset, scene, label));
    }

    [Theory]
    [InlineData(1, "CampaignA", "The Ruined Coast", 0)]
    [InlineData(2, "CampaignB", "The Shrouded Ridge", 0)]
    [InlineData(2, "CampaignC", "The Surface", 1)]
    [InlineData(3, "CampaignD", "Felled Wood", 0)]
    [InlineData(0, "EoT", null, 0)]
    [InlineData(4, "Bazaar", "Bazaar", 0)]
    [InlineData(5, "Observatory", "The Observatory", 0)]
    public void CampaignApproachesAndExistingHubTargetsResolve(
        int preset,
        string scene,
        string label,
        int priority
    )
    {
        Assert.Equal(priority, KeyTeleportTargetRules.MatchPriority(preset, scene, label));
    }

    [Theory]
    [InlineData(1, "CampaignArea", "Temporal Sanctum")]
    [InlineData(2, "CampaignArea", "Lightless Arbor")]
    [InlineData(3, "CampaignArea", "Soulfire Bastion")]
    [InlineData(1, "CampaignArea", "The Ruined Coast Exit")]
    [InlineData(-1, "EoT", "End of Time")]
    public void DungeonLabelsPartialNamesAndInvalidIndexesDoNotGuess(
        int preset,
        string scene,
        string label
    )
    {
        Assert.Equal(-1, KeyTeleportTargetRules.MatchPriority(preset, scene, label));
    }

    [Theory]
    [InlineData("Dun1Q10", 1)]
    [InlineData("dun2q10", 2)]
    [InlineData("Dun3Q10", 3)]
    [InlineData("Dun1Q11", -1)]
    [InlineData("EoT", -1)]
    [InlineData(null, -1)]
    public void OldSavedLobbiesRedirectOnlyToTheirKnownCampaignPreset(string scene, int preset)
    {
        Assert.Equal(preset, KeyTeleportTargetRules.SavedDungeonPreset(scene));
    }
}
