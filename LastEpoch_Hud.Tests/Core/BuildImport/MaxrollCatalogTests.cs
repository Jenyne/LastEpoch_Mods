using System.Net;
using System.Net.Http;
using System.Text;
using LastEpoch_Hud.Scripts.Core.BuildImport;

namespace LastEpoch_Hud.Tests.Core.BuildImport;

public sealed class MaxrollCatalogTests
{
    const string Catalog = """
        {
          "abilities":{"rawSkill":{"abilityName":"Readable Skill"}},
          "classes":[{"classID":3,"treeID":"class-tree","masteries":[{"name":"Base Class"},{"name":"Mastery"}]}],
          "skillTrees":{
            "class-tree":{"nodes":{
              "0":{"nodeName":"Base Node","maxPoints":8,"mastery":0,"masteryRequirement":0,"transform":{"x":-4,"y":-5},"requirements":[],"stats":[]},
              "1":{"nodeName":"Mastery Node","maxPoints":5,"mastery":1,"masteryRequirement":10,"transform":{"x":-4,"y":-5},"requirements":[],"stats":[]}}},
            "skill-tree":{"ability":"rawSkill","nodes":{
              "0":{"nodeName":"Root","maxPoints":0,"transform":{},"requirements":[],"stats":[]},
              "1":{"nodeName":"Readable Node","description":"First<br><color=gold>Second</color> &amp; third","maxPoints":3,"transform":{"x":100},"requirements":[{"node":0,"requirement":0}],"stats":[{"statName":"Damage","value":"+5%"}]}}}
          }
        }
        """;

    static MaxrollTreePreview Preview() =>
        MaxrollBuildParser
            .Parse(
                """
                {"class":3,"passives":{"history":[0,0,1],"position":3},
                 "specializedSkills":["rawSkill"],"skillTrees":{"skill-tree":{"history":[1,1],"position":2}}}
                """
            )
            .SelectedVariant.Trees;

    [Fact]
    public void DisplayCatalog_ResolvesNamesPositionsLimitsAndConnections()
    {
        var catalog = MaxrollPlannerCatalog.Parse(Catalog);
        Assert.Equal("Readable Skill", catalog.AbilityName("rawSkill"));
        Assert.Null(catalog.AbilityName("not-present"));
        var view = Assert.Single(catalog.Views(Preview(), false));
        Assert.Equal("Readable Skill", view.Name);
        var node = view.Definition.Nodes[1];
        Assert.Equal("Readable Node", node.Name);
        Assert.Equal(100, node.X);
        Assert.Equal(0, node.Y); // omitted zero coordinates are intentional in the catalog
        Assert.Equal(3, node.MaximumPoints);
        Assert.Equal(2, view.Rank(1));
        Assert.Equal(2, view.TotalPoints);
        Assert.Equal(0, Assert.Single(node.Requirements).NodeId);
        Assert.Equal("First\nSecond & third", node.Description);
        Assert.Equal("+5% Damage", Assert.Single(node.Stats));
    }

    [Fact]
    public void PassiveViews_SeparateMasteriesEvenWhenPositionsOverlap()
    {
        var catalog = MaxrollPlannerCatalog.Parse(Catalog);
        var views = catalog.Views(Preview(), true);
        Assert.Equal(2, views.Count);
        Assert.Equal("Base Class", views[0].Name);
        Assert.Equal("Mastery", views[1].Name);
        Assert.Equal(0, Assert.Single(views[0].Nodes).Id);
        Assert.Equal(1, Assert.Single(views[1].Nodes).Id);
        Assert.Equal(2, views[0].TotalPoints);
        Assert.Equal(1, views[1].TotalPoints);
        Assert.Equal(3, views.Sum(v => v.TotalPoints));
    }

    [Fact]
    public void UnknownAllocations_AreReportedInsteadOfMappedToAnotherNode()
    {
        var preview = MaxrollBuildParser
            .Parse(
                """
                {"skillTrees":{"skill-tree":{"history":[1,900],"position":2},"unknown-tree":{"history":[1],"position":1}}}
                """
            )
            .SelectedVariant.Trees;
        var view = Assert.Single(MaxrollPlannerCatalog.Parse(Catalog).Views(preview, false));
        Assert.True(view.HasUnknownRanks);
        Assert.Equal(1, view.TotalPoints);
        Assert.Equal(2, view.Snapshot.TotalPoints);
    }

    [Fact]
    public void InvalidHistory_NeverBecomesAnAllocatedGraph()
    {
        var preview = MaxrollBuildParser
            .Parse(
                """
                {"skillTrees":{"skill-tree":{"history":[1,null],"position":2}}}
                """
            )
            .SelectedVariant.Trees;
        var view = Assert.Single(MaxrollPlannerCatalog.Parse(Catalog).Views(preview, false));
        Assert.False(view.Snapshot.IsDecoded);
        Assert.Equal(0, view.TotalPoints);
    }

    [Fact]
    public void NonFinitePositions_AreRejected()
    {
        Assert.Throws<FormatException>(() =>
            MaxrollPlannerCatalog.Parse(Catalog.Replace("\"x\":100", "\"x\":1e100"))
        );
    }

    [Fact]
    public void MissingNodeName_IsRejected()
    {
        Assert.Throws<FormatException>(() =>
            MaxrollPlannerCatalog.Parse(Catalog.Replace("Readable Node", ""))
        );
    }

    [Fact]
    public async Task CatalogClient_UsesOnlyThePublicCatalogEndpoint()
    {
        var handler = new Handler(HttpStatusCode.OK, Catalog);
        using var client = new MaxrollCatalogClient(handler);
        var result = await client.RetrieveAsync(TestContext.Current.CancellationToken);
        Assert.Equal(MaxrollCatalogClient.CatalogUri, handler.RequestedUri);
        Assert.Equal(1, handler.Requests);
        Assert.Equal("Readable Skill", result.AbilityName("rawSkill"));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Redirect)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task CatalogClient_HttpFailuresAreNotRetried(HttpStatusCode status)
    {
        var handler = new Handler(status, "Unavailable");
        using var client = new MaxrollCatalogClient(handler);
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.RetrieveAsync(TestContext.Current.CancellationToken)
        );
        Assert.Equal(1, handler.Requests);
    }

    sealed class Handler(HttpStatusCode status, string data) : HttpMessageHandler
    {
        public int Requests { get; private set; }
        public Uri RequestedUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken token
        )
        {
            Requests++;
            RequestedUri = request.RequestUri;
            return Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(data, Encoding.UTF8, "application/json"),
                }
            );
        }
    }
}
