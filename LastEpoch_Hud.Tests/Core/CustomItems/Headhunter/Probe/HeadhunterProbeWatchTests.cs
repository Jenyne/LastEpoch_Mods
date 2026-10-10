using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Probe;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Probe;

public sealed class HeadhunterProbeWatchTests
{
    private static readonly HeadhunterProbeFlags _on = new(true, false, false, false);

    [Fact]
    public void IsPollDue_FirstTrueThenThrottled()
    {
        var watch = new HeadhunterProbeWatch();

        Assert.True(watch.IsPollDue(10));
        Assert.False(watch.IsPollDue(10.1));
        Assert.True(watch.IsPollDue(10 + HeadhunterProbeWatch.PollSeconds));
    }

    [Fact]
    public void TryChange_SameFlags_False()
    {
        var watch = new HeadhunterProbeWatch();

        Assert.False(watch.TryChange(default, 1, out _));
    }

    [Fact]
    public void TryChange_NewFlags_TrueWithHeld()
    {
        var watch = new HeadhunterProbeWatch();
        watch.Reset(10);

        Assert.True(watch.TryChange(_on, 12.5, out double first));
        Assert.Equal(2.5, first, 6);
        Assert.False(watch.TryChange(_on, 13, out _));
        Assert.True(watch.TryChange(default, 20, out double second));
        Assert.Equal(7.5, second, 6);
    }

    [Fact]
    public void Reset_ForgetsFlags()
    {
        var watch = new HeadhunterProbeWatch();
        Assert.True(watch.TryChange(_on, 5, out _));

        watch.Reset(6);

        Assert.False(watch.TryChange(default, 7, out _));
        Assert.True(watch.TryChange(_on, 8, out double held));
        Assert.Equal(2, held, 6);
    }
}
