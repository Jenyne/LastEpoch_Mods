using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.Factions;
using LastEpoch_Hud.Scripts.Core.ModUI;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Locales;

/// <summary>Faction card texts exist in base.json.</summary>
public sealed class FactionLocaleTests
{
    [Fact]
    public void CardLabels_HaveBaseKeys()
    {
        string path = Path.Combine(
            GameEnvironment.ModProjectDir,
            "LastEpoch_Hud",
            "Locales",
            "base.json"
        );
        var keys = JsonSerializer
            .Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
            .Keys.ToHashSet();
        var missing = FactionSelection
            .OptionLabels.Concat(FactionCardLabels.All)
            .Select(HudLocaleKey.Canonicalize)
            .Where(label => !keys.Contains(label))
            .ToList();

        Assert.True(missing.Count == 0, $"base.json misses: {string.Join(", ", missing)}");
    }
}
