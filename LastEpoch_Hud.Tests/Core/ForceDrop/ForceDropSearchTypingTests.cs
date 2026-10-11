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
    [InlineData("a\bb\rc\nd", "abcd")]
    [InlineData("\u001b", "")]
    [InlineData("\t", "")]
    public void RedirectOnlyPrintableText(string incoming, string expected)
    {
        Assert.Equal(expected, ForceDropSearchTyping.Printable(incoming));
    }
}
