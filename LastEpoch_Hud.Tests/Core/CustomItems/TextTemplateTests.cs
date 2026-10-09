using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class TextTemplateTests
{
    [Fact]
    public void Fill_Placeholders_Filled()
    {
        Assert.Equal("1 to 5", TextTemplate.Fill("{0} to {1}", 1, 5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Fill_NullOrEmptyTemplate_ReturnsNull(string template)
    {
        Assert.Null(TextTemplate.Fill(template, 1));
    }

    [Fact]
    public void Fill_MissingArgument_ReturnsNull()
    {
        Assert.Null(TextTemplate.Fill("{3}", 1, 2));
    }

    [Fact]
    public void Fill_MalformedTemplate_ReturnsNull()
    {
        Assert.Null(TextTemplate.Fill("{x", 1));
    }
}
