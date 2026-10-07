using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterAreaMatchTests
{
    private const int StatId = 42;

    [Theory]
    [InlineData(1f)]
    [InlineData(0.9f)]
    public void MoreArea_NormalOrSmaller_Zero(float scale)
    {
        Assert.Equal(0f, HeadhunterAreaMatch.MoreArea(scale));
    }

    [Theory]
    [InlineData(1.1f, 0.21f)]
    [InlineData(1.2f, 0.44f)]
    public void MoreArea_IsSquareMinusOne(float scale, float expected)
    {
        Assert.Equal(expected, HeadhunterAreaMatch.MoreArea(scale), 5);
    }

    [Fact]
    public void TryNext_Normal_NoBuff_False()
    {
        var match = new HeadhunterAreaMatch(StatId);

        Assert.False(match.TryNext(1f, false, out _));
    }

    [Fact]
    public void TryNext_Grown_AddsFullAction()
    {
        var match = new HeadhunterAreaMatch(StatId);

        bool changed = match.TryNext(1.1f, false, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Add(1.1f), action);
    }

    [Fact]
    public void TryNext_SameScaleLive_False()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);

        Assert.False(match.TryNext(1.1f, true, out _));
    }

    [Fact]
    public void TryNext_SameScaleBuffLost_AddsAgain()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);

        bool changed = match.TryNext(1.1f, false, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Add(1.1f), action);
    }

    [Fact]
    public void TryNext_ScaleChanges_AddsNewValue()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);

        bool changed = match.TryNext(1.2f, true, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Add(1.2f), action);
    }

    [Fact]
    public void TryNext_BackToNormalLive_Remove()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);

        bool changed = match.TryNext(1f, true, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Remove(), action);
    }

    [Fact]
    public void TryNext_BackToNormalThenGrownLive_AddsAgain()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);
        match.TryNext(1f, true, out _);

        bool changed = match.TryNext(1.1f, true, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Add(1.1f), action);
    }

    [Fact]
    public void TryNext_NormalStrayBuff_Remove()
    {
        var match = new HeadhunterAreaMatch(StatId);

        bool changed = match.TryNext(1f, true, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Remove(), action);
    }

    [Fact]
    public void Clear_ReturnsRemove()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);

        BuffAction removed = match.Clear();

        Assert.Equal(Remove(), removed);
    }

    [Fact]
    public void Clear_ThenGrownLive_AddsAgain()
    {
        var match = new HeadhunterAreaMatch(StatId);
        match.TryNext(1.1f, false, out _);
        match.Clear();

        bool changed = match.TryNext(1.1f, true, out BuffAction action);

        Assert.True(changed);
        Assert.Equal(Add(1.1f), action);
    }

    private static BuffAction Add(float scale)
    {
        return new BuffAction(
            BuffActionKind.Add,
            HeadhunterAreaMatch.BuffName,
            StatId,
            0f,
            0f,
            HeadhunterAreaMatch.DurationSeconds,
            1,
            More: HeadhunterAreaMatch.MoreArea(scale)
        );
    }

    private static BuffAction Remove()
    {
        return new BuffAction(
            BuffActionKind.Remove,
            HeadhunterAreaMatch.BuffName,
            StatId,
            0f,
            0f,
            0f,
            0
        );
    }
}
