using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Dungeons
{
    internal static class Dungeons_ObjectiveReveal
    {
        static DungeonZoneManager manager;
        static float originalThreshold;
        static bool overridden;
        public static void Apply(bool enabled)
        {
            if (manager.IsNullOrDestroyed()) { overridden = false; return; }
            if (enabled)
            {
                if (!overridden) originalThreshold = manager.objectiveRevealThresholdModifier;
                manager.objectiveRevealThresholdModifier = float.MaxValue;
                overridden = true;
            }
            else if (overridden)
            {
                manager.objectiveRevealThresholdModifier = originalThreshold;
                overridden = false;
            }
        }
        [HarmonyPatch(typeof(DungeonZoneManager), "initialise")]
        static class Initialise
        {
            [HarmonyPrefix]
            static void Prefix(DungeonZoneManager __instance)
            {
                // Reinitialisation must not capture our override as the native baseline.
                if (!manager.IsNullOrDestroyed() && manager.Pointer == __instance.Pointer) Apply(false);
            }
            [HarmonyPostfix]
            static void Postfix(DungeonZoneManager __instance)
            {
                if (!manager.IsNullOrDestroyed() && manager.Pointer != __instance.Pointer) Apply(false);
                manager = __instance;
                originalThreshold = __instance.objectiveRevealThresholdModifier;
                overridden = false;
                if (!ModSaveManager.instance.IsNullOrDestroyed() && ModSaveManager.instance.initialized)
                    Apply(ModSettings.DungeonReveal.Enabled.Value);
            }
        }
    }
}
