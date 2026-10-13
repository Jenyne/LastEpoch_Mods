using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class UniqueModifierValuesTests
{
    static ResolvedForceDrop Request(int[] tiers = null, int[] rolls = null) =>
        new(
            14,
            0,
            420,
            7,
            0,
            4,
            10,
            true,
            Array.Empty<int>(),
            Array.Empty<int>(),
            Array.Empty<ResolvedForceDropAffix>(),
            new[] { 100, 101 },
            null,
            ForceDropMode.Legal,
            tiers,
            rolls
        );

    [Fact]
    public void ExistingFixedModifiersRetainTheirDefaultValues()
    {
        var request = Request();
        Assert.Equal(new[] { 0, 0 }, request.VariantTiers);
        Assert.Equal(new[] { 255, 255 }, request.VariantRolls);
    }

    [Fact]
    public void TwoTieredModifiersKeepIndependentImmutableValues()
    {
        int[] tiers =  { 6, 2 },
            rolls =  { 255, 38 };
        var request = Request(tiers, rolls);
        tiers[0] = 0;
        rolls[1] = 255;
        Assert.Equal(new[] { 6, 2 }, request.VariantTiers);
        Assert.Equal(new[] { 255, 38 }, request.VariantRolls);
        Assert.Equal(0, request.LegendaryPotential);
        Assert.Equal(0, request.WeaversWill);
    }

    [Fact]
    public void MissingModifierValueIsRejected() =>
        Assert.Throws<ArgumentException>(() => Request(new[] { 6 }, new[] { 255, 255 }));

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void DedicatedTierCannotExceedNativeT7(int tier) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(new[] { tier, 0 }));

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void ModifierRollMustFitNativeByte(int roll) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(null, new[] { roll, 255 }));
}
