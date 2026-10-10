using LastEpoch_Hud.Scripts.Core.Items;

namespace LastEpoch_Hud.Tests.Core.Items;

/// <summary>Loaded drop rarity 5 and 6 map to 4; everything else stays.</summary>
public sealed class DropRarityClampTests
{
    [Theory]
    [InlineData((byte)5)]
    [InlineData((byte)6)]
    public void TryClamp_Rarity5Or6_ReturnsTrueAndClampsToFour(byte rarity)
    {
        bool changed = DropRarityClamp.TryClamp(rarity, out byte clamped);

        Assert.True(changed);
        Assert.Equal(4, clamped);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)4)]
    [InlineData((byte)7)]
    [InlineData((byte)255)]
    public void TryClamp_OtherRarity_ReturnsFalseAndKeepsValue(byte rarity)
    {
        bool changed = DropRarityClamp.TryClamp(rarity, out byte clamped);

        Assert.False(changed);
        Assert.Equal(rarity, clamped);
    }
}
