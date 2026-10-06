using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    internal static class ProphecyRewards
    {
        // Leave native quantity calculations intact. Change only the backing count
        // while this local player's reward is spawning and always restore the asset.
        [HarmonyPatch(typeof(ProphecySlotReward), "SpawnRewardForPlayer")]
        internal static class Spawn
        {
            [HarmonyPrefix]
            static void Prefix(ProphecySlotReward __instance, Actor __0, out int __state)
            {
                __state = -1;
                var setting = ModSettings.ProphecyRewards.Multiplier;
                if (!setting.Enabled || ModSaveManager.instance.IsNullOrDestroyed()
                    || !ModSaveManager.instance.initialized || !Scenes.IsGameScene()
                    || __instance.IsNullOrDestroyed() || __0.IsNullOrDestroyed()
                    || Refs_Manager.player_actor.IsNullOrDestroyed()
                    || __0.Pointer != Refs_Manager.player_actor.Pointer) return;
                float value = setting.Value;
                int factor = float.IsNaN(value) || float.IsInfinity(value)
                    ? 1 : Math.Max(1, Math.Min(10, (int)Math.Round(value)));
                if (factor == 1) return;
                int original = __instance.itemsDropped;
                if (original <= 0) return;
                long multiplied = (long)original * factor;
                if (multiplied > int.MaxValue)
                {
                    Main.logger_instance?.Warning("Prophecy rewards: count overflow; using normal rewards.");
                    return;
                }
                __state = original;
                __instance.itemsDropped = (int)multiplied;
                Main.logger_instance?.Msg("Prophecy rewards: spawning x" + factor
                    + " (base count " + original + " -> " + multiplied + ").");
            }

            [HarmonyFinalizer]
            static void Finalizer(ProphecySlotReward __instance, int __state)
            {
                if (__state < 0 || __instance.IsNullOrDestroyed()) return;
                __instance.itemsDropped = __state;
                ModSettings.Trace("Prophecy rewards: restored base count " + __state);
            }
        }
    }
}
