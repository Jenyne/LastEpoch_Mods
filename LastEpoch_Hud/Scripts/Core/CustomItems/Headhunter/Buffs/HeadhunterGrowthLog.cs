using System.Globalization;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

/// <summary>Builds the one-line debug text for a value growth change.</summary>
public static class HeadhunterGrowthLog
{
    public static string Format(int total, float factor)
    {
        return "Headhunter growth: total="
            + total.ToString(CultureInfo.InvariantCulture)
            + " factor="
            + factor.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
