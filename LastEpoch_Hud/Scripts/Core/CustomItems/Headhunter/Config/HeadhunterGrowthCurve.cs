using System;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

/// <summary>Buff value growth by total stacks: percent per stack after the first, up to a cap.</summary>
public readonly record struct HeadhunterGrowthCurve(float PerStackPercent, float CapPercent)
{
    private const float PercentPerFraction = 100f;

    public static HeadhunterGrowthCurve None => default;

    /// <summary>Value multiplier for a total stack count. One stack or fewer: exactly 1.</summary>
    public float Factor(int total)
    {
        if (total <= 1)
        {
            return 1f;
        }

        float percent = Math.Min(((total - 1) * PerStackPercent), CapPercent);
        return 1f + (percent / PercentPerFraction);
    }
}
