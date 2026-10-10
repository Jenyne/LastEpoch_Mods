using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterCinematicWatchTests
{
    private const double Start = 10;

    private readonly HeadhunterCinematicWatch _watch = new();

    [Fact]
    public void TryChange_Start_TrueHeldZero()
    {
        bool changed = _watch.TryChange(true, Start, out double held);

        Assert.True(changed);
        Assert.Equal(0, held, 6);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryChange_SameValue_False()
    {
        _watch.TryChange(true, Start, out _);

        bool changed = _watch.TryChange(true, Start + 1, out double held);

        Assert.False(changed);
        Assert.Equal(0, held, 6);
        Assert.True(_watch.IsActive);
    }

    [Fact]
    public void TryChange_End_TrueWithHeld()
    {
        _watch.TryChange(true, Start, out _);

        bool changed = _watch.TryChange(false, Start + 2.5, out double held);

        Assert.True(changed);
        Assert.Equal(2.5, held, 6);
        Assert.False(_watch.IsActive);
    }

    [Fact]
    public void TryChange_FalseWhenIdle_False()
    {
        bool changed = _watch.TryChange(false, Start, out double held);

        Assert.False(changed);
        Assert.Equal(0, held, 6);
    }

    [Fact]
    public void TryChange_SecondCinematic_HeldFromOwnStart()
    {
        _watch.TryChange(true, Start, out _);
        _watch.TryChange(false, Start + 1, out _);
        _watch.TryChange(true, 20, out _);

        _watch.TryChange(false, 23, out double held);

        Assert.Equal(3, held, 6);
    }

    [Fact]
    public void Reset_Active_NotActive()
    {
        _watch.TryChange(true, Start, out _);

        _watch.Reset();

        Assert.False(_watch.IsActive);
        Assert.False(_watch.TryChange(false, Start + 2, out _));
    }
}
