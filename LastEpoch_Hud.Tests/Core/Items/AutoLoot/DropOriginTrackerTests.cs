using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class DropOriginTrackerTests
{
    [Fact]
    public void Current_Default_Game()
    {
        Assert.Equal(DropOrigin.Game, new DropOriginTracker().Current);
    }

    [Fact]
    public void RunAsMod_InsideAction_Mod()
    {
        var tracker = new DropOriginTracker();
        DropOrigin inside = DropOrigin.Game;

        tracker.RunAsMod(() => inside = tracker.Current);

        Assert.Equal(DropOrigin.Mod, inside);
    }

    [Fact]
    public void RunAsMod_AfterReturn_Game()
    {
        var tracker = new DropOriginTracker();

        tracker.RunAsMod(() => { });

        Assert.Equal(DropOrigin.Game, tracker.Current);
    }

    [Fact]
    public void RunAsMod_Nested_ModUntilOuterReturns()
    {
        var tracker = new DropOriginTracker();
        DropOrigin afterInner = DropOrigin.Game;

        tracker.RunAsMod(() =>
        {
            tracker.RunAsMod(() => { });
            afterInner = tracker.Current;
        });

        Assert.Equal(DropOrigin.Mod, afterInner);
        Assert.Equal(DropOrigin.Game, tracker.Current);
    }

    [Fact]
    public void RunAsMod_ActionThrows_RethrowsAndResets()
    {
        var tracker = new DropOriginTracker();

        Assert.Throws<InvalidOperationException>(() =>
            tracker.RunAsMod(() => throw new InvalidOperationException())
        );

        Assert.Equal(DropOrigin.Game, tracker.Current);
    }
}
