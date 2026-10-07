using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

/// <summary>The total stacks and value factor the live Headhunter buffs carry.</summary>
public sealed class HeadhunterValueGrowth
{
    private readonly HeadhunterGrowthCurve _curve;

    public HeadhunterValueGrowth(HeadhunterGrowthCurve curve)
    {
        _curve = curve;
    }

    public int AppliedTotal { get; private set; }
    public float Factor { get; private set; } = 1f;

    /// <summary>Stores the new total. True only when the factor changed (live buffs need new values).</summary>
    public bool Update(int total)
    {
        if (total == AppliedTotal)
        {
            return false;
        }

        AppliedTotal = total;
        float factor = _curve.Factor(total);
        bool changed = !factor.Equals(Factor);
        Factor = factor;
        return changed;
    }

    public void Reset()
    {
        AppliedTotal = 0;
        Factor = 1f;
    }
}
