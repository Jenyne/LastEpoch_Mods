using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Scripts.Mods.Monoliths;

public class Monoliths_Islands
{
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
        return EchoReplayRules.CanReplayQuest(
            true,
            run.IsQuestEchoCompleted(questEchoIndex),
            run.IsEmpowered,
            run.IsBossQuestEcho(questEchoIndex),
            run.questCompletion >= quest.requiredQuestCompletion,
            !quest.requiresBranch || run.questBranch == quest.requiredBranch,
            run.HasEnoughStabilityForQuestEcho(questEchoIndex)
        );
    }

    [HarmonyPatch(typeof(EchoWeb), "islandCanBeRun", new[] { typeof(EchoWebIsland) })]
    public class EchoWeb_islandCanBeRun
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, EchoWebIsland __0)
        {
            if (
                !__result
                && !__0.IsNullOrDestroyed()
                && EchoReplayRules.CanReplayIsland(CanRun(), __0.completed)
            )
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(QuestEcho), "GetAvailability", new[] { typeof(MonolithRun), typeof(int) })]
    public class QuestEcho_GetAvailability
    {
        [HarmonyPostfix]
        static void Postfix(MonolithRun __0, int __1, ref QuestEcho.QuestEchoAvailability __result)
        {
            // Only the native Completed state is eligible, never Locked or Unavailable.
            if (__result == QuestEcho.QuestEchoAvailability.Completed && CanReplayQuest(__0, __1))
                __result = QuestEcho.QuestEchoAvailability.UnlockedAvailable;
        }
    }

    [HarmonyPatch(typeof(MonolithRun), "CanStartQuestEcho", new[] { typeof(int) })]
    public class MonolithRun_CanStartQuestEcho
    {
        [HarmonyPostfix]
        static void Postfix(MonolithRun __instance, int __0, ref bool __result)
        {
            if (!__result && CanReplayQuest(__instance, __0))
                __result = true;
        }
    }
}
