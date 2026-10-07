namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One buff change. Increased is a game fraction (0.1 = 10 %).</summary>
public readonly record struct BuffAction(
    BuffActionKind Kind,
    string BuffName,
    int StatId,
    float Added,
    float Increased,
    float DurationSeconds,
    int Stacks
);
