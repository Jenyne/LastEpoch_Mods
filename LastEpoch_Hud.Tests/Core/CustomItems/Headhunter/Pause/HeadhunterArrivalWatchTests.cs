using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterArrivalWatchTests
{
    private const string SceneA = "FakeA";
    private const string SceneB = "FakeB";
    private const double Start = 10;

    [Fact]
    public void Begin_Hostile_WatchesAndReturnsTrue()
    {
        var watch = new HeadhunterArrivalWatch();

        bool assumed = watch.Begin(SceneA, false, Start);

        Assert.True(assumed);
        Assert.True(watch.IsWatching);
    }

    [Fact]
    public void Begin_NonCombat_NotWatchingReturnsFalse()
    {
        var watch = new HeadhunterArrivalWatch();

        bool assumed = watch.Begin(SceneA, true, Start);

        Assert.False(assumed);
        Assert.False(watch.IsWatching);
    }

    [Fact]
    public void Begin_NonCombatWhileWatching_Stops()
    {
        var watch = new HeadhunterArrivalWatch();
        watch.Begin(SceneA, false, Start);

        bool assumed = watch.Begin(SceneB, true, Start + 1);

        Assert.False(assumed);
        Assert.False(watch.IsWatching);
    }

    [Fact]
    public void TryEnd_Protected_KeepsWatching()
    {
        var watch = new HeadhunterArrivalWatch();
        watch.Begin(SceneA, false, Start);

        bool ended = watch.TryEnd(HeadhunterArrivalState.Protected, Start + 1, out _);

        Assert.False(ended);
        Assert.True(watch.IsWatching);
    }

    [Theory]
    [InlineData(HeadhunterArrivalState.Damageable)]
    [InlineData(HeadhunterArrivalState.Missing)]
    public void TryEnd_EndingState_StopsWithHeldSeconds(HeadhunterArrivalState state)
    {
        var watch = new HeadhunterArrivalWatch();
        watch.Begin(SceneA, false, Start);

        bool ended = watch.TryEnd(state, Start + 2.5, out double held);

        Assert.True(ended);
        Assert.Equal(2.5, held, 6);
        Assert.False(watch.IsWatching);
    }

    [Theory]
    [InlineData(HeadhunterArrivalState.Protected)]
    [InlineData(HeadhunterArrivalState.Damageable)]
    [InlineData(HeadhunterArrivalState.Missing)]
    public void TryEnd_NotWatching_False(HeadhunterArrivalState state)
    {
        var watch = new HeadhunterArrivalWatch();

        Assert.False(watch.TryEnd(state, Start, out double held));
        Assert.Equal(0, held);
    }

    [Fact]
    public void Begin_WhileWatching_RestartsForNewScene()
    {
        var watch = new HeadhunterArrivalWatch();
        watch.Begin(SceneA, false, Start);
        watch.Begin(SceneB, false, 20);

        bool ended = watch.TryEnd(HeadhunterArrivalState.Damageable, 21, out double held);

        Assert.True(ended);
        Assert.Equal(1, held, 6);
        Assert.Equal(SceneB, watch.Scene);
    }

    [Fact]
    public void Cancel_Watching_ReturnsTrueAndStops()
    {
        var watch = new HeadhunterArrivalWatch();
        watch.Begin(SceneA, false, Start);

        bool wasWatching = watch.Cancel();

        Assert.True(wasWatching);
        Assert.False(watch.IsWatching);
    }

    [Fact]
    public void Cancel_NotWatching_ReturnsFalse()
    {
        var watch = new HeadhunterArrivalWatch();

        Assert.False(watch.Cancel());
    }
}
