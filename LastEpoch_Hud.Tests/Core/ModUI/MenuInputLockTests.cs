using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Tests.Core.ModUI;

public sealed class MenuInputLockTests
{
    [Fact]
    public void Next_OpenKeyboardLockFree_Takes()
    {
        var inputLock = new MenuInputLock();

        Assert.Equal(
            InputLockAction.Take,
            inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false)
        );
        Assert.True(inputLock.Held);
    }

    [Fact]
    public void Next_OpenKeyboardAlreadyHeld_DoesNothing()
    {
        var inputLock = new MenuInputLock();
        inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false);

        Assert.Equal(
            InputLockAction.None,
            inputLock.Next(menuOpen: true, controllerActive: false, lockActive: true)
        );
    }

    [Fact]
    public void Next_OpenKeyboardHeldLockCleared_TakesAgain()
    {
        var inputLock = new MenuInputLock();
        inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false);

        Assert.Equal(
            InputLockAction.Take,
            inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false)
        );
        Assert.True(inputLock.Held);
    }

    [Fact]
    public void Next_OpenKeyboardGameLock_DoesNotClaim()
    {
        var inputLock = new MenuInputLock();

        Assert.Equal(
            InputLockAction.None,
            inputLock.Next(menuOpen: true, controllerActive: false, lockActive: true)
        );
        Assert.False(inputLock.Held);
    }

    [Fact]
    public void Next_ClosedAfterTake_ReleasesOnce()
    {
        var inputLock = new MenuInputLock();
        inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false);

        Assert.Equal(
            InputLockAction.Release,
            inputLock.Next(menuOpen: false, controllerActive: false, lockActive: true)
        );
        Assert.False(inputLock.Held);
        Assert.Equal(
            InputLockAction.None,
            inputLock.Next(menuOpen: false, controllerActive: false, lockActive: false)
        );
    }

    [Fact]
    public void Next_ClosedGameLock_LeavesItAlone()
    {
        var inputLock = new MenuInputLock();

        Assert.Equal(
            InputLockAction.None,
            inputLock.Next(menuOpen: false, controllerActive: false, lockActive: true)
        );
    }

    [Fact]
    public void Next_OpenControllerAfterTake_Releases()
    {
        var inputLock = new MenuInputLock();
        inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false);

        Assert.Equal(
            InputLockAction.Release,
            inputLock.Next(menuOpen: true, controllerActive: true, lockActive: true)
        );
        Assert.False(inputLock.Held);
    }

    [Fact]
    public void Next_OpenControllerGameLock_LeavesItAlone()
    {
        var inputLock = new MenuInputLock();

        Assert.Equal(
            InputLockAction.None,
            inputLock.Next(menuOpen: true, controllerActive: true, lockActive: true)
        );
    }

    [Fact]
    public void Next_ReopenAfterRelease_TakesAgain()
    {
        var inputLock = new MenuInputLock();
        inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false);
        inputLock.Next(menuOpen: false, controllerActive: false, lockActive: true);

        Assert.Equal(
            InputLockAction.Take,
            inputLock.Next(menuOpen: true, controllerActive: false, lockActive: false)
        );
    }
}
