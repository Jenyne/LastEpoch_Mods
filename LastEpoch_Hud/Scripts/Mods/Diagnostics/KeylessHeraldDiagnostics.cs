using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Dungeons;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Diagnostics
{
    // Temporary, bounded probes. Never alter entry outcomes, inventory or item catalogs.
    internal static class KeylessHeraldDiagnostics
    {
        static readonly Dictionary<string, int> hits = new Dictionary<string, int>();
        static bool scanned;
        static bool Enabled()
        {
            return Scenes.IsGameScene() && !Save_Manager.instance.IsNullOrDestroyed()
                && !Save_Manager.instance.data.IsNullOrDestroyed()
                && Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey;
        }
        static void Trace(string key, string details)
        {
            if (!Enabled()) return;
            hits.TryGetValue(key, out int count);
            if (count >= 12) return;
            hits[key] = count + 1;
            Main.logger_instance?.Msg("[KeylessHerald] " + key + " " + details);
        }
        static void Scan()
        {
            if (scanned || !Enabled()) return;
            var list = ItemList.get(); var uniques = UniqueList.get();
            if (list.IsNullOrDestroyed() || uniques.IsNullOrDestroyed()) return;
            scanned = true;
            try
            {
                bool found = false;
                foreach (var entry in uniques.uniques)
                    if (!entry.IsNullOrDestroyed() && entry.uniqueID == 504)
                    {
                        found = true;
                        Trace("LegacyUnique504", "name=" + entry.name + "; base=" + entry.baseType);
                    }
                if (!found) Trace("LegacyUnique504", "not registered");
                int heralds = 0;
                foreach (var entry in uniques.uniques)
                    if (!entry.IsNullOrDestroyed() && !string.IsNullOrEmpty(entry.name)
                        && entry.name.IndexOf("Herald", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        heralds++;
                        Trace("RegisteredHerald", "id=" + entry.uniqueID + "; name=" + entry.name + "; base=" + entry.baseType);
                    }
                Trace("RegisteredHeralds", "matching items=" + heralds);
                found = false;
                foreach (var type in list.EquippableItems)
                    if (type.baseTypeID == 25)
                        foreach (var subtype in type.subItems)
                            if (subtype.subTypeID == 3)
                            {
                                found = true;
                                Trace("LegacyBase25Subtype3", "name=" + subtype.name);
                            }
                if (!found) Trace("LegacyBase25Subtype3", "not registered");
                int assets = 0;
                if (!Hud_Manager.asset_bundle.IsNullOrDestroyed())
                    foreach (var path in Hud_Manager.asset_bundle.GetAllAssetNames())
                        if (path.IndexOf("herald", StringComparison.OrdinalIgnoreCase) >= 0)
                        { assets++; Trace("HeraldAsset", path); }
                Trace("HeraldAssets", "matching assets=" + assets);
                int abilities = 0;
                foreach (var ability in Resources.FindObjectsOfTypeAll<Ability>())
                    if (!ability.IsNullOrDestroyed() && (ability.abilityName == "Avalanche" || ability.abilityName == "Maggot Explosion"))
                    {
                        abilities++;
                        Trace("HeraldAbility", "name=" + ability.abilityName + "; hasPrefabReference=" + !ability.abilityPrefabSoftRef.IsNullOrDestroyed());
                    }
                Trace("HeraldAbilities", "matching loaded abilities=" + abilities);
            }
            catch (Exception ex) { Trace("MetadataScan", ex.GetType().Name + ": " + ex.Message); }
        }
        [HarmonyPatch]
        static class EntryPath
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (string name in new[] { "Open", "ProceedToTierSelection", "EnterDungeonClick", "SelectTier" })
                    yield return AccessTools.Method(typeof(DungeonEnterPanelUI), name);
                foreach (string name in new[] { "ReceiveAttemptEntry", "AttemptEntry", "AttemptTierSelection" })
                    yield return AccessTools.Method(typeof(DungeonLobby), name);
                yield return AccessTools.Method(typeof(ClientDungeonService), "StartRun");
            }
            [HarmonyPrefix]
            static void Prefix(MethodBase __originalMethod)
            {
                if (!Enabled()) return;
                Scan();
                Trace(__originalMethod.DeclaringType.Name + "." + __originalMethod.Name, "called");
            }
            [HarmonyFinalizer]
            static void Finalizer(MethodBase __originalMethod, Exception __exception)
            {
                if (__exception != null) Trace(__originalMethod.Name + ".exception", __exception.GetType().Name + ": " + __exception.Message);
            }
        }
        [HarmonyPatch(typeof(ItemContainersManager), "CanAffordKeyCost")]
        static class KeyCost
        {
            [HarmonyPostfix, HarmonyPriority(Priority.First)]
            static void Postfix(ushort __0, int __1, bool __result)
            {
                Trace("CanAffordKeyCost", "subtype=" + __0 + "; amount=" + __1 + "; native result=" + __result);
            }
        }
        [HarmonyPatch(typeof(ItemContainersManager), "IsOccupiedWithValidDungeonKey")]
        static class ValidKey
        {
            [HarmonyPostfix, HarmonyPriority(Priority.First)]
            static void Postfix(DungeonID __0, bool __result)
            {
                Trace("IsOccupiedWithValidDungeonKey", "dungeon=" + __0 + "; native result before default-priority patches=" + __result);
            }
        }
        [HarmonyPatch]
        static class Removal
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(ItemContainer), "TryRemoveItem");
                yield return AccessTools.Method(typeof(ItemContainer), "RemoveItem");
            }
            [HarmonyPrefix]
            static void Prefix(MethodBase __originalMethod, ItemContainerEntry __0)
            {
                if (!Enabled() || !DungeonLobby.inUse || __0.IsNullOrDestroyed() || __0.data.IsNullOrDestroyed()) return;
                Trace(__originalMethod.Name, "during dungeon lobby; base=" + __0.data.itemType + "; subtype=" + __0.data.subType);
            }
        }
    }
}
