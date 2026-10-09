using System;
using System.Text;

namespace LastEpoch_Hud.Scripts.Core.QualityOfLife;

public static class KeyTeleportTargetRules
{
    // Prefer the campaign approach; never match the dungeon's own map label.
    static readonly string[][] aliases =
    {
        new[] { "eot", "endoftime" },
        new[] { "ruinedcoast" },
        new[] { "shroudedridge", "surface" },
        new[] { "felledwood" },
        new[] { "bazaar" },
        new[] { "observatory" },
    };

    static string Normalize(string name)
    {
        var result = new StringBuilder();
        foreach (char c in name ?? "")
            if (char.IsLetterOrDigit(c))
                result.Append(char.ToLowerInvariant(c));
        return result.ToString();
    }

    public static int MatchPriority(int index, string scene, string localizedName)
    {
        if (index < 0 || index >= aliases.Length || !TravelSceneRules.IsDestination(scene))
            return -1;
        string normalized = Normalize(scene);
        string label = Normalize(localizedName);
        for (int i = 0; i < aliases[index].Length; i++)
        {
            string alias = aliases[index][i];
            if (
                normalized == alias
                || normalized == "the" + alias
                || label == alias
                || label == "the" + alias
            )
                return i;
        }
        return -1;
    }

    public static int SavedDungeonPreset(string scene)
    {
        if (string.Equals(scene, "Dun1Q10", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (string.Equals(scene, "Dun2Q10", StringComparison.OrdinalIgnoreCase))
            return 2;
        if (string.Equals(scene, "Dun3Q10", StringComparison.OrdinalIgnoreCase))
            return 3;
        return -1;
    }
}
