using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.Cosmetics;

/// <summary>Decides which registry cosmetics are real MTX and which are junk.</summary>
public static class CosmeticCatalogRules
{
    private static readonly string[] _realPrefixes =
    {
        "MTX_",
        "mtx_",
        "SP_",
        "KS_",
        "Event_",
        "TD_",
    };

    private static readonly HashSet<string> _blocklist = new(StringComparer.Ordinal)
    {
        "MTX_Chest_TemporalGuardianFix",
    };

    private static readonly char[] _separators = { '_', ' ', '-' };

    /// <summary>Verdict for one entry; precedence EmptyId, UnknownPrefix, Blocklisted, TestEntry, Keep.</summary>
    public static CosmeticVerdict Judge(string id, string displayName)
    {
        if (string.IsNullOrEmpty(id))
        {
            return CosmeticVerdict.EmptyId;
        }
        if (!HasRealPrefix(id))
        {
            return CosmeticVerdict.UnknownPrefix;
        }
        if (_blocklist.Contains(id))
        {
            return CosmeticVerdict.Blocklisted;
        }
        if (HasTestWord(id) || HasTestWord(displayName))
        {
            return CosmeticVerdict.TestEntry;
        }
        return CosmeticVerdict.Keep;
    }

    private static bool HasRealPrefix(string id)
    {
        foreach (string prefix in _realPrefixes)
        {
            if (id.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasTestWord(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        foreach (string segment in text.Split(_separators))
        {
            if (IsTestWord(segment))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsTestWord(string segment)
    {
        if (
            !segment.StartsWith("Test", StringComparison.Ordinal)
            && !segment.StartsWith("test", StringComparison.Ordinal)
        )
        {
            return false;
        }
        if (segment.Length == 4)
        {
            return true;
        }
        char next = segment[4];
        return char.IsDigit(next) || char.IsUpper(next);
    }
}
