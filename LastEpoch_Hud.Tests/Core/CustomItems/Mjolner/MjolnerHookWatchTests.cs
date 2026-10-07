using LastEpoch_Hud.Scripts.Core.CustomItems.Mjolner;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Mjolner;

public sealed class MjolnerHookWatchTests
{
    private const long PlayerA = 5;
    private const long PlayerB = 6;

    [Fact]
    public void ShouldHook_Disabled_False()
    {
        var watch = new MjolnerHookWatch();

        Assert.False(watch.ShouldHook(false, PlayerA, false));
    }

    [Fact]
    public void ShouldHook_NoPlayer_False()
    {
        var watch = new MjolnerHookWatch();

        Assert.False(watch.ShouldHook(true, 0, false));
    }

    [Fact]
    public void ShouldHook_FirstPlayer_True()
    {
        var watch = new MjolnerHookWatch();

        Assert.True(watch.ShouldHook(true, PlayerA, false));
    }

    [Fact]
    public void ShouldHook_SamePlayerAfterHook_False()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.False(watch.ShouldHook(true, PlayerA, true));
    }

    [Fact]
    public void ShouldHook_PlayerChanged_True()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.True(watch.ShouldHook(true, PlayerB, true));
    }

    [Fact]
    public void ShouldHook_HookDead_True()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.True(watch.ShouldHook(true, PlayerA, false));
    }

    [Fact]
    public void ShouldHook_AfterSceneLoad_TrueOnce()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);
        watch.MarkSceneLoaded();
        watch.MarkSceneLoaded();

        Assert.True(watch.ShouldHook(true, PlayerA, true));
        watch.MarkHooked(PlayerA);
        Assert.False(watch.ShouldHook(true, PlayerA, true));
    }

    [Fact]
    public void ShouldHook_SceneLoadWithoutPlayer_Waits()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);
        watch.MarkSceneLoaded();

        Assert.False(watch.ShouldHook(true, 0, false));
        Assert.True(watch.ShouldHook(true, PlayerA, true));
    }

    [Fact]
    public void ShouldHook_SceneLoadWhileDisabled_Waits()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);
        watch.MarkSceneLoaded();

        Assert.False(watch.ShouldHook(false, PlayerA, true));
        Assert.True(watch.ShouldHook(true, PlayerA, true));
    }

    [Fact]
    public void ShouldHook_AfterMarkUnhooked_True()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);
        watch.MarkUnhooked();

        Assert.True(watch.ShouldHook(true, PlayerA, true));
    }

    [Fact]
    public void ShouldUnhook_NothingHooked_False()
    {
        var watch = new MjolnerHookWatch();

        Assert.False(watch.ShouldUnhook(false, 0));
    }

    [Fact]
    public void ShouldUnhook_Disabled_True()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.True(watch.ShouldUnhook(false, PlayerA));
    }

    [Fact]
    public void ShouldUnhook_NoPlayer_True()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.True(watch.ShouldUnhook(true, 0));
    }

    [Theory]
    [InlineData(PlayerA)]
    [InlineData(PlayerB)]
    public void ShouldUnhook_EnabledWithPlayer_False(long playerId)
    {
        MjolnerHookWatch watch = Hooked(PlayerA);

        Assert.False(watch.ShouldUnhook(true, playerId));
    }

    [Fact]
    public void ShouldUnhook_AfterMarkUnhooked_False()
    {
        MjolnerHookWatch watch = Hooked(PlayerA);
        watch.MarkUnhooked();

        Assert.False(watch.ShouldUnhook(false, PlayerA));
    }

    private static MjolnerHookWatch Hooked(long playerId)
    {
        var watch = new MjolnerHookWatch();
        watch.MarkHooked(playerId);
        return watch;
    }
}
