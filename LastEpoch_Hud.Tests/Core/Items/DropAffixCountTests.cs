using LastEpoch_Hud.Scripts.Core.Items;

namespace LastEpoch_Hud.Tests.Core.Items;

/// <summary>Saved affix-count floats round like the whole-number slider (midpoint to even).</summary>
public sealed class DropAffixCountTests
{
    [Theory]
    [InlineData(2.6f, 3)]
    [InlineData(2.4f, 2)]
    [InlineData(2.5f, 2)]
    [InlineData(3.5f, 4)]
    [InlineData(0f, 0)]
    [InlineData(6f, 6)]
    public void ToWhole_RoundsLikeWholeSlider(float value, int expected) =>
        Assert.Equal(expected, DropAffixCount.ToWhole(value));
}
