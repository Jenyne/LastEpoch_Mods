using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    internal static class ProphecyRewards
    {
        sealed class RewardScope
        {
            public IntPtr Reward;
            public int Multiplier, Calculating, Reading, Hits;
        }
        [ThreadStatic] static List<RewardScope> scopes;
        static bool warned;
        static RewardScope Current(ProphecySlotReward reward)
        {
            if (reward.IsNullOrDestroyed() || scopes == null || scopes.Count == 0) return null;
            var scope = scopes[scopes.Count - 1];
            return scope.Reward == reward.Pointer ? scope : null;
        }
        static void Scale(RewardScope scope, ref int count)
        {
            if (scope == null || count <= 0) return;
            count = (int)Math.Min(int.MaxValue, (long)count * scope.Multiplier);
            scope.Hits++;
        }
        [HarmonyPatch(typeof(ProphecySlotReward), "SpawnRewardForPlayer")]
        static class Spawn
        {
            [HarmonyPrefix]
            static void Prefix(ProphecySlotReward __instance, Actor __0, out RewardScope __state)
            {
                __state = null;
                var setting = ModSettings.ProphecyRewards.Multiplier;
                if (!setting.Enabled || ModSaveManager.instance.IsNullOrDestroyed() || !ModSaveManager.instance.initialized
                    || !Scenes.IsGameScene() || __0.IsNullOrDestroyed()
                    || Refs_Manager.player_actor.IsNullOrDestroyed() || __0.Pointer != Refs_Manager.player_actor.Pointer) return;
                float value = setting.Value;
                int factor = float.IsNaN(value) || float.IsInfinity(value) ? 1 : Math.Max(1, Math.Min(10, (int)Math.Round(value)));
                if (factor == 1) return;
                __state = new RewardScope { Reward = __instance.Pointer, Multiplier = factor };
                if (scopes == null) scopes = new List<RewardScope>();
                scopes.Add(__state);
            }
            [HarmonyFinalizer]
            static void Finalizer(RewardScope __state)
            {
                if (__state == null) return;
                scopes.Remove(__state);
                if (__state.Hits > 0)
                    Main.logger_instance?.Msg("Prophecy rewards: applied x" + __state.Multiplier + " multiplier.");
                if (__state.Hits == 0 && !warned)
                {
                    warned = true;
                    Main.logger_instance?.Warning("Prophecy multiplier: no quantity hook was reached during this reward. Report the reward type; this path may need another hook.");
                }
            }
        }
        [HarmonyPatch(typeof(ProphecySlotReward), "GetItemsDropped")]
        static class CalculateQuantity
        {
            [HarmonyPrefix]
            static void Prefix(ProphecySlotReward __instance, out RewardScope __state)
            {
                __state = Current(__instance);
                if (__state != null) __state.Calculating++;
            }
            [HarmonyPostfix]
            static void Postfix(RewardScope __state, ref int __result)
            {
                if (__state != null && __state.Calculating == 1 && __state.Reading == 0) Scale(__state, ref __result);
            }
            [HarmonyFinalizer]
            static void Finalizer(RewardScope __state)
            {
                if (__state != null) __state.Calculating--;
            }
        }
        // ItemsDropped is a native method; lowercase itemsDropped is a field accessor
        // and cannot be patched. Suppress nested scaling to apply the multiplier once.
        [HarmonyPatch]
        static class Quantity
        {
            static IEnumerable<System.Reflection.MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(ProphecySlotReward), "get_ItemsDropped");
            }
            [HarmonyPrefix]
            static void Prefix(ProphecySlotReward __instance, out RewardScope __state)
            {
                __state = Current(__instance);
                if (__state != null) __state.Reading++;
            }
            [HarmonyPostfix]
            static void Postfix(RewardScope __state, ref int __result)
            {
                if (__state != null && __state.Calculating == 0 && __state.Reading == 1) Scale(__state, ref __result);
            }
            [HarmonyFinalizer]
            static void Finalizer(RewardScope __state)
            {
                if (__state != null) __state.Reading--;
            }
        }
    }
}
