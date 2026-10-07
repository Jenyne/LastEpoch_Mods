namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Turns the monster rarity flags into a kill kind. Magic counts as Normal.</summary>
public static class KillKindClassifier
{
    public static KillKind Classify(bool isBoss, bool isMiniboss, bool isRare)
    {
        if (isBoss)
        {
            return KillKind.Boss;
        }

        if (isMiniboss)
        {
            return KillKind.Miniboss;
        }

        return isRare ? KillKind.Rare : KillKind.Normal;
    }
}
