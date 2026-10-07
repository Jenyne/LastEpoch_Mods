using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Values used when the file is missing or a value is unusable.</summary>
public static class HeadhunterConfigDefaults
{
    public const int CurrentVersion = 1;
    public const string Mechanic = "rare_mods";
    public const float DurationSeconds = 60f;
    public const float EntryAdded = 0f;
    public const float EntryIncreased = 0f;
    public const bool EntryEnabled = true;

    public static readonly HeadhunterTriggers Triggers = new(true, true, true, true);

    public static readonly IReadOnlyList<HeadhunterStatEntry> Stats = new List<HeadhunterStatEntry>
    {
        new("Damage", 0f, 10f, true),
        new("AttackSpeed", 0f, 5f, true),
        new("CastSpeed", 0f, 5f, true),
        new("CriticalChance", 0f, 10f, true),
        new("Movespeed", 0f, 5f, true),
        new("Health", 20f, 0f, true),
        new("Armour", 50f, 0f, true),
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
