using LastEpoch_Hud.Scripts.Core.Items;

namespace LastEpoch_Hud.Tests.Core.Items;

public sealed class ForgeRejectionObservationTests
{
    [Theory]
    [InlineData("Affix is maxed")]
    [InlineData("Affixe au maximum")]
    [InlineData("최대 등급")]
    public void UsesTheNativeLabelFromThisCheck(string label)
    {
        var check = new ForgeRejectionObservation("maxed-key");
        check.Observe("maxed-key", label);
        Assert.True(check.IsMaxedRejection(false, label));
        Assert.False(check.IsMaxedRejection(true, label));
        Assert.False(check.IsMaxedRejection(false, "Missing shards"));
    }

    [Fact]
    public void UnknownKeysAndLiteralPlaceholdersDoNotAuthorizeAnUpgrade()
    {
        var check = new ForgeRejectionObservation("maxed-key");
        check.Observe("missing-shards", "affix_maxed");
        Assert.False(check.IsMaxedRejection(false, "affix_maxed"));
    }

    [Fact]
    public void OlderChecksAndEmptyLabelsCannotAuthorizeThisCheck()
    {
        var first = new ForgeRejectionObservation("maxed-key");
        first.Observe("maxed-key", "Affix is maxed");
        var second = new ForgeRejectionObservation("maxed-key");
        Assert.False(second.IsMaxedRejection(false, "Affix is maxed"));
        second.Observe("maxed-key", null);
        Assert.False(second.IsMaxedRejection(false, null));
        second.Observe("maxed-key", "");
        Assert.False(second.IsMaxedRejection(false, ""));
        second.Observe("maxed-key", " ");
        Assert.False(second.IsMaxedRejection(false, " "));
    }

    [Fact]
    public void DiagnosticKeysAreDeduplicatedAndBounded()
    {
        var check = new ForgeRejectionObservation("maxed-key");
        for (int i = 0; i < 30; i++)
            check.Observe("key-" + i, "label");
        check.Observe("key-0", "label");
        Assert.Equal(8, check.ObservedKeys.Split(',').Length);
        // Classification still works even after the diagnostic list fills.
        check.Observe("maxed-key", "Affix is maxed");
        Assert.True(check.IsMaxedRejection(false, "Affix is maxed"));
    }
}
