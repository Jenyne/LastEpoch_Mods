using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class HeadhunterDescriptionTests
{
    [Fact]
    public void Text_Template_FilledWithDuration()
    {
        var texts = new Dictionary<string, string>
        {
            [CustomItemLocaleKeys.HeadhunterDescription] = "{0}",
        };

        Assert.Equal("45", HeadhunterDescription.Text(texts, 45f));
    }

    [Fact]
    public void Text_MissingTemplate_ReturnsNull()
    {
        Assert.Null(HeadhunterDescription.Text(new Dictionary<string, string>(), 45f));
    }
}
