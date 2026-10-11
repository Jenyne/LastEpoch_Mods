namespace LastEpoch_Hud.Scripts.Core.Factions;

/// <summary>Maps the Factions dropdown option to a faction.</summary>
public static class FactionSelection
{
    /// <summary>Option 0 is the "Select" placeholder; options 1..4 are the factions.</summary>
    public static bool TryKindAt(int optionIndex, out FactionKind kind)
    {
        kind = FactionKind.CircleOfFortune;
        switch (optionIndex)
        {
            case 1:
                kind = FactionKind.CircleOfFortune;
                return true;
            case 2:
                kind = FactionKind.MerchantsGuild;
                return true;
            case 3:
                kind = FactionKind.ForgottenKnights;
                return true;
            case 4:
                kind = FactionKind.TheWeaver;
                return true;
            default:
                return false;
        }
    }
}
