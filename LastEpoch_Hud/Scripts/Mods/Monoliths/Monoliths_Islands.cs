using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Scripts.Mods.Monoliths;

public class Monoliths_Islands
{
    // Diagnostic branch only: cap and deduplicate messages from map refreshes.
    private static readonly System.Collections.Generic.HashSet<string> replayTrace = new();
    private static int islandTraceCount;

    private static bool ReplayConfigured() =>
        !Save_Manager.instance.IsNullOrDestroyed()
        && !Save_Manager.instance.data.IsNullOrDestroyed()
        && Save_Manager.instance.data.Scenes.Monoliths.Enable_Islands;

    public static void ResetReplayTrace(bool enabled)
    {
        replayTrace.Clear();
        islandTraceCount = 0;
        Monoliths_YrunReplay.ResetTrace();
        Main.logger_instance?.Msg($"[Echo Replay] Toggle={enabled}, scene={Scenes.SceneName}.");
    }

    private static void Trace(string message)
    {
        if (ReplayConfigured() && replayTrace.Count < 128 && replayTrace.Add(message))
            Main.logger_instance?.Msg("[Echo Replay] " + message);
    }

    private static bool TracePatch(System.Reflection.MethodBase original)
    {
        if (original != null)
            Main.logger_instance?.Msg(
                $"[Echo Replay] Preparing {original.DeclaringType?.Name}.{original.Name}."
            );
        return true;
    }

    public static bool CanRun()
    {
        bool r = false;
        if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
        {
            if (!Save_Manager.instance.data.IsNullOrDestroyed())
            {
                r = Save_Manager.instance.data.Scenes.Monoliths.Enable_Islands;
            }
        }
        return r;
    }

    [HarmonyPatch(typeof(EchoWeb), "islandCanBeRun", new[] { typeof(EchoWebIsland) })]
    public class EchoWeb_islandCanBeRun
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPostfix]
        static void Postfix(ref bool __result, EchoWebIsland __0)
        {
            bool native = __result;
            if (
                !__result
                && !__0.IsNullOrDestroyed()
                && EchoReplayRules.CanReplayIsland(CanRun(), __0.completed)
            )
            {
                __result = true;
            }
            if (ReplayConfigured() && islandTraceCount < 16 && !__0.IsNullOrDestroyed())
            {
                islandTraceCount++;
                Trace(
                    $"Island hex={__0.hexIndex}, type={__0.islandType}, completed={__0.completed}, "
                        + $"entered={__0.hasEnteredEcho}, native={native}, enabled={CanRun()}, final={__result}."
                );
            }
        }
    }

    [HarmonyPatch(typeof(EchoWebIslandUI), "clicked", new System.Type[0])]
    public class EchoWebIslandUI_ClickTrace
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPrefix]
        static bool Prefix(EchoWebIslandUI __instance)
        {
            if (__instance.IsNullOrDestroyed() || __instance.island.IsNullOrDestroyed())
                return true;
            var island = __instance.island;
            Trace(
                $"Clicked island hex={island.hexIndex}, completed={island.completed}, entered={island.hasEnteredEcho}."
            );
            if (!EchoReplayRules.CanReplayIsland(CanRun(), island.completed))
                return true;
            var panel = __instance._timelinePanel;
            if (
                panel.IsNullOrDestroyed()
                || panel.run.IsNullOrDestroyed()
                || panel.web.IsNullOrDestroyed()
            )
                return true;
            // Submit through the game's normal entry request instead of its UI's
            // completed-node selection gate. Do not clear island.completed.
            Trace(
                $"Requesting completed island timeline={panel.timelineID}, hex={island.hexIndex}."
            );
            Il2CppLE.Networking.Monolith.MonolithNetworkManager.SendStartRun(
                panel.timelineID,
                island.hexIndex
            );
            return false;
        }
    }

    [HarmonyPatch(
        typeof(MonolithRunsManager),
        "StartRun",
        new[] { typeof(TimelineID), typeof(int), typeof(int) }
    )]
    public class MonolithRunsManager_StartRunTrace
    {
        [HarmonyPostfix]
        static void Postfix(
            TimelineID __0,
            int __1,
            int __2,
            Il2CppOperationResult.Result<Il2CppSystem.ValueTuple<
                MonolithRun,
                EchoWebIsland
            >> __result
        ) =>
            Trace(
                $"Entry result timeline={__0}, difficulty={__1}, hex={__2}, success={__result.IsSuccess}."
            );
    }
}
