namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One bad value in the config file. Path is the JSON path, or the parent when a key is missing.</summary>
public readonly record struct HeadhunterConfigProblem(string Path, string Message);
