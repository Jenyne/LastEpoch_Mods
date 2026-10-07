using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterGrowthLogTests
{
    [Fact]
    public void Format_HasTotalAndTwoDecimalFactor()
    {
        string line = HeadhunterGrowthLog.Format(2, 1.02f);

        Assert.Contains("total=2", line);
        Assert.Contains("factor=1.02", line);
    }

    [Fact]
    public void Format_WholeFactor_NoDecimals()
    {
        string line = HeadhunterGrowthLog.Format(1, 1f);

        Assert.Contains("factor=1", line);
        Assert.DoesNotContain("factor=1.", line);
    }

    [Fact]
    public void Format_CommaCulture_UsesDot()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            Assert.Contains("factor=1.04", HeadhunterGrowthLog.Format(3, 1.04f));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
