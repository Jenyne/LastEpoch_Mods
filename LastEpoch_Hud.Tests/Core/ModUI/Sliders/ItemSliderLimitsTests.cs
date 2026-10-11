using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

public sealed class ItemSliderLimitsTests
{
    private const int RangeCount = 14;

    [Fact]
    public void EveryRange_HasMinBelowMax()
    {
        SliderRange[] ranges = ItemSliderLimitsReflection.AllRanges();

        Assert.Equal(RangeCount, ranges.Length);
        Assert.All(ranges, range => Assert.True(range.Min < range.Max));
    }

    [Fact]
    public void AffixCount_IsWholeNumbers() =>
        Assert.Equal(new SliderRange(0f, 6f, true), ItemSliderLimits.DropAffixCount);

    [Fact]
    public void IdolAffixCount_MatchesCodeFallback() =>
        Assert.Equal(new SliderRange(0f, 2f, true), ItemSliderLimits.DropIdolAffixCount);
}
