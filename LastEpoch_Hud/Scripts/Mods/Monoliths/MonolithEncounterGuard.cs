using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Scripts.Mods.Monoliths;

internal static class MonolithEncounterGuard
{
    public static bool ShouldSkip(MonolithZoneManager zone, out string sceneName)
    {
        sceneName = "(unavailable)";
        if (zone.IsNullOrDestroyed())
        {
            return true;
        }

        try
        {
            // The active scene can still be MonolithHub at this point.
            sceneName = zone.gameObject.scene.name;
            return MonolithEncounterRules.IsProtectedEncounter(
                sceneName,
                zone.isQuestZone,
                zone.isHarbingerFight,
                zone.isTimelineBossEncounter
            );
        }
        catch (Exception ex)
        {
            Main.logger_instance?.Warning(
                "[Monolith Guard] Encounter classification failed, skipping automation: " + ex
            );
            return true;
        }
    }
}
