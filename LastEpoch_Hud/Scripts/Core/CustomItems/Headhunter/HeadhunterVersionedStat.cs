namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>A default stat row plus the defaults version that introduced it.</summary>
public readonly record struct HeadhunterVersionedStat(HeadhunterStatEntry Entry, int Since);
