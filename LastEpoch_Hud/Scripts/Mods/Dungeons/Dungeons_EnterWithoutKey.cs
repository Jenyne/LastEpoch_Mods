using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Dungeons
{
    public class Dungeons_EnterWithoutKey
    {
        public static bool CanRun()
        {
            return Scenes.IsGameScene() && !Save_Manager.instance.IsNullOrDestroyed()
                && Save_Manager.instance.initialized && !Save_Manager.instance.data.IsNullOrDestroyed()
                && Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey;
        }

        [HarmonyPatch(typeof(ItemContainersManager), "IsOccupiedWithValidDungeonKey")]
        public class ItemContainersManager_IsOccupiedWithValidDungeonKey
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result)
            {
                if (CanRun()) __result = true;
            }
        }

        [HarmonyPatch(typeof(DungeonEnterPanelUI), "Open")]
        static class OpenEntryPanel
        {
            [HarmonyPostfix]
            static void Postfix(DungeonEnterPanelUI __instance)
            {
                if (!CanRun()) return;
                if (!__instance.tierSelectionHolder.IsNullOrDestroyed()
                    && __instance.tierSelectionHolder.activeSelf) return;
                // Without a key insertion there is no onKeyEnter event to advance
                // the entry UI. Use its normal request/unlocked-tier flow instead.
                __instance.ProceedToTierSelection();
                Main.logger_instance?.Msg("Keyless dungeon entry: requested native tier selection");
            }
        }
    }
}
