using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.BuildImport;

namespace LastEpoch_Hud.Tests.Core.BuildImport;

public sealed class MaxrollBuildTests
{
    internal const string Data = """
        {"activeProfile":1,"activeEmbed":-1,"embeds":[],
        "items":{"1":{"itemType":0,"subType":1,"uniqueID":0,"uniqueRolls":[1,0.48627450980392156],
          "affixes":[{"id":0,"tier":7,"roll":1}],
          "sealedAffix":{"id":2,"tier":4,"roll":0},
          "primordialAffix":{"id":3,"tier":8,"roll":1},
          "corruptedAffixes":[{"id":1020,"tier":7,"roll":0.5}],"future":{"untouched":true}},
          "2":{"itemType":25,"subType":1,"affixes":[]},
          "999":{"itemType":20,"subType":99}},
        "profiles":[{"name":"Starter","class":0,"items":{"head":1}},
          {"name":"Endgame Gear","class":0,"items":{"head":1,"weapon":null,"altar":{"itemType":37,"subType":0}},
           "idols":[null,2,null,2],"blessings":[null,{"itemType":34,"subType":47,"implicits":[1]}],
           "passives":{"history":[1,1,{"2":3}],"position":2},
           "skillTrees":{"tree":{"history":[3],"position":0}},
           "weaverItems":[],"futureVariant":"preserved"}],"futureBuild":[1,2]}
        """;

    [Theory]
    [InlineData("https://maxroll.gg/last-epoch/planner/Ab12CD34#2", "2")]
    [InlineData(
        "https://www.maxroll.gg/last-epoch/planner/Ab12CD34/?utm_source=test#Endgame%20Gear",
        "Endgame Gear"
    )]
    public void Links_AcceptOnlyPlannerAndPreserveSelection(string input, string fragment)
    {
        var link = MaxrollLink.Parse(input);
        Assert.Equal("Ab12CD34", link.BuildId);
        Assert.Equal(fragment, link.Fragment);
        Assert.Equal(
            "https://planners.maxroll.gg/profiles/le/Ab12CD34",
            link.ProfileEndpoint.ToString()
        );
        Assert.DoesNotContain("utm_", link.CanonicalUri.ToString());
    }

    [Theory]
    [InlineData("http://maxroll.gg/last-epoch/planner/abcd")]
    [InlineData("https://maxroll.gg.evil.test/last-epoch/planner/abcd")]
    [InlineData("https://maxroll.gg@evil.test/last-epoch/planner/abcd")]
    [InlineData("https://user:password@maxroll.gg/last-epoch/planner/abcd")]
    [InlineData("https://maxroll.gg:444/last-epoch/planner/abcd")]
    [InlineData("https://maxroll.gg/last-epoch/build-guides/example")]
    [InlineData("https://maxroll.gg/poe2/planner/abcd")]
    [InlineData("https://maxroll.gg/last-epoch/planner/abcd/extra")]
    [InlineData("https://maxroll.gg/last-epoch/planner/abcd#%0a")]
    public void Links_RejectUnrelatedUrls(string url) =>
        Assert.Throws<FormatException>(() => MaxrollLink.Parse(url));

    [Fact]
    public void ResolveOnlyVariantReferences_PreserveGridAndSpecialAffixes()
    {
        var build = MaxrollBuildParser.Parse(Data);
        Assert.Equal(1, build.SelectedVariantIndex);
        var v = build.SelectedVariant;
        Assert.Empty(v.Issues);
        Assert.Equal(9, v.Placements.Count);
        Assert.Equal(5, v.Placements.Count(p => p.Item.HasValue));
        Assert.DoesNotContain(v.Placements, p => p.Reference == "999");
        var idols = v.Placements.Where(p => p.Section == "idols").ToArray();
        Assert.True(idols[0].IsEmpty);
        Assert.Equal(3, idols[3].GridIndex);
        Assert.Equal("2", idols[1].Reference);
        Assert.Equal("2", idols[3].Reference);
        var item = v.Placements[0].Item.Value;
        Assert.Equal(0, item.GetProperty("uniqueID").GetInt32());
        Assert.Equal(0.48627450980392156, item.GetProperty("uniqueRolls")[1].GetDouble());
        Assert.Equal(8, item.GetProperty("primordialAffix").GetProperty("tier").GetInt32());
        Assert.True(item.GetProperty("future").GetProperty("untouched").GetBoolean());
        Assert.True(item.TryGetProperty("sealedAffix", out _));
        Assert.True(item.TryGetProperty("corruptedAffixes", out _));
        Assert.False(item.TryGetProperty("legendaryPotential", out _));
        Assert.Equal(2, v.Data.GetProperty("passives").GetProperty("position").GetInt32());
        Assert.Equal(3, v.Data.GetProperty("passives").GetProperty("history").GetArrayLength());
        Assert.True(build.Data.TryGetProperty("futureBuild", out _));
    }

    [Theory]
    [InlineData("#1", 0)]
    [InlineData("#2", 1)]
    [InlineData("#Endgame%20Gear", 1)]
    public void Fragment_OverridesStoredActiveProfile(string fragment, int index)
    {
        var build = MaxrollBuildParser.Parse(
            Data,
            MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd" + fragment)
        );
        Assert.Equal(index, build.SelectedVariantIndex);
    }

    [Fact]
    public void UnknownFragment_DoesNotSelectAnotherSet()
    {
        var build = MaxrollBuildParser.Parse(
            Data,
            MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd#99")
        );
        Assert.Null(build.SelectedVariant);
        Assert.NotNull(build.SelectionIssue);
        build.SelectVariant(0);
        Assert.Equal("Starter", build.SelectedVariant.Name);
        Assert.Null(build.SelectionIssue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothServerEnvelopes_DecodeTheSameBuild(bool loader)
    {
        object profile = new
        {
            id = "abcd",
            game = "le",
            name = "Example",
            data = Data,
        };
        string json = loader
            ? JsonSerializer.Serialize(new { profile })
            : JsonSerializer.Serialize(profile);
        var build = MaxrollBuildParser.Parse(
            json,
            MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd#1")
        );
        Assert.Equal("Example", build.Name);
        Assert.Equal("Starter", build.SelectedVariant.Name);
        Assert.Equal(json, build.ResponseJson);
    }

    [Fact]
    public void Clipboard_InlineItemsNeedNoSharedDictionary()
    {
        var build = MaxrollBuildParser.Parse(
            "{\"items\":{\"head\":{\"itemType\":0,\"subType\":1,\"uniqueID\":0}},\"idols\":[null]}"
        );
        Assert.Equal("clipboard", build.SelectedVariant.Kind);
        Assert.Equal(
            0,
            build.SelectedVariant.Placements[0].Item.Value.GetProperty("uniqueID").GetInt32()
        );
    }

    [Fact]
    public void EquipmentEmbed_UsesItsIdNotProfileIndex()
    {
        const string json =
            "{\"profiles\":[],\"items\":{},\"activeEmbed\":0,\"embeds\":[{\"id\":7,\"type\":\"equipment\",\"items\":{}},{\"id\":8,\"type\":\"skilltree\"}]}";
        var link = MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd#7");
        Assert.Equal(7, MaxrollBuildParser.Parse(json, link).SelectedVariant.EmbedId);
        var other = MaxrollBuildParser.Parse(
            json,
            MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd#8")
        );
        Assert.Null(other.SelectedVariant);
    }

    [Theory]
    [InlineData("{\"items\":{\"head\":{\"itemType\":\"bad\",\"subType\":1}}}")]
    [InlineData("{\"profiles\":[{\"items\":{\"head\":9}}],\"items\":{}}")]
    [InlineData(
        "{\"items\":{\"head\":{\"itemType\":0,\"subType\":1,\"affixes\":[{\"id\":1,\"tier\":7,\"roll\":255}]}}}"
    )]
    [InlineData(
        "{\"items\":{\"head\":{\"itemType\":0,\"subType\":1,\"affixes\":[{\"id\":1,\"values\":[20]}]}}}"
    )]
    public void UnresolvedOrUnsupportedData_IsReportedAndRetained(string json)
    {
        var build = MaxrollBuildParser.Parse(json);
        Assert.NotEmpty(build.SelectedVariant.Issues);
        Assert.Equal(json, build.Data.GetRawText());
    }

    [Theory]
    [InlineData("{\"game\":\"poe2\",\"data\":{}}")]
    [InlineData("{\"id\":\"wrong\",\"data\":{}}")]
    [InlineData("{\"items\":{},\"items\":{}}")]
    [InlineData("{\"error\":\"Not found\"}")]
    [InlineData("{}")]
    public void BadEnvelope_IsRejected(string json) =>
        Assert.Throws<FormatException>(() =>
            MaxrollBuildParser.Parse(
                json,
                MaxrollLink.Parse("https://maxroll.gg/last-epoch/planner/abcd")
            )
        );

    [Fact]
    public void OversizedData_IsRejected() =>
        Assert.Throws<FormatException>(() =>
            MaxrollBuildParser.Parse(new string(' ', MaxrollBuildParser.MaximumBytes + 1))
        );
}
