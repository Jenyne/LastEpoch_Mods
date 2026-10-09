using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterModelSelectorTests
{
    [Theory]
    [InlineData(false, false, HeadhunterModelSource.Base)]
    [InlineData(false, true, HeadhunterModelSource.Base)]
    [InlineData(true, false, HeadhunterModelSource.Base)]
    [InlineData(true, true, HeadhunterModelSource.Form)]
    public void Pick_FormOnlyWhenTransformedAndAlive(
        bool transformed,
        bool formAlive,
        HeadhunterModelSource expected
    )
    {
        Assert.Equal(expected, HeadhunterModelSelector.Pick(transformed, formAlive));
    }
}
