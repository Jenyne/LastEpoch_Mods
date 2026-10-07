namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Which kills fire Headhunter.</summary>
public readonly record struct HeadhunterTriggers(
    bool Rare,
    bool Boss,
    bool Miniboss,
    bool MinionKills,
    bool Magic
);
