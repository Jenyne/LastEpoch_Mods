namespace LastEpoch_Hud.Scripts.Core.Monoliths;

internal static class EchoReplayRules
{
    public static bool CanReplayIsland(bool enabled, bool completed) => enabled && completed;

    public static bool CanReplayQuest(
        bool enabled,
        bool completed,
        bool empowered,
        bool boss,
        bool completionRequirementMet,
        bool branchRequirementMet,
        bool enoughStability
    ) =>
        enabled
        && completed
        && !empowered
        && !boss
        && completionRequirementMet
        && branchRequirementMet
        && enoughStability;
}
