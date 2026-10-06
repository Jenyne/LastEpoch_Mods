using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class HeadhunterDescriptionTests
{
    [Fact]
    public void Text_Template_FilledWithMinMaxDuration()
    {
        var texts = new Dictionary<string, string>
        {
            [CustomItemLocaleKeys.HeadhunterDescription] = "{0}-{1}-{2}",
        };

        Assert.Equal("1-5-20", HeadhunterDescription.Text(texts, 1, 5, 20f));
    }

    [Fact]
    public void Text_MissingTemplate_ReturnsNull()
    {
        Assert.Null(HeadhunterDescription.Text(new Dictionary<string, string>(), 1, 5, 20f));
    }
}
