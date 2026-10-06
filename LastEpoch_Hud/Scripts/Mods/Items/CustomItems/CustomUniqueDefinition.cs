using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems;

/// <summary>Everything one custom unique hands to the registrar.</summary>
public sealed class CustomUniqueDefinition
{
    public CustomUniqueSpec Spec { get; init; }

    /// <summary>Null when the item uses a base the game already has.</summary>
    public string SubtypeNameKey { get; init; }
    public string UniqueNameKey { get; init; }
    public string LoreKey { get; init; }

    /// <summary>Mod texts to description; null when the item has no description.</summary>
    public Func<IReadOnlyDictionary<string, string>, string> Description { get; init; }
    public Func<CustomUniqueFlags> Flags { get; init; }

    /// <summary>Null when the item uses a base the game already has.</summary>
    public Func<Il2CppSystem.Collections.Generic.List<ItemList.EquipmentImplicit>> Implicits { get; init; }
    public Func<Il2CppSystem.Collections.Generic.List<UniqueItemMod>> Mods { get; init; }
    public Func<Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry>> TooltipEntries { get; init; }
}
