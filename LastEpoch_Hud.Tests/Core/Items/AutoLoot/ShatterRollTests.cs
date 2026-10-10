using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class ShatterRollTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Triggers_ChanceZeroOrLess_FalseNoRoll(int chance)
    {
        var random = new FakeShatterRandom();
        Assert.False(ShatterRoll.Triggers(chance, random));
        Assert.Equal(0, random.Calls);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(150)]
    public void Triggers_ChanceHundredOrMore_TrueNoRoll(int chance)
    {
        var random = new FakeShatterRandom();
        Assert.True(ShatterRoll.Triggers(chance, random));
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void Triggers_RollBelowChance_True()
    {
        var random = new FakeShatterRandom(29);
        Assert.True(ShatterRoll.Triggers(30, random));
        Assert.Equal(1, random.Calls);
    }

    [Fact]
    public void Triggers_RollAtChance_False()
    {
        var random = new FakeShatterRandom(30);
        Assert.False(ShatterRoll.Triggers(30, random));
        Assert.Equal(1, random.Calls);
    }

    [Fact]
    public void ShardsFor_AffixIdZero_ZeroNoRoll()
    {
        var random = new FakeShatterRandom();
        Assert.Equal(0, ShatterRoll.ShardsFor(0, 100, 100, random));
        Assert.Equal(0, random.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ShardsFor_AffixChanceZeroOrLess_ZeroNoRoll(int affixChance)
    {
        var random = new FakeShatterRandom();
        Assert.Equal(0, ShatterRoll.ShardsFor(7, affixChance, 100, random));
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void ShardsFor_AffixRollFails_ZeroOneRoll()
    {
        var random = new FakeShatterRandom(50);
        Assert.Equal(0, ShatterRoll.ShardsFor(7, 50, 100, random));
        Assert.Equal(1, random.Calls);
    }

    [Fact]
    public void ShardsFor_QuantityChanceZero_OneShard()
    {
        var random = new FakeShatterRandom();
        Assert.Equal(1, ShatterRoll.ShardsFor(7, 100, 0, random));
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void ShardsFor_QuantityChanceHundred_TwoShards()
    {
        var random = new FakeShatterRandom();
        Assert.Equal(2, ShatterRoll.ShardsFor(7, 100, 100, random));
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void ShardsFor_QuantityRollBelow_Two()
    {
        var random = new FakeShatterRandom(39);
        Assert.Equal(2, ShatterRoll.ShardsFor(7, 100, 40, random));
        Assert.Equal(1, random.Calls);
    }

    [Fact]
    public void ShardsFor_QuantityRollAt_One()
    {
        var random = new FakeShatterRandom(40);
        Assert.Equal(1, ShatterRoll.ShardsFor(7, 100, 40, random));
        Assert.Equal(1, random.Calls);
    }

    [Fact]
    public void ShardsFor_RollOrder_AffixThenQuantityOne()
    {
        var random = new FakeShatterRandom(10, 90);
        Assert.Equal(1, ShatterRoll.ShardsFor(7, 50, 50, random));
        Assert.Equal(2, random.Calls);
    }

    [Fact]
    public void ShardsFor_RollOrder_AffixThenQuantityTwo()
    {
        var random = new FakeShatterRandom(10, 10);
        Assert.Equal(2, ShatterRoll.ShardsFor(7, 50, 50, random));
        Assert.Equal(2, random.Calls);
    }

    [Fact]
    public void ShardsFor_RollOrder_AffixFailsSkipsQuantity()
    {
        var random = new FakeShatterRandom(90, 10);
        Assert.Equal(0, ShatterRoll.ShardsFor(7, 50, 50, random));
        Assert.Equal(1, random.Calls);
    }
}
