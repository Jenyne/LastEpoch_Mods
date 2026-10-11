using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class AutoLootSettingsTests
{
    [Fact]
    public void AnyEnabled_AllOff_False()
    {
        Assert.False(default(AutoLootSettings).AnyEnabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AnyEnabled_EachToggleAlone_True(int index)
    {
        Assert.True(Only(index).AnyEnabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void UsesFilter_OnlyDirectToggles_False(int index)
    {
        Assert.False(Only(index).UsesFilter);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void UsesFilter_EachFilterToggleAlone_True(int index)
    {
        Assert.True(Only(index).UsesFilter);
    }

    private static AutoLootSettings Only(int index)
    {
        return new AutoLootSettings(
            index == 0,
            index == 1,
            index == 2,
            index == 3,
            index == 4,
            index == 5
        );
    }
}
