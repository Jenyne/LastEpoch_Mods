using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

/// <summary>The whole Headhunter configuration as read from its JSON file.</summary>
public sealed class HeadhunterConfig
{
    public int Version { get; init; }
    public string Mechanic { get; init; }
    public float DurationSeconds { get; init; }
    public int MaxStacks { get; init; } = HeadhunterConfigDefaults.MaxStacks;
    public HeadhunterTriggers Triggers { get; init; }
    public IReadOnlyList<HeadhunterStatEntry> Stats { get; init; }
    public IReadOnlyList<HeadhunterAffixEntry> AffixMap { get; init; } =
        Array.Empty<HeadhunterAffixEntry>();
}
