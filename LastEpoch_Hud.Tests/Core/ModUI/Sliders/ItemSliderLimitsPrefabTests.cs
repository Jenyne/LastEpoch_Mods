using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

public sealed class ItemSliderLimitsPrefabTests
{
    private static readonly string _prefabPath = Path.Combine(
        GameEnvironment.RepoRoot,
        "AssetBundleExport",
        "Assets",
        "Hud",
        "Prefab",
        "Hud.prefab"
    );

    [Theory]
    [InlineData(
        nameof(ItemSliderLimits.DropImplicits),
        "Slider_Items_Drop_Implicits_Min",
        "Slider_Items_Drop_Implicits_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropForgingPotential),
        "Slider_Items_Drop_ForginPotencial_Min",
        "Slider_Items_Drop_ForginPotencial_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropSealTier),
        "Slider_Items_Drop_SealTier_Min",
        "Slider_Items_Drop_SealTier_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropSealValue),
        "Slider_Items_Drop_SealValue_Min",
        "Slider_Items_Drop_SealValue_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropAffixCount),
        "Slider_Items_Drop_NbAffixes_Min",
        "Slider_Items_Drop_NbAffixes_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropAffixTiers),
        "Slider_Items_Drop_AffixesTiers_Min",
        "Slider_Items_Drop_AffixesTiers_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropAffixValues),
        "Slider_Items_Drop_AffixesValues_Min",
        "Slider_Items_Drop_AffixesValues_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropUniqueMods),
        "Slider_Items_Drop_UniqueMods_Min",
        "Slider_Items_Drop_UniqueMods_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropLegendaryPotential),
        "Slider_Items_Drop_LegendaryPotencial_Min",
        "Slider_Items_Drop_LegendaryPotencial_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.DropWeaverWill),
        "Slider_Items_Drop_WeaverWill_Min",
        "Slider_Items_Drop_WeaverWill_Max"
    )]
    [InlineData(
        nameof(ItemSliderLimits.AutoShatterChance),
        "Slider_Items_Pickup_AutoShatter_Chance",
        "Slider_Items_Pickup_AutoShatter_Chance"
    )]
    [InlineData(
        nameof(ItemSliderLimits.AutoShatterAffixChance),
        "Slider_Items_Pickup_AutoShatter_AffixChance",
        "Slider_Items_Pickup_AutoShatter_AffixChance"
    )]
    [InlineData(
        nameof(ItemSliderLimits.AutoShatterQuantityChance),
        "Slider_Items_Pickup_AutoShatter_QuantityChance",
        "Slider_Items_Pickup_AutoShatter_QuantityChance"
    )]
    public void Range_MatchesPrefab(string field, string minSlider, string maxSlider)
    {
        IReadOnlyDictionary<string, SliderRange> prefab = PrefabSliders.Load(_prefabPath);

        Assert.True(prefab.ContainsKey(minSlider), $"Prefab slider missing: {minSlider}");
        Assert.True(prefab.ContainsKey(maxSlider), $"Prefab slider missing: {maxSlider}");
        var expected = new SliderRange(
            prefab[minSlider].Min,
            prefab[maxSlider].Max,
            prefab[minSlider].WholeNumbers
        );
        SliderRange? actual = ItemSliderLimitsReflection.Find(field);
        Assert.True(actual.HasValue, $"Limits field missing: {field}");
        Assert.Equal(expected, actual.Value);
    }
}
