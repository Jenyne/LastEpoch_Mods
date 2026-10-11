using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

internal static class MonolithRunFactory
{
    public static MonolithRunSnapshot Run(
        int timelineId,
        int difficulty,
        int stability = 0,
        bool hasEchoWeb = false,
        int corruption = 0,
        int gaze = 0
    )
    {
        return new MonolithRunSnapshot(
            timelineId,
            difficulty,
            stability,
            hasEchoWeb,
            corruption,
            gaze
        );
    }
}
