namespace LastEpoch_Hud.Scripts.Core.Monoliths;

internal static class EchoReplayRules
{
    public static bool CanReplayIsland(bool enabled, bool completed) => enabled && completed;

    public static bool CanReplayYrun(
        bool enabled,
        bool isYrun,
        bool completed,
        bool waveCompleted,
        int storedKills,
        int requiredKills,
        bool enteringEcho
    ) =>
        enabled
        && isYrun
        && completed
        && waveCompleted
        && requiredKills >= 0
        && storedKills >= requiredKills
        && !enteringEcho;
}
