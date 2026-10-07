namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Merged config text, whether it differs from the input, and how many entries were added.</summary>
public readonly record struct HeadhunterMergeResult(string Text, bool Changed, int Added);
