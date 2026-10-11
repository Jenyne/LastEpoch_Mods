using Il2CppLE.Services.Cosmetics;
using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics;

/// <summary>Reads the game's cosmetics registry into a Core catalog.</summary>
internal static class CosmeticRegistryReader
{
    public static CosmeticCatalog Read(CosmeticResources registry)
    {
        var catalog = new CosmeticCatalog();
        AddList(registry.ItemCosmetics, catalog);
        AddList(registry.AbilityCosmetics, catalog);
        AddList(registry.BackSlotCosmetics, catalog);
        AddList(registry.PortalCosmetics, catalog);
        AddList(registry.PetCosmetics, catalog);
        AddList(registry.PetEffectCosmetics, catalog);
        AddList(registry.PortraitCosmetics, catalog);
        AddList(registry.CharacterTitleCosmetics, catalog);
        AddList(registry.FootprintCosmetics, catalog);
        AddList(registry.CharacterFXCosmetics, catalog);
        AddList(registry.StashCosmetics, catalog);
        AddList(registry.CosmeticBundles, catalog);
        AddBundleContents(registry.CosmeticBundles, catalog);
        AddLookupPairs(registry.CosmeticLookupPairs, catalog);
        return catalog;
    }

    private static void AddList<T>(
        Il2CppSystem.Collections.Generic.List<T> list,
        CosmeticCatalog catalog
    )
        where T : Cosmetic
    {
        if (list.IsNullOrDestroyed())
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            T cosmetic = list[i];
            if (cosmetic.IsNullOrDestroyed())
            {
                continue;
            }
            catalog.Add(cosmetic.BackendID, cosmetic.DisplayName);
        }
    }

    private static void AddBundleContents(
        Il2CppSystem.Collections.Generic.List<CosmeticBundle> bundles,
        CosmeticCatalog catalog
    )
    {
        if (bundles.IsNullOrDestroyed())
        {
            return;
        }
        for (int i = 0; i < bundles.Count; i++)
        {
            CosmeticBundle bundle = bundles[i];
            if (bundle.IsNullOrDestroyed())
            {
                continue;
            }
            AddList(bundle.bundledCosmetics, catalog);
        }
    }

    private static void AddLookupPairs(
        Il2CppSystem.Collections.Generic.Dictionary<string, Cosmetic> pairs,
        CosmeticCatalog catalog
    )
    {
        if (pairs.IsNullOrDestroyed())
        {
            return;
        }
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Cosmetic> pair in pairs)
        {
            string displayName = pair.Value.IsNullOrDestroyed() ? null : pair.Value.DisplayName;
            catalog.Add(pair.Key, displayName);
        }
    }
}
