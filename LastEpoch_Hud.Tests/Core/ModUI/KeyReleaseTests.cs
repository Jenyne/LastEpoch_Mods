using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Tests.Core.ModUI;

public sealed class KeyReleaseTests
{
    [Fact]
    public void Next_UpWithoutDown_False()
    {
        Assert.False(new KeyRelease().Next(false, true));
    }

    [Fact]
    public void Next_DownThenUpLaterFrame_True()
    {
        var release = new KeyRelease();
        release.Next(true, false);
        release.Next(false, false);

        Assert.True(release.Next(false, true));
    }

    [Fact]
    public void Next_DownAndUpSameFrame_True()
    {
        Assert.True(new KeyRelease().Next(true, true));
    }

    [Fact]
    public void Next_SecondUpAfterRelease_False()
    {
        var release = new KeyRelease();
        release.Next(true, false);
        release.Next(false, true);

        Assert.False(release.Next(false, true));
    }

    [Fact]
    public void Next_DownOnly_False()
    {
        Assert.False(new KeyRelease().Next(true, false));
    }
}
