using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    internal static class Items_InfiniteForgingPotential
    {
        [ThreadStatic] static int localCraftDepth;
        sealed class CraftState { public bool OriginalNoCost; }
        [HarmonyPatch(typeof(CraftingManager), "Forge")]
        static class Forge
        {
            [HarmonyPrefix]
            static void Prefix(CraftingManager __instance, out CraftState __state)
            {
                __state = null;
                if (!ModSettings.InfiniteForgingPotential.Enabled.Value
                    || ModSaveManager.instance.IsNullOrDestroyed() || !ModSaveManager.instance.initialized
                    || !Scenes.IsGameScene() || __instance.actor.IsNullOrDestroyed()
                    || Refs_Manager.player_actor.IsNullOrDestroyed()
                    || __instance.actor.Pointer != Refs_Manager.player_actor.Pointer) return;
                __state = new CraftState { OriginalNoCost = __instance.debugNoForgingPotentialCost };
                __instance.debugNoForgingPotentialCost = true;
                localCraftDepth++;
            }
            [HarmonyFinalizer]
            static void Finalizer(CraftingManager __instance, CraftState __state)
            {
                if (__state == null) return;
                localCraftDepth--;
                if (!__instance.IsNullOrDestroyed()) __instance.debugNoForgingPotentialCost = __state.OriginalNoCost;
            }
        }
        [HarmonyPatch(typeof(ItemData), "applyForgingPotentialCost")]
        static class Cost
        {
            [HarmonyPrefix]
            static void Prefix(ref bool __2)
            {
                if (localCraftDepth > 0) __2 = true;
            }
        }
        [HarmonyPatch(typeof(ItemData), "applyForgingPotentialCostFromShard")]
        static class ShardCost
        {
            [HarmonyPrefix]
            static void Prefix(ref bool __1)
            {
                if (localCraftDepth > 0) __1 = true;
            }
        }
    }
}
