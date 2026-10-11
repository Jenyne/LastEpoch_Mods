using HarmonyLib;
using Il2Cpp;
using Il2CppActivities;
using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Scripts.Mods.Monoliths;

// Yrun is a Morditas activity, not a timeline quest echo.
internal static class Monoliths_YrunReplay
{
    private static readonly System.Collections.Generic.HashSet<string> trace = new();

    internal static void ResetTrace() => trace.Clear();

    private static void Trace(string message)
    {
        if (trace.Count < 64 && trace.Add(message))
            Main.logger_instance?.Msg("[Yrun Replay] " + message);
    }

    private static bool CanReplay(RageOfMorditasActivityPanel panel, int index)
    {
        if (
            !Monoliths_Islands.CanRun()
            || panel.IsNullOrDestroyed()
            || panel._data.IsNullOrDestroyed()
            || index != 1
        )
            return false;

        bool replay = EchoReplayRules.CanReplayYrun(
            true,
            panel.GetWovenEchoType(index) == WovenEchoType.EndgameYrun,
            panel.yrunFightCompleted,
            panel.waveFightCompleted,
            panel.storedKills,
            panel.GetWovenEchoKillThreshold(index),
            panel._enteringEcho
        );
        Trace(
            $"index={index}, completed={panel.yrunFightCompleted}, "
                + $"waveCompleted={panel.waveFightCompleted}, kills={panel.storedKills}, "
                + $"required={panel.GetWovenEchoKillThreshold(index)}, entering={panel._enteringEcho}, replay={replay}."
        );
        return replay;
    }

    [HarmonyPatch(typeof(RageOfMorditasActivityPanel), "CanEnterOrb", new[] { typeof(int) })]
    internal static class CanEnterOrb
    {
        [HarmonyPostfix]
        private static void Postfix(
            RageOfMorditasActivityPanel __instance,
            int __0,
            ref bool __result
        )
        {
            if (!__result && CanReplay(__instance, __0))
                __result = true;
        }
    }

    [HarmonyPatch(
        typeof(RageOfMorditasActivityPanel),
        "UpdateOrbVisuals",
        new[] { typeof(int), typeof(bool) }
    )]
    internal static class UpdateOrbVisuals
    {
        [HarmonyPostfix]
        private static void Postfix(RageOfMorditasActivityPanel __instance, int __0)
        {
            if (!CanReplay(__instance, __0) || __instance._islandOrbs.IsNullOrDestroyed())
                return;
            foreach (var orb in __instance._islandOrbs)
                if (
                    !orb.IsNullOrDestroyed()
                    && orb.IslandIndex == 1
                    && !orb.OrbButton.IsNullOrDestroyed()
                )
                    orb.OrbButton.interactable = true;
        }
    }

    [HarmonyPatch(typeof(RageOfMorditasActivityPanel), "OnOrbClicked", new[] { typeof(int) })]
    internal static class OnOrbClicked
    {
        [HarmonyPrefix]
        private static bool Prefix(RageOfMorditasActivityPanel __instance, int __0)
        {
            if (!CanReplay(__instance, __0))
                return true;
            __instance.SelectOrb(__0);
            __instance.TryEnterWovenEcho(__0);
            return false;
        }
    }

    [HarmonyPatch(typeof(RageOfMorditasActivityPanel), "TryEnterWovenEcho", new[] { typeof(int) })]
    internal static class TryEnterWovenEcho
    {
        [HarmonyPrefix]
        private static void Prefix(
            RageOfMorditasActivityPanel __instance,
            int __0,
            ref bool __state
        )
        {
            __state = CanReplay(__instance, __0);
            if (__state)
            {
                // This is the UI panel's cached flag, not character save data.
                // Let the native UI run its remaining entry checks. Restore the
                // cache even when entry throws; never rewind saved progression.
                __instance.yrunFightCompleted = false;
                Trace("Requesting completed Yrun activity.");
            }
        }

        [HarmonyFinalizer]
        private static void Finalizer(RageOfMorditasActivityPanel __instance, bool __state)
        {
            if (__state && !__instance.IsNullOrDestroyed())
                __instance.yrunFightCompleted = true;
        }
    }

    [HarmonyPatch(
        typeof(RageOfMorditasActivity),
        "CanRunActivity",
        new[] { typeof(PlayerActorSync), typeof(WovenEchoType) }
    )]
    internal static class CanRunActivity
    {
        [HarmonyPostfix]
        private static void Postfix(PlayerActorSync __0, WovenEchoType __1, ref bool __result)
        {
            if (
                __result
                || __1 != WovenEchoType.EndgameYrun
                || !Monoliths_Islands.CanRun()
                || __0.IsNullOrDestroyed()
            )
                return;
            var tracker = __0.GetComponent<CharacterDataTracker>();
            var data = RageOfMorditasData.Instance;
            if (
                tracker.IsNullOrDestroyed()
                || tracker.charData.IsNullOrDestroyed()
                || data.IsNullOrDestroyed()
            )
                return;
            var progression = tracker.charData.RageOfMorditasData;
            if (progression.IsNullOrDestroyed())
                return;

            __result = EchoReplayRules.CanReplayYrun(
                true,
                true,
                progression.YrunFightCompleted,
                progression.WaveFightCompleted,
                progression.StoredKills,
                data.YrunKillCount,
                false
            );
            Trace($"Activity entry validation={__result}.");
        }
    }
}
