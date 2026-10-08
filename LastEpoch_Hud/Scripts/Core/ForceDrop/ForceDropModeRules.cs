namespace LastEpoch_Hud.Scripts.Core.ForceDrop;

// Illegal mode relaxes gameplay eligibility, not the native definition or
// three-bit packed tier range. Creation mode belongs to the immutable request.
public static class ForceDropModeRules
{
    public static int MaximumTier(int definitionTierCount, ForceDropMode mode) =>
        ForceDropTierRules.MaximumDisplayTier(
            definitionTierCount,
            mode == ForceDropMode.Illegal ? 8 : 7
        );

    public static bool CanSealPrimordial(
        int itemType,
        int rarity,
        ForceDropAffixFamily family,
        int definitionTierCount,
        ForceDropMode mode
    ) =>
        definitionTierCount >= 8
        && (
            mode == ForceDropMode.Illegal
            || (
                itemType >= 0
                && itemType <= 24
                && rarity < 7
                && family == ForceDropAffixFamily.Standard
            )
        );

    public static int MaximumPersistedTier(int definitionTierCount) =>
        ForceDropTierRules.MaximumDisplayTier(definitionTierCount, 8) - 1;
}
