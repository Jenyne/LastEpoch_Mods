namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

/// <summary>Decides whether HH buff timers stand still.</summary>
public static class HeadhunterPauseRule
{
    public static bool IsPaused(bool nonCombatZone, bool arrivalProtected, bool cinematicActive)
    {
        return nonCombatZone || arrivalProtected || cinematicActive;
    }
}
