using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.BuildImport;

namespace LastEpoch_Hud.Tests.Core.BuildImport;

public sealed class MaxrollTreeTests
{
    static MaxrollTreeSnapshot Decode(string json)
    {
        using var document = JsonDocument.Parse(json);
        return MaxrollTreeParser.Decode(document.RootElement, "tree");
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 1, 0, 1)]
    [InlineData(2, 2, 0, 2)]
    [InlineData(3, 5, 3, 8)]
    [InlineData(4, 5, 4, 9)]
    public void Cursor_SelectsExactStateBeforeNextStep(int position, int one, int two, int total)
    {
        var tree = Decode("{\"history\":[1,1,{\"1\":5,\"2\":3},2],\"position\":" + position + "}");
        Assert.True(tree.IsDecoded);
        Assert.Equal(one, tree.Ranks.GetValueOrDefault(1));
        Assert.Equal(two, tree.Ranks.GetValueOrDefault(2));
        Assert.Equal(total, tree.TotalPoints);
        Assert.Equal(position, tree.Steps.Count);
        Assert.Equal(4, tree.HistoryLength);
        Assert.Empty(tree.Issues);
    }

    [Fact]
    public void BulkSteps_OverwriteListedNodesAndPreserveOtherRanks()
    {
        var tree = Decode("{\"history\":[1,2,2,{\"1\":4},2,{\"1\":0}],\"position\":6}");
        Assert.True(tree.IsDecoded);
        Assert.Equal(0, tree.Ranks[1]);
        Assert.Equal(3, tree.Ranks[2]);
        Assert.Equal(3, tree.TotalPoints);
        Assert.True(tree.Steps[3].IsBulk);
        Assert.Equal(4, tree.Steps[3].RanksAfter[1]);
        Assert.Single(tree.Steps[3].RanksAfter);
        Assert.Equal(5, tree.Steps[5].HistoryIndex);
    }

    [Fact]
    public void RepeatedAndZeroNodeIds_PreserveOrderAndRanks()
    {
        var tree = Decode("{\"history\":[0,4,0,4],\"position\":4,\"futureField\":true}");
        Assert.Equal(2, tree.Ranks[0]);
        Assert.Equal(2, tree.Ranks[4]);
        Assert.Equal(new[] { 0, 4, 0, 4 }, tree.Steps.Select(s => s.RanksAfter.Keys.Single()));
        Assert.True(tree.Source.GetProperty("futureField").GetBoolean());
    }

    [Theory]
    [InlineData("{\"history\":[1]}")]
    [InlineData("{\"history\":[1],\"position\":-1}")]
    [InlineData("{\"history\":[1],\"position\":2}")]
    [InlineData("{\"history\":[1],\"position\":0.5}")]
    [InlineData("{\"history\":[1],\"position\":\"1\"}")]
    [InlineData("{\"history\":{},\"position\":0}")]
    [InlineData("null")]
    [InlineData("{\"history\":[1,null],\"position\":2}")]
    [InlineData("{\"history\":[1,[]],\"position\":2}")]
    [InlineData("{\"history\":[1,-1],\"position\":2}")]
    [InlineData("{\"history\":[1,1.5],\"position\":2}")]
    [InlineData("{\"history\":[1,{\"2\":-1}],\"position\":2}")]
    [InlineData("{\"history\":[1,{\"2\":1.5}],\"position\":2}")]
    [InlineData("{\"history\":[1,{\"02\":1}],\"position\":2}")]
    [InlineData("{\"history\":[1,{\"2\":2147483647},2],\"position\":3}")]
    public void InvalidActiveHistory_NeverExposesPartialAllocations(string json)
    {
        var tree = Decode(json);
        Assert.False(tree.IsDecoded);
        Assert.Empty(tree.Ranks);
        Assert.Empty(tree.Steps);
        Assert.NotEmpty(tree.Issues);
        Assert.Equal(0, tree.TotalPoints);
    }

    [Fact]
    public void UnknownStepsAfterCursor_AreRetainedWithoutChangingCurrentState()
    {
        var tree = Decode("{\"history\":[1,{\"future\":\"unknown\"}],\"position\":1}");
        Assert.True(tree.IsDecoded);
        Assert.Equal(1, tree.TotalPoints);
        Assert.Equal(
            "unknown",
            tree.Source.GetProperty("history")[1].GetProperty("future").GetString()
        );
    }

    [Fact]
    public void Totals_UseWideIntegersWithoutWrapping()
    {
        var tree = Decode("{\"history\":[{\"1\":2147483647,\"2\":2147483647}],\"position\":1}");
        Assert.True(tree.IsDecoded);
        Assert.Equal(4294967294L, tree.TotalPoints);
    }

    [Fact]
    public void VariantSelection_UsesItsOwnTreesAndPreservesSkillSlots()
    {
        var build = MaxrollBuildParser.Parse(
            """
            {"profiles":[
              {"name":"First","class":3,"mastery":2,"level":100,
               "passives":{"history":[1,1],"position":1},
               "skillTrees":{"treeA":{"history":[2,2],"position":2}},
               "activeSkills":[null,"SkillA","SkillA"],"specializedSkills":["SkillB",null]},
              {"name":"Second","passives":{"history":[4,4,4],"position":3}}],
             "activeProfile":0}
            """
        );
        var first = build.SelectedVariant.Trees;
        Assert.Equal(3, first.ClassId);
        Assert.Equal(2, first.MasteryId);
        Assert.Equal(100, first.Level);
        Assert.Equal(1, first.Passives.TotalPoints);
        Assert.Equal("treeA", Assert.Single(first.Skills).PlannerId);
        Assert.Equal(JsonValueKind.Null, first.ActiveSkills[0].ValueKind);
        Assert.Equal("SkillA", first.ActiveSkills[1].GetString());
        Assert.Equal("SkillA", first.ActiveSkills[2].GetString());
        Assert.Equal(JsonValueKind.Null, first.SpecializedSkills[1].ValueKind);
        build.SelectVariant(1);
        Assert.Equal(3, build.SelectedVariant.Trees.Passives.TotalPoints);
        Assert.Empty(build.SelectedVariant.Trees.Skills);
    }

    [Fact]
    public void EquipmentEmbeds_DoNotBorrowAnotherProfilesTrees()
    {
        var build = MaxrollBuildParser.Parse(
            """
            {"profiles":[{"passives":{"history":[1],"position":1}}],
             "embeds":[{"type":"equipment","id":7,"items":{}}],"activeEmbed":0}
            """
        );
        Assert.Equal("equipment-embed", build.SelectedVariant.Kind);
        Assert.Null(build.SelectedVariant.Trees.Passives);
        Assert.Empty(build.SelectedVariant.Trees.Skills);
    }

    [Fact]
    public void MalformedTreeSections_ReportIssuesWithoutBlockingItemCapture()
    {
        var build = MaxrollBuildParser.Parse(
            """
            {"items":{"head":{"itemType":0,"subType":1}},"skillTrees":[],"activeSkills":{} }
            """
        );
        Assert.Single(build.SelectedVariant.Placements);
        Assert.Equal(2, build.SelectedVariant.Trees.Issues.Count);
        Assert.Empty(build.SelectedVariant.Trees.Skills);
    }
}
