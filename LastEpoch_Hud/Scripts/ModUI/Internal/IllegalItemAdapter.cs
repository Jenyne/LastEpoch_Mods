using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // Set membership is derived from the saved native set affix, never a sidecar save.
    public static class IllegalItemAdapter
    {
        static IntPtr catalogPointer;
        static int catalogCount = -1;
        static readonly Dictionary<int, AffixList.Affix> definitions = new Dictionary<int, AffixList.Affix>();
        static readonly Dictionary<int, List<AffixList.Affix>> setPools = new Dictionary<int, List<AffixList.Affix>>();
        public static AffixList.Affix Definition(int id)
        {
            var list = AffixList.get();
            if (list.IsNullOrDestroyed() || list.AllAffixes.IsNullOrDestroyed()) return null;
            if (catalogPointer != list.Pointer || catalogCount != list.AllAffixes.Count)
            {
                catalogPointer = list.Pointer; catalogCount = list.AllAffixes.Count;
                definitions.Clear(); setPools.Clear();
                foreach (var definition in list.AllAffixes)
                    if (!definition.IsNullOrDestroyed()) definitions[definition.affixId] = definition;
            }
            return definitions.TryGetValue(id, out var result) ? result : null;
        }
        public static bool IsSetAffix(AffixList.Affix definition)
        {
            return !definition.IsNullOrDestroyed() && definition.specialAffixType == AffixList.SpecialAffixType.Set;
        }
        public static bool SupportsUnsealedT8(AffixList.Affix definition)
        {
            return !definition.IsNullOrDestroyed() &&
                definition.specialAffixType == AffixList.SpecialAffixType.Standard &&
                !definition.tiers.IsNullOrDestroyed() && definition.tiers.Count >= 8;
        }
        public static byte SafeUnsealedTier(int id)
        {
            var definition = Definition(id);
            // Do not downgrade a saved T8 while catalogs are still loading.
            if (definition.IsNullOrDestroyed() || definition.tiers.IsNullOrDestroyed() ||
                definition.tiers.Count == 0) return 7;
            return SupportsUnsealedT8(definition) ? (byte)7 : (byte)6;
        }
        static UniqueList.Entry SourcePiece(AffixList.Affix definition)
        {
            if (!IsSetAffix(definition) || UniqueList.instance.IsNullOrDestroyed()) return null;
            var source = UniqueList.getUnique(definition.uniqueId);
            return !source.IsNullOrDestroyed() && source.isSetItem ? source : null;
        }
        public static List<AffixList.Affix> SetCatalog(int baseType)
        {
            var result = new List<AffixList.Affix>();
            if (UniqueList.instance.IsNullOrDestroyed()) return result;
            Definition(0); // Refresh the catalog cache if the native data changed.
            if (setPools.TryGetValue(baseType, out var cached)) return cached;
            foreach (var definition in definitions.Values)
            {
                var source = SourcePiece(definition);
                if (!source.IsNullOrDestroyed() && source.baseType == baseType &&
                    !result.Exists(x => x.affixId == definition.affixId)) result.Add(definition);
            }
            if (result.Count > 0) setPools[baseType] = result;
            return result;
        }
        public static string SetPieceName(AffixList.Affix definition)
        {
            var source = SourcePiece(definition);
            if (source.IsNullOrDestroyed()) return "None";
            try
            {
                string localized = Il2Cpp.Localization.Items.GetUniqueName(source.uniqueID, true, false);
                if (!string.IsNullOrWhiteSpace(localized)) return localized;
            }
            catch { }
            return string.IsNullOrWhiteSpace(source.displayName) ? source.name : source.displayName;
        }
        static bool ForeignSetPiece(ItemData item, out UniqueList.Entry source)
        {
            source = null;
            if (item.IsNullOrDestroyed() || item.affixes.IsNullOrDestroyed() ||
                item.rarity < 7 || UniqueList.instance.IsNullOrDestroyed()) return false;
            var own = UniqueList.getUnique(item.uniqueID);
            if (own.IsNullOrDestroyed() || own.isSetItem) return false;
            foreach (var affix in item.affixes)
            {
                if (affix.IsNullOrDestroyed()) return false;
                if (affix.specialAffixType != AffixList.SpecialAffixType.Set) continue;
                var piece = SourcePiece(Definition(affix.affixId));
                if (piece.IsNullOrDestroyed() || piece.baseType != item.itemType ||
                    !source.IsNullOrDestroyed()) return false;
                source = piece;
            }
            return !source.IsNullOrDestroyed();
        }
        public static void ApplySetPiece(ItemDataUnpacked item, int id)
        {
            var own = UniqueList.getUnique(item.uniqueID);
            var definition = Definition(id);
            var source = SourcePiece(definition);
            if (own.IsNullOrDestroyed() || own.isSetItem || !item.isUniqueOrLegendary() ||
                source.IsNullOrDestroyed() || source.baseType != item.itemType)
                throw new InvalidOperationException("The set modifier does not match this unique's equipment category");
            foreach (var existing in item.affixes)
                if (existing.IsNullOrDestroyed() || existing.specialAffixType == AffixList.SpecialAffixType.Set)
                    throw new InvalidOperationException("Only one set-piece modifier is supported per item");
            var affix = new ItemAffix((ushort)id, 0, 255, item.itemType, SealedAffixType.None);
            if (affix.affixId != id || affix.specialAffixType != AffixList.SpecialAffixType.Set ||
                affix.sealedAffixType != SealedAffixType.None)
                throw new InvalidOperationException("The native constructor rejected the set-piece modifier");
            ushort uniqueId = item.uniqueID;
            byte rarity = item.rarity, lp = item.legendaryPotential, ww = item.weaversWill;
            var originals = new List<string>();
            foreach (var existing in item.affixes) originals.Add(Signature(existing));
            originals.Sort(StringComparer.Ordinal);
            item.affixes.Add(affix);
            item.sockets = (byte)item.affixes.Count;
            item.RefreshIDAndValues();
            VerifySetPiece(item, id);
            var remaining = new List<string>();
            foreach (var saved in item.affixes)
                if (saved.affixId != id) remaining.Add(Signature(saved));
            remaining.Sort(StringComparer.Ordinal);
            if (remaining.Count != originals.Count)
                throw new InvalidOperationException("Adding the set modifier changed an existing affix; no item was dropped");
            for (int index = 0; index < originals.Count; index++)
                if (originals[index] != remaining[index])
                    throw new InvalidOperationException("Adding the set modifier changed an existing affix; no item was dropped");
            if (item.uniqueID != uniqueId || item.rarity != rarity ||
                item.legendaryPotential != lp || item.weaversWill != ww)
                throw new InvalidOperationException("Adding the set modifier changed the unique item; no item was dropped");
        }
        static string Signature(ItemAffix affix)
        {
            return affix.affixId + ":" + affix.affixTier + ":" + affix.affixRoll + ":" +
                affix.sealedAffixType + ":" + affix.specialAffixType;
        }
        public static void VerifySetPiece(ItemDataUnpacked item, int id)
        {
            int count = 0;
            foreach (var affix in item.affixes)
                if (!affix.IsNullOrDestroyed() && affix.affixId == id &&
                    affix.specialAffixType == AffixList.SpecialAffixType.Set &&
                    affix.sealedAffixType == SealedAffixType.None &&
                    affix.affixTier == 0 && affix.affixRoll == 255) count++;
            if (count != 1 || !ForeignSetPiece(item, out var source) ||
                item.getSetItemUniqueId() != source.uniqueID || !item.grantsSetBonus() ||
                item.SetId != source.setID)
                throw new InvalidOperationException("Set-piece storage or membership verification failed; no item was dropped");
        }
        static HashSet<ushort> EquippedSetPieces(ItemContainersManager manager, byte setId, out bool hasForeign)
        {
            var pieces = new HashSet<ushort>();
            hasForeign = false;
            if (manager.IsNullOrDestroyed() || manager.equipment.IsNullOrDestroyed() ||
                manager.equipment.Containers.IsNullOrDestroyed() || UniqueList.instance.IsNullOrDestroyed()) return pieces;
            foreach (var container in manager.equipment.Containers)
            {
                if (container.IsNullOrDestroyed() || !container.TryGetContentItemData(out ItemData item) ||
                    item.IsNullOrDestroyed()) continue;
                if (ForeignSetPiece(item, out var source))
                {
                    if (source.setID == setId) { pieces.Add(source.uniqueID); hasForeign = true; }
                }
                else if (item.grantsSetBonus())
                {
                    var nativeSource = UniqueList.getUnique(item.getSetItemUniqueId());
                    if (!nativeSource.IsNullOrDestroyed() && nativeSource.isSetItem &&
                        nativeSource.setID == setId) pieces.Add(nativeSource.uniqueID);
                }
            }
            return pieces;
        }
        [HarmonyPatch(typeof(ItemContainersManager), nameof(ItemContainersManager.getGearCountForSetID))]
        static class EquippedSetCount
        {
            [HarmonyPostfix]
            static void Postfix(ItemContainersManager __instance, byte __0, ref int __result)
            {
                var pieces = EquippedSetPieces(__instance, __0, out bool hasForeign);
                if (!hasForeign) return;
                // Preserve the existing explicit "remove set requirements" feature.
                if (!Save_Manager.instance.IsNullOrDestroyed() && Save_Manager.instance.initialized &&
                    Save_Manager.instance.data.Items.Req.set) return;
                __result = pieces.Count;
            }
        }
        [HarmonyPatch(typeof(ItemContainersManager), nameof(ItemContainersManager.IsSetItemEquipped))]
        static class EquippedSetPiece
        {
            [HarmonyPostfix]
            static void Postfix(ItemContainersManager __instance, ushort __0, ref bool __result)
            {
                if (__result || UniqueList.instance.IsNullOrDestroyed()) return;
                var source = UniqueList.getUnique(__0);
                if (source.IsNullOrDestroyed() || !source.isSetItem) return;
                var pieces = EquippedSetPieces(__instance, source.setID, out bool hasForeign);
                if (hasForeign && pieces.Contains(__0)) __result = true;
            }
        }
        [HarmonyPatch(typeof(ItemDataUnpacked), "AfterIDChange")]
        static class PackedSetIdentity
        {
            [HarmonyPostfix]
            static void Postfix(ItemDataUnpacked __instance)
            {
                // Native equipment code can read the cached field directly.
                if (ForeignSetPiece(__instance, out var source)) __instance.SetId = source.setID;
            }
        }
        // These recognition patches are independent of the creation toggle so an
        // already-created item's bonuses continue working after reload or mode changes.
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.grantsSetBonus))]
        static class GrantsSetBonus
        {
            [HarmonyPostfix]
            static void Postfix(ItemData __instance, ref bool __result)
            {
                if (ForeignSetPiece(__instance, out _)) __result = true;
            }
        }
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.getSetItemUniqueId))]
        static class SetPieceIdentity
        {
            [HarmonyPostfix]
            static void Postfix(ItemData __instance, ref ushort __result)
            {
                if (ForeignSetPiece(__instance, out var source)) __result = source.uniqueID;
            }
        }
        [HarmonyPatch(typeof(ItemDataUnpacked), "get_SetId")]
        static class SetIdentity
        {
            [HarmonyPostfix]
            static void Postfix(ItemDataUnpacked __instance, ref int __result)
            {
                if (ForeignSetPiece(__instance, out var source)) __result = source.setID;
            }
        }
    }
}
