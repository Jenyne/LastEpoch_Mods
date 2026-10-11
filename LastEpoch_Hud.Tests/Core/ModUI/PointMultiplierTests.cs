using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Tests.Core.ModUI;

public sealed class PointMultiplierTests
{
    [Theory]
    [InlineData(-3f, 2)]
    [InlineData(0f, 2)]
    [InlineData(0.9f, 2)]
    [InlineData(1f, 1)]
    [InlineData(5.9f, 5)]
    [InlineData(10f, 10)]
    [InlineData(10.5f, 10)]
    [InlineData(11f, 10)]
    [InlineData(1000f, 10)]
    public void Clamp_ReturnsLegacyMultiplier(float value, int expected)
    {
        Assert.Equal(expected, PointMultiplier.Clamp(value));
    }
}
