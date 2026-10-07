using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Kills;

internal static class MonsterModTestData
{
    public static MonsterModStat Stat(
        string property,
        float added,
        float increased,
        params float[] more
    )
    {
        return new MonsterModStat(property, added, increased, more, "TagX");
    }

    public static MonsterModRow Row(string list, IReadOnlyList<MonsterModStat> stats)
    {
        return new MonsterModRow
        {
            List = list,
            ModKey = 7,
            Title = "Title one",
            ModType = "TypeA",
            RarityRequirement = "RarityA",
            DisplayGroup = "GroupA",
            ClassName = "ClassA",
            RareModifier = 1.5f,
            ScalingType = "ScaleA",
            Stats = stats,
        };
    }
}
