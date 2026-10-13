using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

public sealed class CharacterSliderLimitsPrefabTests
{
    [Theory]
    [InlineData(nameof(CharacterSliderLimits.FactionFavor), "Slider_Character_Factions_Favor")]
    [InlineData(nameof(CharacterSliderLimits.FactionRank), "Slider_Character_Factions_Rank")]
    [InlineData(
        nameof(CharacterSliderLimits.FactionReputation),
        "Slider_Character_Factions_Reputation"
    )]
    public void FactionRange_MatchesPrefab(string field, string slider)
    {
        IReadOnlyDictionary<string, SliderRange> prefab = PrefabSliders.Load(
            PrefabSliders.HudPrefabPath
        );

        Assert.True(prefab.ContainsKey(slider), $"Prefab slider missing: {slider}");
        SliderRange? actual = SliderLimitsReflection.Find(typeof(CharacterSliderLimits), field);
        Assert.True(actual.HasValue, $"Limits field missing: {field}");
        Assert.Equal(prefab[slider], actual.Value);
    }
}
