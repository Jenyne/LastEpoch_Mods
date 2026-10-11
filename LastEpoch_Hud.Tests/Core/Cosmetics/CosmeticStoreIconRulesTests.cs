using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

public sealed class CosmeticStoreIconRulesTests
{
    [Theory]
    [InlineData(false, false, false, false, false)]
    [InlineData(false, true, true, true, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, false, true, true, false)]
    [InlineData(true, true, false, false, true)]
    [InlineData(true, true, false, true, true)]
    [InlineData(true, true, true, true, true)]
    [InlineData(true, true, true, false, false)]
    public void HasStoreIcon_FollowsPolicy(
        bool hasSoftRef,
        bool guidValid,
        bool bundlesKnown,
        bool foundInBundles,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            CosmeticStoreIconRules.HasStoreIcon(hasSoftRef, guidValid, bundlesKnown, foundInBundles)
        );
    }
}
