using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Scripts.Mods.Items;

/// <summary>Everything one custom unique hands to the registrar.</summary>
public sealed class CustomUniqueDefinition
{
    public CustomUniqueSpec Spec { get; init; }

    /// <summary>Null when the item uses a base the game already has.</summary>
    public LocalizedText SubtypeName { get; init; }
    public LocalizedText UniqueName { get; init; }
    public LocalizedText Lore { get; init; }

    /// <summary>Language to text; null when the item has no description.</summary>
    public Func<string, string> Description { get; init; }
    public Func<CustomUniqueFlags> Flags { get; init; }

    /// <summary>Null when the item uses a base the game already has.</summary>
    public Func<Il2CppSystem.Collections.Generic.List<ItemList.EquipmentImplicit>> Implicits { get; init; }
    public Func<Il2CppSystem.Collections.Generic.List<UniqueItemMod>> Mods { get; init; }
    public Func<Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry>> TooltipEntries { get; init; }
}
