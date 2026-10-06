using System;
using System.Collections.Generic;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI;

// Variant ids come exclusively from the selected unique's native fixed pool.
public static class UniqueVariantAdapter
{
    public static bool IsUnsated(UniqueList.Entry entry)
    {
        return !entry.IsNullOrDestroyed()
            && string.Equals(
                (entry.name ?? "").Replace(" ", "").Replace("_", ""),
                "UnsatedRage",
                StringComparison.OrdinalIgnoreCase
            );
    }

    public static int VariantCount(UniqueList.Entry entry)
    {
        if (IsUnsated(entry))
            return 1;
        if (
            !entry.IsNullOrDestroyed()
            && string.Equals(
                (entry.name ?? "").Replace(" ", "").Replace("_", ""),
                "WithstandtheElements",
                StringComparison.OrdinalIgnoreCase
            )
        )
            return 2;
        return 0;
    }

    public static bool HasVariants(UniqueList.Entry entry)
    {
        int count = VariantCount(entry);
        return count > 0
            && !entry.droppableLegendaryAffixes.IsNullOrDestroyed()
            && entry.droppableLegendaryAffixes.Count >= count;
    }

    public static bool HasSingleVariant(UniqueList.Entry entry)
    {
        // These flags also govern legendary drop generation; they are not a
        // capability test for selecting a variant from an explicit native pool.
        return VariantCount(entry) == 1 && HasVariants(entry);
    }

    public static bool IsVariant(AffixList.Affix definition)
    {
        if (definition.IsNullOrDestroyed())
            return false;
        if (definition.specialAffixType == AffixList.SpecialAffixType.FakeUniqueMod)
            return true;
        var uniques = UniqueList.instance;
        if (uniques.IsNullOrDestroyed())
            return false;
        foreach (var entry in uniques.uniques)
            if (
                VariantCount(entry) > 0
                && !entry.droppableLegendaryAffixes.IsNullOrDestroyed()
                && entry.droppableLegendaryAffixes.Contains(definition.affixId)
            )
                return true;
        return false;
    }

    public static List<AffixList.Affix> Catalog(UniqueList.Entry entry)
    {
        var result = new List<AffixList.Affix>();
        var list = AffixList.get();
        if (!HasVariants(entry) || list.IsNullOrDestroyed() || list.AllAffixes.IsNullOrDestroyed())
            return result;
        foreach (var id in entry.droppableLegendaryAffixes)
        foreach (var definition in list.AllAffixes)
            if (
                !definition.IsNullOrDestroyed()
                && definition.affixId == id
                && !CorruptedAffixAdapter.IsCorruption(definition)
                && !result.Exists(a => a.affixId == id)
            )
            {
                result.Add(definition);
                break;
            }
        return result;
    }

    public static void Apply(ItemDataUnpacked item, params int[] ids)
    {
        var entry = UniqueList.getUnique(item.uniqueID);
        var catalog = Catalog(entry);
        if (
            !HasVariants(entry)
            || item.itemType != entry.baseType
            || !entry.subTypes.Contains((byte)item.subType)
            || !item.isUniqueOrLegendary()
        )
            throw new InvalidOperationException("Unique variant does not match this item");
        if (ids == null || ids.Length != VariantCount(entry))
            throw new InvalidOperationException("Choose every exclusive unique modifier");
        var selected = new HashSet<int>();
        var additions = new List<ItemAffix>();
        foreach (int id in ids)
        {
            var definition = catalog.Find(a => a.affixId == id);
            if (definition.IsNullOrDestroyed() || !selected.Add(id))
                throw new InvalidOperationException(
                    "Choose distinct modifiers from this unique's native fixed pool"
                );
            var affix = new ItemAffix((ushort)id, 0, 255, item.itemType, SealedAffixType.None);
            if (
                affix.affixId != id
                || affix.specialAffixType != definition.specialAffixType
                || affix.sealedAffixType != SealedAffixType.None
            )
                throw new InvalidOperationException(
                    "Native unique variant constructor rejected the modifier"
                );
            additions.Add(affix);
        }
        var original = new List<string>();
        foreach (var existing in item.affixes)
        {
            if (existing.IsNullOrDestroyed())
                throw new InvalidOperationException("Invalid existing affix; no item was dropped");
            if (
                existing.specialAffixType == AffixList.SpecialAffixType.FakeUniqueMod
                || entry.droppableLegendaryAffixes.Contains(existing.affixId)
            )
                throw new InvalidOperationException("The item already has a variant modifier");
            original.Add(Signature(existing));
        }
        original.Sort(StringComparer.Ordinal);
        byte rarity = item.rarity,
            potential = item.legendaryPotential,
            weaversWill = item.weaversWill;
        ushort uniqueId = item.uniqueID;
        // Variants are additional fixed-pool modifiers, not LP-transferred affixes.
        // Add both glove variants before packing, without spending LP.
        foreach (var affix in additions)
            item.affixes.Add(affix);
        item.sockets = (byte)item.affixes.Count;
        item.RefreshIDAndValues();
        VerifySelection(item, ids);
        var remaining = new List<string>();
        foreach (var saved in item.affixes)
            if (!selected.Contains(saved.affixId))
                remaining.Add(Signature(saved));
        remaining.Sort(StringComparer.Ordinal);
        if (
            item.rarity != rarity
            || item.legendaryPotential != potential
            || item.weaversWill != weaversWill
            || item.uniqueID != uniqueId
            || item.sockets != item.affixes.Count
            || remaining.Count != original.Count
        )
            throw new InvalidOperationException(
                "Unique variant storage verification failed; no item was dropped"
            );
        for (int i = 0; i < original.Count; i++)
            if (original[i] != remaining[i])
                throw new InvalidOperationException(
                    "Unique variant changed an existing affix; no item was dropped"
                );
    }

    static string Signature(ItemAffix affix)
    {
        return affix.affixId
            + ":"
            + affix.affixTier
            + ":"
            + affix.affixRoll
            + ":"
            + affix.sealedAffixType
            + ":"
            + affix.specialAffixType;
    }

    public static void VerifySelection(ItemDataUnpacked item, params int[] ids)
    {
        var entry = UniqueList.getUnique(item.uniqueID);
        var catalog = Catalog(entry);
        if (!HasVariants(entry) || ids == null || ids.Length != VariantCount(entry))
            throw new InvalidOperationException(
                "Invalid unique variant selection; no item was dropped"
            );
        var expected = new HashSet<int>(ids);
        if (expected.Count != ids.Length)
            throw new InvalidOperationException("Duplicate unique variants; no item was dropped");
        int variantCount = 0;
        foreach (var saved in item.affixes)
        {
            if (saved.IsNullOrDestroyed())
                throw new InvalidOperationException("Invalid packed affix; no item was dropped");
            if (
                saved.specialAffixType != AffixList.SpecialAffixType.FakeUniqueMod
                && !entry.droppableLegendaryAffixes.Contains(saved.affixId)
            )
                continue;
            var definition = catalog.Find(a => a.affixId == saved.affixId);
            if (
                !expected.Remove(saved.affixId)
                || definition.IsNullOrDestroyed()
                || saved.specialAffixType != definition.specialAffixType
                || saved.sealedAffixType != SealedAffixType.None
                || saved.affixTier != 0
                || saved.affixRoll != 255
            )
                throw new InvalidOperationException(
                    "Unique variant packing changed a selected modifier; no item was dropped"
                );
            variantCount++;
        }
        if (variantCount != ids.Length || expected.Count != 0)
            throw new InvalidOperationException(
                "Unique variant missing after packing; no item was dropped"
            );
    }
}
