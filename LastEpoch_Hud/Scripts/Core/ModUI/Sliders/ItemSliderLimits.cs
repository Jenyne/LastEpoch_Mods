namespace LastEpoch_Hud.Scripts.Core.ModUI.Sliders;

/// <summary>Slider limits of the Items menu sliders, copied from the legacy prefab.</summary>
public static class ItemSliderLimits
{
    // Source: Slider_Items_Drop_Implicits_Min / _Max
    public static readonly SliderRange DropImplicits = new(0f, 255f, false);

    // Source: Slider_Items_Drop_ForginPotencial_Min / _Max
    public static readonly SliderRange DropForgingPotential = new(0f, 255f, true);

    // Source: Slider_Items_Drop_SealTier_Min / _Max
    public static readonly SliderRange DropSealTier = new(0f, 6f, true);

    // Source: Slider_Items_Drop_SealValue_Min / _Max
    public static readonly SliderRange DropSealValue = new(0f, 255f, false);

    // Source: Slider_Items_Drop_NbAffixes_Min / _Max
    public static readonly SliderRange DropAffixCount = new(0f, 6f, false);

    // Source: no prefab slider; former ItemsDropPage fallback (0..2, whole), commit 99141da
    public static readonly SliderRange DropIdolAffixCount = new(0f, 2f, true);

    // Source: Slider_Items_Drop_AffixesTiers_Min / _Max
    public static readonly SliderRange DropAffixTiers = new(0f, 6f, true);

    // Source: Slider_Items_Drop_AffixesValues_Min / _Max
    public static readonly SliderRange DropAffixValues = new(0f, 255f, false);

    // Source: Slider_Items_Drop_UniqueMods_Min / _Max
    public static readonly SliderRange DropUniqueMods = new(0f, 255f, false);

    // Source: Slider_Items_Drop_LegendaryPotencial_Min / _Max
    public static readonly SliderRange DropLegendaryPotential = new(0f, 4f, true);

    // Source: Slider_Items_Drop_WeaverWill_Min / _Max
    public static readonly SliderRange DropWeaverWill = new(0f, 28f, true);

    // Source: Slider_Items_Pickup_AutoShatter_Chance
    public static readonly SliderRange AutoShatterChance = new(0f, 100f, true);

    // Source: Slider_Items_Pickup_AutoShatter_AffixChance
    public static readonly SliderRange AutoShatterAffixChance = new(0f, 100f, true);

    // Source: Slider_Items_Pickup_AutoShatter_QuantityChance
    public static readonly SliderRange AutoShatterQuantityChance = new(0f, 100f, true);
}
