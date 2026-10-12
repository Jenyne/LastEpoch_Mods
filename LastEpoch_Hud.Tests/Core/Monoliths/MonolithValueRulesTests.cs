using System.Globalization;
using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithValueRulesTests
{
    [Fact]
    public void TryParse_Number_ReturnsValue()
    {
        bool ok = MonolithValueRules.TryParse("1234", out int value);

        Assert.True(ok);
        Assert.Equal(1234, value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("99999999999")]
    public void TryParse_Invalid_ReturnsFalse(string text)
    {
        bool ok = MonolithValueRules.TryParse(text, out int value);

        Assert.False(ok);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryParse_Negative_ClampsToZero()
    {
        bool ok = MonolithValueRules.TryParse("-5", out int value);

        Assert.True(ok);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryParse_AboveMax_ClampsToMax()
    {
        bool ok = MonolithValueRules.TryParse("10000000", out int value);

        Assert.True(ok);
        Assert.Equal(MonolithValueRules.MaxValue, value);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(MonolithValueRules.MaxValue + 1, MonolithValueRules.MaxValue)]
    public void Clamp_Range(int input, int expected)
    {
        Assert.Equal(expected, MonolithValueRules.Clamp(input));
    }

    [Fact]
    public void MaxValue_FitsCharacterLimit()
    {
        Assert.Equal(
            MonolithValueRules.CharacterLimit,
            MonolithValueRules.MaxValue.ToString(CultureInfo.InvariantCulture).Length
        );
    }
}
