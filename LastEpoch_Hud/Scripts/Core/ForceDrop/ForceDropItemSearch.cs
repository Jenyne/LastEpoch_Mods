using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.ForceDrop;

internal readonly record struct ForceDropItemIdentity(
    int BaseType,
    int SubType,
    int Rarity,
    int UniqueId
);

internal static class ForceDropItemSearch
{
    public static bool Matches(string query, string name, string aliases)
    {
        foreach (
            string token in (query ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
        )
            if (
                (name ?? "").IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0
                && (aliases ?? "").IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0
            )
                return false;
        return true;
    }

    // Names are not keys: two items can share a label. Refuse ambiguous/stale ids.
    public static int FindOption(
        ForceDropItemIdentity identity,
        IEnumerable<(int Index, ForceDropItemIdentity Identity)> options
    )
    {
        int match = -1;
        foreach (var option in options)
            if (option.Index > 0 && option.Identity == identity)
            {
                if (match > 0)
                    return -1;
                match = option.Index;
            }
        return match;
    }
}
