using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

/// <summary>Values used when the file is missing or a value is unusable.</summary>
public static class HeadhunterConfigDefaults
{
    public const int CurrentVersion = 1;
    public const int DefaultsVersion = 6;
    public const int UnstampedDefaultsVersion = 1;
    public const float DurationSeconds = 60f;
    public const int MaxStacks = 10;
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
            new(new HeadhunterStatEntry("CriticalChance", 0.03f, 0f, true), 1),
            new(new HeadhunterStatEntry("Movespeed", 0f, 5f, true), 1),
            new(new HeadhunterStatEntry("Health", 0f, 5f, true), 1),
            new(new HeadhunterStatEntry("Armour", 50f, 0f, true), 1),
            new(new HeadhunterStatEntry("ManaRegen", 0f, 10f, true), 2),
            new(new HeadhunterStatEntry("ManaEfficiency", 0f, 5f, true), 2),
            new(new HeadhunterStatEntry("IncreasedLeechRate", 0f, 10f, true), 2),
            new(new HeadhunterStatEntry("HealthLeech", 0.01f, 0f, true), 2),
            new(new HeadhunterStatEntry("DodgeRating", 50f, 0f, true), 5),
            new(new HeadhunterStatEntry("HealthRegen", 5f, 0f, true), 5),
            new(new HeadhunterStatEntry("StunAvoidance", 100f, 0f, true), 5),
            new(new HeadhunterStatEntry("IncreasedCooldownRecoverySpeed", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Fire"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Cold"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Lightning"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Necrotic"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Poison"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Void"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Physical"), 5),
            new(new HeadhunterStatEntry("Damage", 0f, 10f, true, "Minion"), 5),
            new(new HeadhunterStatEntry("FireResistance", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("ColdResistance", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("LightningResistance", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("NecroticResistance", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("PoisonResistance", 0.05f, 0f, true), 5),
            new(new HeadhunterStatEntry("VoidResistance", 0.05f, 0f, true), 5),
        };

    public static readonly IReadOnlyList<HeadhunterStatEntry> Stats = VersionedStats
        .Select(row => row.Entry)
        .ToList();

    public static readonly IReadOnlyList<HeadhunterVersionedField> VersionedFields =
        new List<HeadhunterVersionedField>
        {
            new(HeadhunterConfigKeys.Triggers, HeadhunterConfigKeys.Magic, new JValue(true), 3),
            new("", HeadhunterConfigKeys.MaxStacks, new JValue(MaxStacks), 4),
        };

    public static readonly HeadhunterMergeDefaults MergeDefaults = new()
    {
        Version = DefaultsVersion,
        Stats = VersionedStats,
        Fields = VersionedFields,
        Affixes = HeadhunterAffixDefaults.VersionedAffixes,
    };

    public static readonly HeadhunterConfig Config = new()
    {
        Version = CurrentVersion,
        DurationSeconds = DurationSeconds,
        MaxStacks = MaxStacks,
        Triggers = Triggers,
        Stats = Stats,
        AffixMap = HeadhunterAffixDefaults.AffixMap,
    };
}
