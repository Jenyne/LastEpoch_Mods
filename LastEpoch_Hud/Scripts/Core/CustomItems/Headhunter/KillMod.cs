namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One monster mod of a kill and its slice of KillInfo.ModStats.</summary>
public readonly record struct KillMod(int Key, int StatStart, int StatCount);
