using LastEpoch_Hud.Scripts.Core;

namespace LastEpoch_Hud.Tests.Core;

public class ProphecyRewardCountTests
{
    [Theory]
    [InlineData(2, 2)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    [InlineData(100, 10)]
    [InlineData(-1, 1)]
    [InlineData(float.NaN, 1)]
    [InlineData(float.PositiveInfinity, 1)]
    public void Factor_ClampsAndRejectsNonfinite(float value, int expected) =>
        Assert.Equal(expected, ProphecyRewardCount.Factor(value));

    [Fact]
    public void Count_RestoresWhenTriggerThrows()
    {
        int count = 3;
        Assert.Throws<InvalidOperationException>(
            (Action)(
                () =>
                {
                    using var scope = ProphecyRewardCount.Begin(101, count, 5, n => count = n);
                    Assert.Equal(15, count);
                    throw new InvalidOperationException("native trigger failed");
                }
            )
        );
        Assert.Equal(3, count);
    }

    [Fact]
    public void SharedReward_IsNotMultipliedAgainInNestedTrigger()
    {
        int count = 3;
        var outer = ProphecyRewardCount.Begin(102, count, 2, n => count = n);
        Assert.Equal(6, count);
        Assert.Null(ProphecyRewardCount.Begin(102, count, 5, _ => Assert.Fail("nested write")));
        outer.Dispose();
        outer.Dispose();
        Assert.Equal(3, count);
        using var later = ProphecyRewardCount.Begin(102, count, 5, n => count = n);
        Assert.Equal(15, count);
    }

    [Fact]
    public void DifferentReward_RestoresIndependently()
    {
        int first = 2,
            second = 4;
        using (var outer = ProphecyRewardCount.Begin(103, first, 2, n => first = n))
        {
            using (var inner = ProphecyRewardCount.Begin(104, second, 5, n => second = n))
                Assert.Equal(20, second);
            Assert.Equal(4, second);
            Assert.Equal(4, first);
        }
        Assert.Equal(2, first);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(3, 1)]
    [InlineData(int.MaxValue, 10)]
    public void UnsupportedCount_LeavesOriginalAlone(int count, float multiplier) =>
        Assert.Null(
            ProphecyRewardCount.Begin(105, count, multiplier, _ => Assert.Fail("unexpected write"))
        );

    [Fact]
    public void FailingSetter_RestoresAndReleasesReward()
    {
        int count = 3;
        Assert.Throws<InvalidOperationException>(() =>
            ProphecyRewardCount.Begin(
                106,
                count,
                5,
                n =>
                {
                    count = n;
                    if (n != 3)
                        throw new InvalidOperationException("write failed");
                }
            )
        );
        Assert.Equal(3, count);
        using var retry = ProphecyRewardCount.Begin(106, count, 2, n => count = n);
        Assert.Equal(6, count);
    }
}
