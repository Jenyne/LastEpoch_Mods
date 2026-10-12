using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropSearchTypingTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("lich", "lich")]
    [InlineData(" Skeleton", " Skeleton")]
    [InlineData("Épée", "Épée")]
    [InlineData("骷髅", "骷髅")]
    [InlineData("a\bbc", "abc")]
    [InlineData("\u001b", "")]
    [InlineData("\t", "")]
    public void RedirectOnlyPrintableText(string incoming, string expected)
    {
        Assert.Equal(expected, ForceDropSearchTyping.Redirected(incoming, false));
    }

    // Enter ends the edit in the same frame the IME commits its last syllable,
    // so the field already holds the committed text.
    [Theory]
    [InlineData("사\n")]
    [InlineData("사\r")]
    [InlineData("a\rb")]
    public void SkipFrameWithEnterCharacter(string incoming)
    {
        Assert.Equal("", ForceDropSearchTyping.Redirected(incoming, false));
    }

    [Fact]
    public void SkipFrameWithSubmitKey()
    {
        Assert.Equal("", ForceDropSearchTyping.Redirected("사", true));
    }
}
