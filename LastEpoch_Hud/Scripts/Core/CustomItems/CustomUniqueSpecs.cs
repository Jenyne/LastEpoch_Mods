using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>The custom uniques the mod adds to the game.</summary>
public static class CustomUniqueSpecs
{
    public static readonly CustomUniqueSpec Headhunter = new()
    {
        Name = "Headhunter",
        UniqueId = 500,
        BaseType = 2, // Belt
        BaseId = CustomUniqueSpec.AllocateBaseId,
        AddsBase = true,
        LevelRequirement = 40,
        OverrideLevelRequirement = true,
        EffectiveLevelForLegendaryPotential = 0,
    };

    public static readonly CustomUniqueSpec Mjolner = new()
    {
        Name = "Mjolner",
        UniqueId = 501,
        BaseType = 7, // Mace
        BaseId = 10, // Rune hammer
        AddsBase = false,
        LevelRequirement = 78,
        OverrideLevelRequirement = false,
        EffectiveLevelForLegendaryPotential = 60,
    };

    public static readonly CustomUniqueSpec SandsOfSilk = new()
    {
        Name = "Sands of Silk",
        UniqueId = 502,
        BaseType = 1, // Body armor
        BaseId = 71,
        AddsBase = true,
        LevelRequirement = 16,
        OverrideLevelRequirement = true,
        EffectiveLevelForLegendaryPotential = 0,
    };

    public static readonly CustomUniqueSpec EssentiaSanguis = new()
    {
        Name = "Essentia Sanguis",
        UniqueId = 503,
        BaseType = 4, // Gloves
        BaseId = 15,
        AddsBase = true,
        LevelRequirement = 52,
        OverrideLevelRequirement = true,
        EffectiveLevelForLegendaryPotential = 0,
    };

    public static readonly IReadOnlyList<CustomUniqueSpec> All = new[]
    {
        Headhunter,
        Mjolner,
        SandsOfSilk,
        EssentiaSanguis,
    };
}
