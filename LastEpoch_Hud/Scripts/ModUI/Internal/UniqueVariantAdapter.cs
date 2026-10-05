using System;
using System.Collections.Generic;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // Variant ids come exclusively from the selected unique's native fixed pool.
    public static class UniqueVariantAdapter
    {
        public static bool IsUnsated(UniqueList.Entry entry)
        {
            return !entry.IsNullOrDestroyed() &&
                string.Equals((entry.name ?? "").Replace(" ", "").Replace("_", ""),
                    "UnsatedRage", StringComparison.OrdinalIgnoreCase);
        }
        public static bool HasSingleVariant(UniqueList.Entry entry)
        {
            // These flags also govern legendary drop generation; they are not a
            // capability test for selecting a variant from an explicit native pool.
            return IsUnsated(entry) &&
                !entry.droppableLegendaryAffixes.IsNullOrDestroyed() &&
                entry.droppableLegendaryAffixes.Count > 0;
        }
        public static bool IsVariant(AffixList.Affix definition)
        {
            if (definition.IsNullOrDestroyed()) return false;
            if (definition.specialAffixType == AffixList.SpecialAffixType.FakeUniqueMod) return true;
            var uniques = UniqueList.instance;
            if (uniques.IsNullOrDestroyed()) return false;
            foreach (var entry in uniques.uniques)
                if (HasSingleVariant(entry) && entry.droppableLegendaryAffixes.Contains(definition.affixId))
                    return true;
            return false;
        }
        public static List<AffixList.Affix> Catalog(UniqueList.Entry entry)
        {
            var result = new List<AffixList.Affix>();
            var list = AffixList.get();
            if (!HasSingleVariant(entry) || list.IsNullOrDestroyed() ||
                list.AllAffixes.IsNullOrDestroyed()) return result;
            foreach (var id in entry.droppableLegendaryAffixes)
                foreach (var definition in list.AllAffixes)
                    if (!definition.IsNullOrDestroyed() && definition.affixId == id &&
                        !CorruptedAffixAdapter.IsCorruption(definition) &&
                        !result.Exists(a => a.affixId == id))
                    { result.Add(definition); break; }
            return result;
        }
        public static void Apply(ItemDataUnpacked item, int id)
        {
            var entry = UniqueList.getUnique(item.uniqueID);
            var catalog = Catalog(entry);
            if (!HasSingleVariant(entry) || item.itemType != entry.baseType ||
                !entry.subTypes.Contains((byte)item.subType) || !item.isUniqueOrLegendary())
                throw new InvalidOperationException("Unsated Rage variant does not match this item");
            AffixList.Affix definition = catalog.Find(a => a.affixId == id);
            if (definition.IsNullOrDestroyed())
                throw new InvalidOperationException("Chosen Rage is not in this ring's native fixed pool");
            foreach (var existing in item.affixes)
                if (existing.specialAffixType == AffixList.SpecialAffixType.FakeUniqueMod ||
                    entry.droppableLegendaryAffixes.Contains(existing.affixId))
                    throw new InvalidOperationException("The ring already has a variant modifier");
            var affix = new ItemAffix((ushort)id, 0, 255, item.itemType, SealedAffixType.None);
            if (affix.affixId != id || affix.specialAffixType != definition.specialAffixType ||
                affix.sealedAffixType != SealedAffixType.None)
                throw new InvalidOperationException("Native Rage constructor rejected the variant");
            byte rarity = item.rarity, potential = item.legendaryPotential;
            // Keep the native definition's special type and a non-sealed variant.
            // Adding the ring's fixed-pool modifier does not spend LP or change rarity.
            item.affixes.Add(affix);
            item.sockets = (byte)item.affixes.Count;
            item.RefreshIDAndValues();
            int count = 0;
            foreach (var saved in item.affixes)
                if (saved.affixId == id && saved.specialAffixType == definition.specialAffixType &&
                    saved.sealedAffixType == SealedAffixType.None) count++;
            if (count != 1 || item.rarity != rarity || item.legendaryPotential != potential ||
                item.sockets != item.affixes.Count)
                throw new InvalidOperationException("Rage storage verification failed; no item was dropped");
        }
    }
}
