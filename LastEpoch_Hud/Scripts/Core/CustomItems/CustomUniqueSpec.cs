namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Fixed ids and levels of one custom unique item.</summary>
public sealed class CustomUniqueSpec
{
    /// <summary>Base id value meaning "take the next free subtype id".</summary>
    public const int AllocateBaseId = -1;

    public string Name { get; init; }
    public ushort UniqueId { get; init; }
    public byte BaseType { get; init; }
    public int BaseId { get; init; }
    public bool AddsBase { get; init; }
    public int LevelRequirement { get; init; }
    public bool OverrideLevelRequirement { get; init; }
    public int EffectiveLevelForLegendaryPotential { get; init; }
}
