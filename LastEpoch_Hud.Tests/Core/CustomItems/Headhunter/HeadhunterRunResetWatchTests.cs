using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterRunResetWatchTests
{
    private const long PlayerA = 7;
    private const long PlayerB = 9;

    [Fact]
    public void ShouldReset_NoPlayer_ReturnsFalse()
    {
        var watch = new HeadhunterRunResetWatch();

        Assert.False(watch.ShouldReset(0));
    }

    [Fact]
    public void ShouldReset_NoPlayerAfterSceneLoad_ReturnsFalse()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.MarkSceneLoaded();

        Assert.False(watch.ShouldReset(0));
    }

    [Fact]
    public void ShouldReset_FirstPlayer_TrueOnce()
    {
        var watch = new HeadhunterRunResetWatch();

        Assert.True(watch.ShouldReset(PlayerA));
        Assert.False(watch.ShouldReset(PlayerA));
    }

    [Fact]
    public void ShouldReset_AfterSceneLoad_TrueOnceForSamePlayer()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);
        watch.MarkSceneLoaded();

        Assert.True(watch.ShouldReset(PlayerA));
        Assert.False(watch.ShouldReset(PlayerA));
    }

    [Fact]
    public void ShouldReset_SeveralSceneLoads_TrueOnce()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);
        watch.MarkSceneLoaded();
        watch.MarkSceneLoaded();

        Assert.True(watch.ShouldReset(PlayerA));
        Assert.False(watch.ShouldReset(PlayerA));
    }

    [Fact]
    public void ShouldReset_SceneLoadWithoutPlayer_WaitsForPlayer()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);
        watch.MarkSceneLoaded();

        Assert.False(watch.ShouldReset(0));
        Assert.True(watch.ShouldReset(PlayerA));
    }

    [Fact]
    public void ShouldReset_PlayerChanged_TrueOnce()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);

        Assert.True(watch.ShouldReset(PlayerB));
        Assert.False(watch.ShouldReset(PlayerB));
    }

    [Fact]
    public void ShouldReset_SceneLoadAndPlayerChange_TrueOnce()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);
        watch.MarkSceneLoaded();

        Assert.True(watch.ShouldReset(PlayerB));
        Assert.False(watch.ShouldReset(PlayerB));
    }

    [Fact]
    public void ShouldReset_SamePlayerWithoutLoad_StaysFalse()
    {
        var watch = new HeadhunterRunResetWatch();
        watch.ShouldReset(PlayerA);

        Assert.False(watch.ShouldReset(PlayerA));
        Assert.False(watch.ShouldReset(PlayerA));
    }
}
