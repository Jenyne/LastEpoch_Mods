using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class MjolnerDescriptionTests
{
    private static readonly Dictionary<string, string> _texts = new()
    {
        [CustomItemLocaleKeys.MjolnerDescriptionProc] = "{0}|{1}|{2}|{3}",
        [CustomItemLocaleKeys.MjolnerDescriptionSocketed] = "{0}|{1}|{2}|{3}|{4}|{5}",
    };

    [Fact]
    public void LightningProc_Template_FilledFromSettings()
    {
        Assert.Equal("105|95|50|100", Proc(_texts, 127.5f, 255f));
    }

    [Fact]
    public void LightningProc_Chance_ScaledFrom255AndTruncated()
    {
        Assert.Equal("105|95|50|0", Proc(_texts, 130f, 2f));
    }

    [Fact]
    public void LightningProc_MissingTemplate_ReturnsNull()
    {
        Assert.Null(Proc(new Dictionary<string, string>(), 127.5f, 255f));
    }

    [Fact]
    public void LightningProc_NullTexts_ReturnsNull()
    {
        Assert.Null(Proc(null, 127.5f, 255f));
    }

    [Fact]
    public void SocketedSkills_Template_FilledFromSettings()
    {
        Assert.Equal("105|95|a|b|c|2", Socketed(_texts, 2000));
    }

    [Fact]
    public void SocketedSkills_FractionalCooldown_InSeconds()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            Assert.Equal("105|95|a|b|c|0.25", Socketed(_texts, 250));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void SocketedSkills_MissingTemplate_ReturnsNull()
    {
        Assert.Null(Socketed(new Dictionary<string, string>(), 2000));
    }

    private static string Proc(IReadOnlyDictionary<string, string> texts, float min, float max) =>
        MjolnerDescription.LightningProc(texts, 105, 95, min, max);

    private static string Socketed(IReadOnlyDictionary<string, string> texts, double cooldownMs) =>
        MjolnerDescription.SocketedSkills(texts, 105, 95, cooldownMs, "a", "b", "c");
}
