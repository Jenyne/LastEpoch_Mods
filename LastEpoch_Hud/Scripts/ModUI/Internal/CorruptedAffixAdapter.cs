using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // Capability checks against the running game wrappers. Never infer corruption from an id.
    public static class CorruptedAffixAdapter
    {
        static MemberInfo storage;
        static bool checkedStorage;
        static bool flagStorage;
        static readonly List<AffixList.Affix> catalog = new List<AffixList.Affix>();
        static bool catalogLoaded;

        public static bool IsSupported
        {
            get { FindStorage(); LoadCatalog(); return storage != null && catalog.Count > 0; }
        }
        static void FindStorage()
        {
            if (checkedStorage) return;
            checkedStorage = true;
            foreach (string name in new[] { "corruptedAffix", "CorruptedAffix" })
            {
                var property = typeof(ItemDataUnpacked).GetProperty(name);
                if (property != null && property.CanRead && property.CanWrite && property.PropertyType == typeof(ItemAffix)) { storage = property; return; }
                var field = typeof(ItemDataUnpacked).GetField(name);
                if (field != null && !field.IsInitOnly && field.FieldType == typeof(ItemAffix)) { storage = field; return; }
            }
            // Packed items carry a separate corrupted-affix presence flag and place that
            // affix after a sealed affix, before ordinary affixes. Require the live wrapper
            // to expose the matching writable flag before using this representation.
            foreach (string name in new[] { "hasSealedAffixFromCorruption" })
            {
                var property = typeof(ItemDataUnpacked).GetProperty(name);
                if (property != null && property.CanRead && property.CanWrite && property.PropertyType == typeof(bool)) { storage = property; flagStorage = true; return; }
                var field = typeof(ItemDataUnpacked).GetField(name);
                if (field != null && !field.IsInitOnly && field.FieldType == typeof(bool)) { storage = field; flagStorage = true; return; }
            }
        }
        public static bool IsCorruption(AffixList.Affix affix)
        {
            return !affix.IsNullOrDestroyed() &&
                affix.specialAffixType == AffixList.SpecialAffixType.Corrupted;
        }
        public static bool FitsItem(AffixList.Affix definition, int baseType, int subType)
        {
            if (definition.IsNullOrDestroyed() || baseType < 0 || subType < 0) return false;
            var list = ItemList.get();
            if (list.IsNullOrDestroyed()) return false;
            try
            {
                foreach (var type in list.EquippableItems)
                    if (type.baseTypeID == baseType)
                        foreach (var item in type.subItems)
                            if (item.subTypeID == subType)
                            {
                                if (!LastEpoch_Hud.Scripts.Mods.Items.Items_Req_Class.TryGetOriginalRequirement(
                                    baseType, subType, item.classRequirement, out var requiredClass)) return false;
                                if (requiredClass == ItemList.ClassRequirement.None) requiredClass = ItemList.ClassRequirement.Any;
                                return definition.CanRollOn(baseType, subType, requiredClass);
                            }
            }
            catch { return false; }
            // No permissive fallback for an unknown category or subtype.
            return false;
        }
        static void LoadCatalog()
        {
            if (catalogLoaded) return;
            catalog.Clear();
            var list = AffixList.get();
            if (list.IsNullOrDestroyed()) return;
            foreach (var a in list.singleAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            foreach (var a in list.multiAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            // AllAffixes includes definitions omitted by the editor's single/multi views.
            var all = list.AllAffixes;
            if (!all.IsNullOrDestroyed()) foreach (var definition in all)
                if (!definition.IsNullOrDestroyed() && IsCorruption(definition) &&
                    !catalog.Exists(x => x.affixId == definition.affixId)) catalog.Add(definition);
            // Also support separately exposed collections.
            foreach (string name in new[] { "specialAffixes", "corruptedAffixes" })
            {
                object collection = null;
                var property = typeof(AffixList).GetProperty(name);
                if (property != null && property.CanRead) collection = property.GetValue(list);
                var field = typeof(AffixList).GetField(name);
                if (field != null) collection = field.GetValue(list);
                if (collection == null) continue;
                var countProperty = collection.GetType().GetProperty("Count") ?? collection.GetType().GetProperty("Length");
                var indexer = collection.GetType().GetProperty("Item", new[] { typeof(int) });
                if (countProperty == null || indexer == null) continue;
                int count = (int)countProperty.GetValue(collection);
                if (count < 0 || count > 10000) continue;
                for (int i = 0; i < count; i++)
                    if (indexer.GetValue(collection, new object[] { i }) is AffixList.Affix definition &&
                        !definition.IsNullOrDestroyed() && IsCorruption(definition) &&
                        !catalog.Exists(x => x.affixId == definition.affixId)) catalog.Add(definition);
            }
            // A catalog can still be loading when the menu is first opened; retry until populated.
            catalogLoaded = catalog.Count > 0;
        }
        public static IEnumerable<AffixList.Affix> Catalog() { LoadCatalog(); return catalog; }
        public static void Apply(ItemDataUnpacked item, int id, int tier, int roll)
        {
            if (!IsSupported) throw new InvalidOperationException("Chosen corrupted affixes are unsupported by this game's item wrappers");
            AffixList.Affix definition = null;
            foreach (var a in catalog) if (a.affixId == id) { definition = a; break; }
            if (definition.IsNullOrDestroyed() || !FitsItem(definition, item.itemType, item.subType))
                throw new InvalidOperationException("The corrupted affix cannot roll on this item");
            // Let the native constructor initialize item-type-dependent metadata.
            var affix = new ItemAffix((ushort)id, (byte)Math.Max(0, Math.Min(6, tier)),
                (byte)Math.Max(0, Math.Min(255, roll)), item.itemType, Il2Cpp.SealedAffixType.FromCorruption);
            if (affix.affixId != id || !affix.IsSealedCorrupted ||
                affix.specialAffixType != AffixList.SpecialAffixType.Corrupted)
                throw new InvalidOperationException("Corruption constructor rejected affix " + id +
                    " (saved=" + affix.affixId + ", sealed=" + affix.sealedAffixType +
                    ", special=" + affix.specialAffixType + "); no item was dropped");
            var originalAffixes = SnapshotAffixes(item);
            bool originalRegularSeal = item.hasSealedRegularAffix;
            bool originalPrimordialSeal = item.hasSealedPrimordialAffix;
            ushort originalUniqueId = item.uniqueID;
            byte originalLP = item.legendaryPotential;
            byte originalWW = item.weaversWill;
            if (flagStorage)
            {
                // The native operation owns sealed-affix ordering, socket counts and
                // rarity-specific packing flags. Appending an affix manually causes
                // the unpacker to put the corruption seal on an ordinary affix.
                AddUsingNativeCorruptionSlot(item, affix);
            }
            else if (storage is PropertyInfo property) property.SetValue(item, affix);
            else ((FieldInfo)storage).SetValue(item, affix);
            VerifyStoredCorruption(item, id, "before packing");
            item.RefreshIDAndValues();
            VerifyStoredCorruption(item, id, "after packing");
            VerifyOriginalAffixes(item, originalAffixes, id);
            if (item.hasSealedRegularAffix != originalRegularSeal || item.hasSealedPrimordialAffix != originalPrimordialSeal)
                throw new InvalidOperationException("Corruption changed an existing seal flag; no item was dropped");
            if (item.uniqueID != originalUniqueId || item.legendaryPotential != originalLP || item.weaversWill != originalWW)
                throw new InvalidOperationException("Corruption changed unique item properties; no item was dropped");
            VerifySelection(item, id, tier, roll);
        }
        static void AddUsingNativeCorruptionSlot(ItemDataUnpacked item, ItemAffix selected)
        {
            if (item.hasSealedAffixFromCorruption)
                throw new InvalidOperationException("This item already has a corrupted affix; no item was dropped");
            var outcome = new WeightedCorruptionOutcome
            {
                corruptionOutcome = CorruptionOutcome.AddsCorruptedAffix,
                replacesAffix = false,
                tierWeights = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(
                    new float[] { 1, 1, 1, 1, 1, 1, 1 })
            };
            // Do not use a forced unique-id match: that parameter filters unique
            // items, not affix ids. Replace only the native-created corruption slot.
            if (!item.AddRandomSpecialAffix(outcome, AffixList.SpecialAffixType.Corrupted,
                false, 100, out int addedId, out _, false,
                new Il2CppSystem.Nullable<ushort>(), false))
                throw new InvalidOperationException("The game could not create a corrupted affix slot; no item was dropped");
            // The regular-seal output is not a prohibition on a second, corruption seal.
            // Validate the actual corruption and preserve existing seals independently.
            if (!item.TryGetSealedCorruptedAffixe(out ItemAffix generated) ||
                generated.IsNullOrDestroyed() || generated.affixId != addedId ||
                !generated.IsSealedCorrupted)
                throw new InvalidOperationException("The game did not create a sealed corruption slot; no item was dropped");
            int index = -1;
            for (int i = 0; i < item.affixes.Count; i++)
                if (!item.affixes[i].IsNullOrDestroyed() && item.affixes[i].IsSealedCorrupted)
                {
                    if (index >= 0) throw new InvalidOperationException("Multiple corrupted affix slots; no item was dropped");
                    index = i;
                }
            if (index < 0) throw new InvalidOperationException("Missing corrupted affix slot; no item was dropped");
            item.affixes[index] = selected;
        }
        static List<string> SnapshotAffixes(ItemDataUnpacked item)
        {
            var result = new List<string>();
            foreach (var affix in item.affixes)
            {
                if (affix.IsNullOrDestroyed()) throw new InvalidOperationException("Invalid existing affix; no item was dropped");
                result.Add(AffixSignature(affix));
            }
            result.Sort(StringComparer.Ordinal);
            return result;
        }
        static string AffixSignature(ItemAffix affix)
        {
            return affix.affixId + ":" + affix.affixTier + ":" + affix.affixRoll +
                ":" + affix.sealedAffixType + ":" + affix.specialAffixType;
        }
        static void VerifyOriginalAffixes(ItemDataUnpacked item, List<string> expected, int corruptionId)
        {
            var actual = new List<string>();
            foreach (var affix in item.affixes)
            {
                if (affix.IsNullOrDestroyed()) throw new InvalidOperationException("Invalid packed affix; no item was dropped");
                if (affix.affixId == corruptionId && affix.IsSealedCorrupted) continue;
                actual.Add(AffixSignature(affix));
            }
            actual.Sort(StringComparer.Ordinal);
            if (actual.Count != expected.Count)
                throw new InvalidOperationException("Corruption changed the existing affix count; no item was dropped");
            for (int i = 0; i < actual.Count; i++)
                if (actual[i] != expected[i])
                    throw new InvalidOperationException("Corruption changed an existing affix; no item was dropped");
        }
        public static void VerifySelection(ItemDataUnpacked item, int id, int tier, int roll)
        {
            VerifyStoredCorruption(item, id, "final packing");
            VerifySealFlags(item);
            item.TryGetSealedCorruptedAffixe(out ItemAffix saved);
            if (saved.affixTier != Math.Max(0, Math.Min(6, tier)) ||
                saved.affixRoll != Math.Max(0, Math.Min(255, roll)))
                throw new InvalidOperationException("Corruption tier or roll changed during packing; no item was dropped");
        }
        static void VerifySealFlags(ItemDataUnpacked item)
        {
            int regular = 0, primordial = 0, corruption = 0;
            foreach (var affix in item.affixes)
            {
                if (affix.IsNullOrDestroyed()) throw new InvalidOperationException("Invalid packed affix; no item was dropped");
                if (affix.IsSealedRegular) regular++;
                if (affix.IsSealedPrimordial) primordial++;
                if (affix.IsSealedCorrupted) corruption++;
            }
            if (regular != (item.hasSealedRegularAffix ? 1 : 0) ||
                primordial != (item.hasSealedPrimordialAffix ? 1 : 0) ||
                corruption != (item.hasSealedAffixFromCorruption ? 1 : 0))
                throw new InvalidOperationException("Packed seal flags do not match the affixes; no item was dropped");
        }
        static void VerifyStoredCorruption(ItemDataUnpacked item, int id, string stage)
        {
            // The generated game wrapper declares this parameter as out, not ref.
            bool recognized = item.TryGetSealedCorruptedAffixe(out ItemAffix saved);
            if (!recognized || saved.IsNullOrDestroyed() || saved.affixId != id || !saved.IsSealedCorrupted ||
                saved.specialAffixType != AffixList.SpecialAffixType.Corrupted)
            {
                string entries = "";
                foreach (var entry in item.affixes)
                    entries += entry.IsNullOrDestroyed() ? " null" :
                        " " + entry.affixId + ":" + entry.sealedAffixType + ":" + entry.specialAffixType;
                throw new InvalidOperationException("Corruption verification failed " + stage +
                    " (requested=" + id + ", accessor=" + recognized +
                    ", flag=" + item.hasSealedAffixFromCorruption + ", rarity=" + item.rarity +
                    ", sockets=" + item.sockets + ", affixes=" + entries + "); no item was dropped");
            }
        }
    }
}
