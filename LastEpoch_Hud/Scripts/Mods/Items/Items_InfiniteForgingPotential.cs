using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    internal static class Items_InfiniteForgingPotential
    {
        static bool Enabled() =>
            ModSettings.InfiniteForgingPotential.Enabled.Value
            && !ModSaveManager.instance.IsNullOrDestroyed() && ModSaveManager.instance.initialized
            && Scenes.IsGameScene() && !Refs_Manager.player_actor.IsNullOrDestroyed();
        sealed class CraftState { public bool OriginalNoCost; }
        [HarmonyPatch(typeof(CraftingManager), "Forge")]
        static class Forge
        {
            [HarmonyPrefix]
            static void Prefix(CraftingManager __instance, out CraftState __state)
            {
                __state = null;
                if (!Enabled() || __instance.IsNullOrDestroyed()) return;
                __state = new CraftState { OriginalNoCost = __instance.debugNoForgingPotentialCost };
                __instance.debugNoForgingPotentialCost = true;
            }
            [HarmonyFinalizer]
            static void Finalizer(CraftingManager __instance, CraftState __state)
            {
                if (__state == null) return;
                if (!__instance.IsNullOrDestroyed()) __instance.debugNoForgingPotentialCost = __state.OriginalNoCost;
            }
        }
        [HarmonyPatch(typeof(ItemData), "applyForgingPotentialCost")]
        static class Cost
        {
            [HarmonyPrefix]
            static void Prefix(ref bool __2)
            {
                if (Enabled()) __2 = true;
            }
        }
        [HarmonyPatch(typeof(ItemData), "applyForgingPotentialCostFromShard")]
        static class ShardCost
        {
            [HarmonyPrefix]
            static void Prefix(ref bool __1)
            {
                if (Enabled()) __1 = true;
            }
        }
    }
}
