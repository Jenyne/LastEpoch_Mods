namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One stat row. Increased is a percent (10 = 10 %), Added is the raw flat game value.</summary>
public readonly record struct HeadhunterStatEntry(
    string Stat,
    float Added,
    float Increased,
    bool Enabled
);
