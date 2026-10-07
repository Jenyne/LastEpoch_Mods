using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterBuffLabelTests
{
    [Fact]
    public void Format_Increased_Percent()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0.1f, false, 1);

        Assert.Contains("+10%", label);
    }

    [Fact]
    public void Format_Added_Flat()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 20f, 0f, false, 1);

        Assert.Contains("+20", label);
        Assert.DoesNotContain("%", label);
    }

    [Fact]
    public void Format_AddedAsPercent()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0.01f, 0f, true, 1);

        Assert.Contains("+1%", label);
    }

    [Fact]
    public void Format_NegativeIncreased_Minus()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, -0.05f, false, 1);

        Assert.Contains("-5%", label);
    }

    [Fact]
    public void Format_ZeroParts_NameOnly()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0f, false, 1);

        Assert.Equal("FakeName", label);
    }

    [Fact]
    public void Format_Both_NameAddedIncreased()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 20f, 0.1f, false, 1);

        int name = label.IndexOf("FakeName", StringComparison.Ordinal);
        int added = label.IndexOf("+20", StringComparison.Ordinal);
        int increased = label.IndexOf("+10%", StringComparison.Ordinal);

        Assert.True(name >= 0);
        Assert.True(name < added);
        Assert.True(added < increased);
    }

    [Fact]
    public void Format_Decimals_MaxTwo()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0.123456f, false, 1);

        Assert.Contains("+12.35%", label);
        Assert.DoesNotContain("12.345", label);
    }

    [Fact]
    public void Format_CommaCulture_UsesDot()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0.025f, false, 1);

            Assert.Contains("+2.5%", label);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Format_AppendsCount_WhenAboveOne()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0.3f, false, 3);

        Assert.Equal("FakeName +30% (x3)", label);
    }

    [Fact]
    public void Format_NoCount_AtOne()
    {
        string label = HeadhunterBuffLabel.Format("FakeName", "FooBar", 0f, 0.3f, false, 1);

        Assert.Equal("FakeName +30%", label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Format_NoGameName_UsesSplitEnumName(string gameName)
    {
        string label = HeadhunterBuffLabel.Format(gameName, "FooBar", 0f, 0.1f, false, 1);

        Assert.StartsWith("Foo Bar", label);
    }

    [Fact]
    public void Format_Tagged_PrefixesGameTagName()
    {
        string label = HeadhunterBuffLabel.Format("Gn", "FooBar", 0f, 0.1f, false, 1, "Gt", "Et");

        Assert.StartsWith("Gt Gn", label);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Format_Tagged_NoGameTagName_UsesEnumTagName(string gameTagName)
    {
        string label = HeadhunterBuffLabel.Format(
            "Gn",
            "FooBar",
            0f,
            0.1f,
            false,
            1,
            gameTagName,
            "Et"
        );

        Assert.StartsWith("Et Gn", label);
    }
}
