using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.Monoliths;

/// <summary>Picks the first timeline the editor selects.</summary>
public static class MonolithTimelineSelection
{
    /// <summary>Current if set, else the first saved run timeline in 1..max, else 1.</summary>
    public static int Initial(
        int current,
        IReadOnlyList<MonolithRunSnapshot> runs,
        int maxTimelineId
    )
    {
        if (current > 0)
        {
            return current;
        }

        for (int i = 0; i < runs.Count; i++)
        {
            int id = runs[i].TimelineId;
            if (id >= 1 && id <= maxTimelineId)
            {
                return id;
            }
        }

        return 1;
    }
}
