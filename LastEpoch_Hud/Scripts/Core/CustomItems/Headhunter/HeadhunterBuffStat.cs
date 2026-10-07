namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One enabled, game-ready stat row.</summary>
public readonly record struct HeadhunterBuffStat(
    int StatId,
    string BuffName,
    float Added,
    float Increased
);
