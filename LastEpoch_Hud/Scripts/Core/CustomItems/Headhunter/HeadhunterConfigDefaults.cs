using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Values used when the file is missing or a value is unusable.</summary>
public static class HeadhunterConfigDefaults
{
    public const int CurrentVersion = 1;
    public const int DefaultsVersion = 3;
    public const int UnstampedDefaultsVersion = 1;
    public const string Mechanic = HeadhunterMechanics.RareModsId;
    public const float DurationSeconds = 60f;
    public const float EntryAdded = 0f;
    public const float EntryIncreased = 0f;
    public const bool EntryEnabled = true;

    public static readonly HeadhunterTriggers Triggers = new(true, true, true, true, true);

    public static readonly IReadOnlyList<HeadhunterVersionedStat> VersionedStats =
        new List<HeadhunterVersionedStat>
        {
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true), 1),
            new(new HeadhunterStatEntry("AttackSpeed", 0f, 5f, true), 1),
            new(new HeadhunterStatEntry("CastSpeed", 0f, 5f, true), 1),
            new(new HeadhunterStatEntry("CriticalChance", 0f, 10f, true), 1),
            new(new HeadhunterStatEntry("Movespeed", 0f, 5f, true), 1),
            new(new HeadhunterStatEntry("Health", 20f, 0f, true), 1),
            new(new HeadhunterStatEntry("Armour", 50f, 0f, true), 1),
            new(new HeadhunterStatEntry("ManaRegen", 0f, 10f, true), 2),
            new(new HeadhunterStatEntry("ManaEfficiency", 0f, 5f, true), 2),
            new(new HeadhunterStatEntry("IncreasedLeechRate", 0f, 10f, true), 2),
            new(new HeadhunterStatEntry("HealthLeech", 0.01f, 0f, true), 2),
        };

    public static readonly IReadOnlyList<HeadhunterStatEntry> Stats = VersionedStats
        .Select(row => row.Entry)
        .ToList();

    public static readonly IReadOnlyList<HeadhunterVersionedField> VersionedFields =
        new List<HeadhunterVersionedField>
        {
            new(HeadhunterConfigKeys.Triggers, HeadhunterConfigKeys.Magic, new JValue(true), 3),
        };

    public static readonly HeadhunterMergeDefaults MergeDefaults = new()
    {
        Version = DefaultsVersion,
        Stats = VersionedStats,
        Fields = VersionedFields,
    };

    public static readonly HeadhunterConfig Config = new()
    {
        Version = CurrentVersion,
        Mechanic = Mechanic,
        DurationSeconds = DurationSeconds,
        Triggers = Triggers,
        Stats = Stats,
    };
}
