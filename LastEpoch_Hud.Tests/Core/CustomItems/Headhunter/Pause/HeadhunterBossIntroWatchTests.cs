using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterBossIntroWatchTests
{
    private const string ActorA = "FakeA";
    private const string ActorB = "FakeB";
    private const double Start = 10;

    private readonly HeadhunterBossIntroWatch _watch = new();

    [Theory]
    [InlineData(4.99f, false)]
    [InlineData(5f, true)]
    [InlineData(12f, true)]
    [InlineData(1.3f, false)]
    public void IsLong_Threshold(float seconds, bool expected)
    {
        Assert.Equal(expected, HeadhunterBossIntroWatch.IsLong(seconds));
    }

    [Fact]
    public void TryStart_Short_Ignored()
    {
        bool started = _watch.TryStart(1, ActorA, 2f, Start);

        Assert.False(started);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryStart_Long_Active()
    {
        bool started = _watch.TryStart(1, ActorA, 12f, Start);

        Assert.True(started);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_Tracked_ReturnsIntroAndHeld()
    {
        _watch.TryStart(1, ActorA, 12f, Start);

        bool ended = _watch.TryEnd(1, 21.5, out HeadhunterBossIntro intro, out double held);

        Assert.True(ended);
        Assert.Equal(new HeadhunterBossIntro(1, ActorA, 12f, Start), intro);
        Assert.Equal(11.5, held, 6);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_Unknown_False()
    {
        _watch.TryStart(1, ActorA, 12f, Start);

        bool ended = _watch.TryEnd(99, 15, out _, out _);

        Assert.False(ended);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryEnd_Twice_SecondFalse()
    {
        _watch.TryStart(1, ActorA, 12f, Start);
        _watch.TryEnd(1, 15, out _, out _);

        Assert.False(_watch.TryEnd(1, 16, out _, out _));
    }

    [Fact]
    public void Overlap_ActiveUntilLastEnds()
    {
        _watch.TryStart(1, ActorA, 12f, Start);
        _watch.TryStart(2, ActorB, 12f, Start);

        _watch.TryEnd(1, 15, out _, out _);
        Assert.True(_watch.IsActive);

        _watch.TryEnd(2, 16, out _, out _);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryStart_SameIdTwice_OneEntry()
    {
        _watch.TryStart(1, ActorA, 12f, Start);
        _watch.TryStart(1, ActorB, 12f, Start + 1);

        _watch.TryEnd(1, 15, out HeadhunterBossIntro intro, out _);

        Assert.Equal(ActorB, intro.Actor);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryExpire_BeforeDeadline_False()
    {
        _watch.TryStart(1, ActorA, 12f, Start);

        bool expired = _watch.TryExpire(23.9, out _, out _);

        Assert.False(expired);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryExpire_AtDeadline_RemovesWithHeld()
    {
        _watch.TryStart(1, ActorA, 12f, Start);

        bool expired = _watch.TryExpire(24, out HeadhunterBossIntro intro, out double held);

        Assert.True(expired);
        Assert.Equal(new HeadhunterBossIntro(1, ActorA, 12f, Start), intro);
        Assert.Equal(14, held, 6);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryExpire_OnlyOverdueOne()
    {
        _watch.TryStart(1, ActorA, 5f, Start);
        _watch.TryStart(2, ActorB, 12f, Start);

        bool first = _watch.TryExpire(18, out HeadhunterBossIntro intro, out _);
        bool second = _watch.TryExpire(18, out _, out _);

        Assert.True(first);
        Assert.Equal(1, intro.Id);
        Assert.False(second);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryExpire_OverdueNotFirst()
    {
        _watch.TryStart(2, ActorB, 12f, Start);
        _watch.TryStart(1, ActorA, 5f, Start);

        bool first = _watch.TryExpire(18, out HeadhunterBossIntro intro, out _);
        bool second = _watch.TryExpire(18, out _, out _);

        Assert.True(first);
        Assert.Equal(new HeadhunterBossIntro(1, ActorA, 5f, Start), intro);
        Assert.False(second);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void Reset_ForgetsAll()
    {
        _watch.TryStart(1, ActorA, 12f, Start);
        _watch.TryStart(2, ActorB, 12f, Start);

        _watch.Reset();

        Assert.False(_watch.IsActive);
        Assert.False(_watch.TryEnd(1, 15, out _, out _));
    }
}
