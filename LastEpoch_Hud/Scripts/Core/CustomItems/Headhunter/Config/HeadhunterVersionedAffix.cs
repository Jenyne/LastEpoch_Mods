namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

/// <summary>A default affix map entry plus the defaults version that introduced it.</summary>
public readonly record struct HeadhunterVersionedAffix(HeadhunterAffixEntry Entry, int Since);
