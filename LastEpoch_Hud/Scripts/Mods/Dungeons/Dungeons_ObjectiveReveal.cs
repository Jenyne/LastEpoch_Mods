using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Dungeons
{
    internal static class Dungeons_ObjectiveReveal
    {
        static DelayedZoneObjectivePulse pulse;

        static bool Enabled => Scenes.IsGameScene()
            && !ModSaveManager.instance.IsNullOrDestroyed()
            && ModSaveManager.instance.initialized
            && ModSettings.DungeonReveal.Enabled.Value;

        public static void Apply(bool enabled)
        {
            if (!enabled || !Enabled || pulse.IsNullOrDestroyed() || pulse.activated
                || pulse.dungeonZoneManager.IsNullOrDestroyed()) return;
            // Use the native reveal path, including its activation state and sync.
            // The same component also serves monoliths: require a dungeon manager.
            pulse.activate();
            Main.logger_instance?.Msg("Dungeon objective reveal: activated native pulse");
        }

        static System.Collections.IEnumerator ApplyWhenReady(DelayedZoneObjectivePulse target)
        {
            // Start and dungeon initialise can run in either order. Wait for both.
            for (int i = 0; i < 120; i++)
            {
                yield return null;
                if (target.IsNullOrDestroyed()) yield break;
                if (target.dungeonZoneManager.IsNullOrDestroyed()) continue;
                pulse = target;
                Apply(ModSettings.DungeonReveal.Enabled.Value);
                if (!ModSaveManager.instance.IsNullOrDestroyed()
                    && ModSaveManager.instance.initialized) yield break;
            }
        }

        [HarmonyPatch(typeof(DelayedZoneObjectivePulse), "Start")]
        static class PulseStart
        {
            [HarmonyPostfix]
            static void Postfix(DelayedZoneObjectivePulse __instance)
            {
                MelonLoader.MelonCoroutines.Start(ApplyWhenReady(__instance));
            }
        }

        [HarmonyPatch(typeof(DelayedZoneObjectivePulse), "onScoreChanged")]
        static class ScoreChanged
        {
            [HarmonyPostfix]
            static void Postfix(DelayedZoneObjectivePulse __instance)
            {
                if (__instance.dungeonZoneManager.IsNullOrDestroyed()) return;
                pulse = __instance;
                Apply(ModSettings.DungeonReveal.Enabled.Value);
            }
        }

        [HarmonyPatch(typeof(DelayedZoneObjectivePulse), "OnDestroy")]
        static class PulseDestroyed
        {
            [HarmonyPrefix]
            static void Prefix(DelayedZoneObjectivePulse __instance)
            {
                if (!pulse.IsNullOrDestroyed() && pulse.Pointer == __instance.Pointer) pulse = null;
            }
        }
    }
}
