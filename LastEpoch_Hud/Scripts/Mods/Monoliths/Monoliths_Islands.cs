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

    private static bool CanReplayQuest(MonolithRun run, int questEchoIndex)
    {
        if (!CanRun() || run.IsNullOrDestroyed() || run.timeline.IsNullOrDestroyed())
            return false;

        var quests = run.timeline.questZones;
        if (quests.IsNullOrDestroyed() || questEchoIndex < 0 || questEchoIndex >= quests.Count)
            return false;

        var quest = quests[questEchoIndex];
        if (quest.IsNullOrDestroyed())
            return false;

        // Never rewind questCompletion or questBranch to make a replay available.
        // Empowered timelines and the timeline boss retain their native rules.
        bool completed = run.IsQuestEchoCompleted(questEchoIndex);
        bool empowered = run.IsEmpowered;
        bool boss = run.IsBossQuestEcho(questEchoIndex);
        bool completionMet = run.questCompletion >= quest.requiredQuestCompletion;
        bool branchMet = !quest.requiresBranch || run.questBranch == quest.requiredBranch;
        bool stabilityMet = run.HasEnoughStabilityForQuestEcho(questEchoIndex);
        bool replay = EchoReplayRules.CanReplayQuest(
            true,
            completed,
            empowered,
            boss,
            completionMet,
            branchMet,
            stabilityMet
        );
        Trace(
            $"Quest timeline={run.timelineID}, index={questEchoIndex}, scene={quest.sceneName}, "
                + $"completed={completed}, empowered={empowered}, boss={boss}, "
                + $"completion={run.questCompletion}/{quest.requiredQuestCompletion}, "
                + $"branch={run.questBranch}/{quest.requiredBranch}, requiresBranch={quest.requiresBranch}, "
                + $"stability={run.stability}, needed={run.GetStabilityRequiredForQuestEcho(questEchoIndex)}, "
                + $"stabilityMet={stabilityMet}, replay={replay}."
        );
        return replay;
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

    [HarmonyPatch(typeof(QuestEcho), "GetAvailability", new[] { typeof(MonolithRun), typeof(int) })]
    public class QuestEcho_GetAvailability
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPostfix]
        static void Postfix(MonolithRun __0, int __1, ref QuestEcho.QuestEchoAvailability __result)
        {
            var native = __result;
            // Only the native Completed state is eligible, never Locked or Unavailable.
            if (__result == QuestEcho.QuestEchoAvailability.Completed && CanReplayQuest(__0, __1))
                __result = QuestEcho.QuestEchoAvailability.UnlockedAvailable;
            Trace(
                $"Quest availability index={__1}, native={native}, enabled={CanRun()}, final={__result}."
            );
        }
    }

    [HarmonyPatch(typeof(MonolithRun), "CanStartQuestEcho", new[] { typeof(int) })]
    public class MonolithRun_CanStartQuestEcho
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPostfix]
        static void Postfix(MonolithRun __instance, int __0, ref bool __result)
        {
            bool native = __result;
            if (!__result && CanReplayQuest(__instance, __0))
                __result = true;
            Trace(
                $"Quest start index={__0}, native={native}, enabled={CanRun()}, final={__result}."
            );
        }
    }

    [HarmonyPatch(typeof(QuestEcho), "Init", new[] { typeof(MonolithRun), typeof(int) })]
    public class QuestEcho_InitTrace
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPostfix]
        static void Postfix(QuestEcho __instance, int __1, QuestEcho.QuestEchoAvailability __result)
        {
            if (!__instance.IsNullOrDestroyed())
                Trace(
                    $"Quest UI index={__1}, availability={__result}, isAvailable={__instance.IsAvailable}."
                );
        }
    }

    [HarmonyPatch(typeof(EchoWebIslandUI), "clicked", new System.Type[0])]
    public class EchoWebIslandUI_ClickTrace
    {
        [HarmonyPrepare]
        static bool Prepare(System.Reflection.MethodBase original) => TracePatch(original);

        [HarmonyPrefix]
        static void Prefix(EchoWebIslandUI __instance)
        {
            if (__instance.IsNullOrDestroyed() || __instance.island.IsNullOrDestroyed())
                return;
            var island = __instance.island;
            Trace(
                $"Clicked island hex={island.hexIndex}, completed={island.completed}, entered={island.hasEnteredEcho}."
            );
        }
    }
}
