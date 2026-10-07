namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

/// <summary>One enabled, game-ready stat row. Tags is the game ability tag id (0 = none).</summary>
public readonly record struct HeadhunterBuffStat(
    int StatId,
    string BuffName,
    float Added,
    float Increased,
    int Tags = 0
)
{
    public HeadhunterStatKey Key => new(StatId, Tags);

    public float AddedFor(int stacks, float factor)
    {
        return Added * stacks * factor;
    }

    public float IncreasedFor(int stacks, float factor)
    {
        return Increased * stacks * factor;
    }
}
