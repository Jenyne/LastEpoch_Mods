using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Debug line for a bar change.</summary>
public static class HeadhunterBarLog
{
    public static string Format(
        IReadOnlyList<HeadhunterBarEntry> entries,
        Func<int, string> statName
    )
    {
        string[] names = new string[entries.Count];
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = statName(entries[i].StatId);
        }

        return $"Headhunter bar: count={entries.Count} [{string.Join(", ", names)}]";
    }
}
