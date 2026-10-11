using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Tests.Core.ModUI;

public sealed class MenuToggleTests
{
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, true)]
    [InlineData(true, false, true, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(false, false, false, false)]
    public void Next_OutsideGameScene_False(
        bool open,
        bool menuKeyDown,
        bool escapeDown,
        bool hudReady
    )
    {
        Assert.False(MenuToggle.Next(open, false, hudReady, menuKeyDown, escapeDown));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public void Next_HudNotReady_KeepsOpen(bool open, bool menuKeyDown, bool escapeDown)
    {
        Assert.Equal(open, MenuToggle.Next(open, true, false, menuKeyDown, escapeDown));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Next_MenuKeyDown_FlipsOpen(bool open)
    {
        Assert.Equal(!open, MenuToggle.Next(open, true, true, true, false));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Next_EscapeDown_Closes(bool open, bool menuKeyDown)
    {
        Assert.False(MenuToggle.Next(open, true, true, menuKeyDown, true));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Next_NoKey_KeepsOpen(bool open)
    {
        Assert.Equal(open, MenuToggle.Next(open, true, true, false, false));
    }
}
