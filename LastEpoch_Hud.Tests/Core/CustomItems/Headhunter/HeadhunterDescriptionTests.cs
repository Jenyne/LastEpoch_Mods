using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterDescriptionTests
{
    [Fact]
    public void Text_Template_FilledWithDurationAndMaxStacks()
    {
        var texts = new Dictionary<string, string>
        {
            [CustomItemLocaleKeys.HeadhunterDescription] = "{0}|{1}",
        };

        Assert.Equal("45|7", HeadhunterDescription.Text(texts, 45f, 7));
    }

    [Fact]
    public void Text_MissingTemplate_ReturnsNull()
    {
        Assert.Null(HeadhunterDescription.Text(new Dictionary<string, string>(), 45f, 7));
    }
}
