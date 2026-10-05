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
        static IntPtr catalogPointer;
        static int catalogCount = -1;
        static void LoadCatalog()
        {
            var list = AffixList.get();
            if (list.IsNullOrDestroyed() || list.AllAffixes.IsNullOrDestroyed()) return;
            if (catalogLoaded && catalogPointer == list.Pointer && catalogCount == list.AllAffixes.Count) return;
            catalog.Clear();
            foreach (var definition in list.AllAffixes)
                if (!definition.IsNullOrDestroyed() && !catalog.Exists(a => a.affixId == definition.affixId))
                    catalog.Add(definition);
            catalogPointer = list.Pointer; catalogCount = list.AllAffixes.Count;
            catalogLoaded = catalog.Count > 0;
        }
        public static AffixList.Affix Definition(int id)
        {
            LoadCatalog();
            return catalog.Find(a => a.affixId == id);
        }
        public static bool IsChampion(AffixList.Affix definition)
        {
            if (definition.IsNullOrDestroyed() || definition.specialAffixType != AffixList.SpecialAffixType.Personal)
                return false;
            var champions = ChampionDataList.Instance;
            if (champions.IsNullOrDestroyed() || champions.mods.IsNullOrDestroyed()) return false;
            foreach (var mod in champions.mods)
                if (!mod.IsNullOrDestroyed() && mod.affixId == definition.affixId) return true;
            return false;
        }
        public static string PoolLabel(AffixList.Affix definition)
        {
            if (IsCorruption(definition)) return "Corruption-exclusive";
            if (IsChampion(definition)) return "Champion";
            if (definition.IsNullOrDestroyed()) return "None";
            switch (definition.specialAffixType)
            {
                case AffixList.SpecialAffixType.Standard: return "Standard";
                case AffixList.SpecialAffixType.Experimental: return "Experimental";
                case AffixList.SpecialAffixType.Set: return "Set";
                case AffixList.SpecialAffixType.Personal: return "Personal";
                case AffixList.SpecialAffixType.IdolEnchantment: return "Idol enchantment";
                case AffixList.SpecialAffixType.IdolWeaver: return "Weaver idol";
                default: return "Unique modifier";
            }
        }
        static bool MatchesOutcome(AffixList.Affix definition, CorruptionOutcome outcome)
        {
            switch (definition.specialAffixType)
            {
                case AffixList.SpecialAffixType.Corrupted:
                    return outcome == CorruptionOutcome.AddsCorruptedAffix
                        || outcome == CorruptionOutcome.AddsLowTierCorruptedAffix;
                case AffixList.SpecialAffixType.Standard:
                    return definition.uniqueId == 0 && (definition.type == AffixList.AffixType.PREFIX
                        || definition.type == AffixList.AffixType.SUFFIX)
                        && (outcome == CorruptionOutcome.AddStandardAffix || outcome == CorruptionOutcome.AddLowTierStandardAffix);
                case AffixList.SpecialAffixType.Personal:
                    return IsChampion(definition) && outcome == CorruptionOutcome.AddChampionAffix;
                case AffixList.SpecialAffixType.Experimental:
                    return outcome == CorruptionOutcome.AddExperimentalAffix;
                case AffixList.SpecialAffixType.Set:
                    return outcome == CorruptionOutcome.AddSetAffix;
                default: return false;
            }
        }
        static bool HasPositiveWeight(WeightedCorruptionOutcome outcome)
        {
            return !outcome.IsNullOrDestroyed() && outcome.weight > 0
                && (outcome.maximumChanceType.ToString() != "Custom" || outcome.maximumChance > 0);
        }
        static IEnumerable<WeightedCorruptionOutcome> LegalOutcomes(ItemDataUnpacked item)
        {
            // Fail closed if the live game cannot supply this item's configuration.
            if (!item.TryGetCorruptionConfig(out CorruptionCategoryConfig category)
                || category.IsNullOrDestroyed()) yield break;
            if (!category.positiveOutcomes.IsNullOrDestroyed())
                foreach (var outcome in category.positiveOutcomes)
                    if (HasPositiveWeight(outcome) && item.CorruptionOutcomeCanApplyToItem(outcome)) yield return outcome;
            if (category.cannotCombineWithType) yield break;
            var list = ItemList.get();
            if (list.IsNullOrDestroyed()) yield break;
            var config = list.GetCorruptionOutcomeConfig();
            if (config.IsNullOrDestroyed() || config.corruptionEquipmentTypeConfig.IsNullOrDestroyed()) yield break;
            var equipmentType = list.GetEquipmentTypeForBaseType(item.itemType);
            foreach (var equipment in config.corruptionEquipmentTypeConfig)
                if (!equipment.IsNullOrDestroyed() && equipment.type == equipmentType
                    && !equipment.positiveOutcomes.IsNullOrDestroyed())
                    foreach (var outcome in equipment.positiveOutcomes)
                        if (HasPositiveWeight(outcome) && item.CorruptionOutcomeCanApplyToItem(outcome)) yield return outcome;
        }
        static WeightedCorruptionOutcome LegalOutcome(ItemDataUnpacked item, AffixList.Affix definition, int tier)
        {
            if (!FitsItem(definition, item.itemType, item.subType)
                || tier < 0 || tier > 6 || definition.tiers.IsNullOrDestroyed()
                || tier >= definition.tiers.Count || !item.TierValidForCorruptionNoRestrictions(tier)) return null;
            foreach (var outcome in LegalOutcomes(item))
                if (MatchesOutcome(definition, outcome.corruptionOutcome)
                    && !outcome.replacesAffix && !outcome.tierWeights.IsNullOrDestroyed()
                    && tier < outcome.tierWeights.Length && outcome.tierWeights[tier] > 0) return outcome;
            return null;
        }
        public static int MaximumTier(ItemDataUnpacked item, AffixList.Affix definition, bool illegal)
        {
            if (definition.IsNullOrDestroyed()) return 0;
            if (illegal) return 8;
            try
            {
                for (int tier = 6; tier >= 0; tier--)
                    if (!LegalOutcome(item, definition, tier).IsNullOrDestroyed()) return tier + 1;
            }
            catch { /* Native configuration unavailable: hide this selection. */ }
            return 0;
        }
        static IntPtr validatedItem, validatedDefinition;
        static int validatedTier = -1;
        static bool validatedResult;
        public static bool SelectionAllowed(ItemDataUnpacked item, AffixList.Affix definition, int tier, bool illegal)
        {
            if (item.IsNullOrDestroyed() || definition.IsNullOrDestroyed()) return false;
            if (illegal) return tier >= 0 && tier <= 7;
            // The HUD validates every frame; the picker probe is immutable and replaced
            // when item/affix selections change. Actual drops receive a fresh native item.
            if (validatedItem == item.Pointer && validatedDefinition == definition.Pointer && validatedTier == tier)
                return validatedResult;
            bool allowed;
            try { allowed = !LegalOutcome(item, definition, tier).IsNullOrDestroyed(); }
            catch { allowed = false; }
            validatedItem = item.Pointer; validatedDefinition = definition.Pointer;
            validatedTier = tier; validatedResult = allowed;
            return allowed;
        }
        public static IEnumerable<AffixList.Affix> Catalog(bool illegal = false)
        {
            LoadCatalog();
            foreach (var definition in catalog)
                if (illegal || IsCorruption(definition) || definition.specialAffixType == AffixList.SpecialAffixType.Standard
                    || IsChampion(definition) || definition.specialAffixType == AffixList.SpecialAffixType.Set
                    || definition.specialAffixType == AffixList.SpecialAffixType.Experimental) yield return definition;
        }
        public static void Apply(ItemDataUnpacked item, int id, int tier, int roll, bool illegal = false)
        {
            if (!IsSupported) throw new InvalidOperationException("Chosen corrupted affixes are unsupported by this game's item wrappers");
            var definition = Definition(id);
            validatedItem = IntPtr.Zero;
            if (!SelectionAllowed(item, definition, tier, illegal))
                throw new InvalidOperationException("The corrupted affix cannot roll on this item");
            // Let the native constructor initialize item-type-dependent metadata.
            var affix = new ItemAffix((ushort)id, (byte)tier,
                (byte)Math.Max(0, Math.Min(255, roll)), item.itemType, Il2Cpp.SealedAffixType.FromCorruption);
            if (affix.affixId != id || !affix.IsSealedCorrupted ||
                affix.specialAffixType != definition.specialAffixType)
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
                AddUsingNativeCorruptionSlot(item, affix, definition, tier, illegal);
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
            VerifySelection(item, id, tier, roll, illegal);
            var restored = new ItemDataUnpacked(item.GetID());
            VerifySelection(restored, id, tier, roll, illegal);
            VerifyOriginalAffixes(restored, originalAffixes, id);
            if (restored.uniqueID != originalUniqueId || restored.legendaryPotential != originalLP
                || restored.weaversWill != originalWW || restored.hasSealedRegularAffix != originalRegularSeal
                || restored.hasSealedPrimordialAffix != originalPrimordialSeal)
                throw new InvalidOperationException("Corruption failed saved-byte round trip; no item was dropped");
        }
        static void AddUsingNativeCorruptionSlot(ItemDataUnpacked item, ItemAffix selected,
            AffixList.Affix definition, int tier, bool illegal)
        {
            if (item.hasSealedAffixFromCorruption)
                throw new InvalidOperationException("This item already has a corrupted affix; no item was dropped");
            var legalOutcome = illegal ? null : LegalOutcome(item, definition, tier);
            if (!illegal && legalOutcome.IsNullOrDestroyed())
                throw new InvalidOperationException("No legal corruption outcome for this selection; no item was dropped");
            var weights = new float[illegal ? 8 : 7];
            weights[tier] = 1;
            var outcome = new WeightedCorruptionOutcome
            {
                // Illegal mode reserves a native slot, then substitutes the requested definition.
                corruptionOutcome = illegal ? CorruptionOutcome.AddsCorruptedAffix : legalOutcome.corruptionOutcome,
                replacesAffix = false,
                tierWeights = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(
                    illegal ? new float[] { 1, 1, 1, 1, 1, 1, 1 } : weights)
            };
            // Do not use a forced unique-id match: that parameter filters unique
            // items, not affix ids. Replace only the native-created corruption slot.
            if (!item.AddRandomSpecialAffix(outcome, illegal ? AffixList.SpecialAffixType.Corrupted : definition.specialAffixType,
                false, 100, out int addedId, out _, !illegal && IsChampion(definition),
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
        public static void VerifySelection(ItemDataUnpacked item, int id, int tier, int roll, bool illegal = false)
        {
            VerifyStoredCorruption(item, id, "final packing");
            VerifySealFlags(item);
            item.TryGetSealedCorruptedAffixe(out ItemAffix saved);
            if (saved.affixTier != tier ||
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
                saved.specialAffixType != Definition(id)?.specialAffixType)
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

