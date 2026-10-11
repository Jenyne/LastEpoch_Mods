using System;

namespace LastEpoch_Hud.Scripts.Core.Monoliths;

// Scene IDs are local to the loaded encounter; do not use the globally active scene
// while MonolithZoneManager.initialise is running.
public static class MonolithEncounterRules
{
    public static bool IsProtectedEncounter(
        string sceneName,
        bool isQuestZone,
        bool isHarbingerFight,
        bool isTimelineBossEncounter
    )
    {
        return isQuestZone
            || isHarbingerFight
            || isTimelineBossEncounter
            || string.Equals(sceneName, "WE502", StringComparison.OrdinalIgnoreCase);
    }
}
