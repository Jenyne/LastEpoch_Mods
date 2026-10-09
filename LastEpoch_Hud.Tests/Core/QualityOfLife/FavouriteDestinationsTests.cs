using LastEpoch_Hud.Scripts.Core.QualityOfLife;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class FavouriteDestinationsTests
{
    [Fact]
    public void RoundTripPreservesOrderAndDistinctDestinations()
    {
        var favourites = new FavouriteDestinations();
        Assert.True(favourites.Add("EoT"));
        Assert.True(favourites.Add("Bazaar"));
        Assert.False(favourites.Add("EoT"));
        var restored = FavouriteDestinations.FromJson(favourites.ToJson());
        Assert.Equal(new[] { "EoT", "Bazaar" }, restored.Scenes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(" EoT")]
    [InlineData("EoT\n")]
    [InlineData("../EoT")]
    [InlineData("<b>EoT</b>")]
    public void InvalidSceneValuesAreRejected(string scene)
    {
        Assert.False(new FavouriteDestinations().Add(scene));
    }

    [Fact]
    public void OversizedAndMalformedEntriesDoNotConsumeSlotsOnLoad()
    {
        var array = new JArray(
            null,
            7,
            true,
            new string('x', 161),
            "<b>EoT</b>",
            "EoT",
            "EoT",
            "Bazaar"
        );
        var restored = FavouriteDestinations.FromJson(
            new JObject { ["Scenes"] = array }.ToString()
        );
        Assert.Equal(new[] { "EoT", "Bazaar" }, restored.Scenes);
    }

    [Fact]
    public void CapacityIsBoundedAndRemovingOneAllowsAReplacement()
    {
        var favourites = new FavouriteDestinations();
        for (int i = 0; i < FavouriteDestinations.Capacity; i++)
            Assert.True(favourites.Add("Area" + i));
        Assert.False(favourites.Add("Area8"));
        Assert.True(favourites.Remove("Area3"));
        Assert.True(favourites.Add("Area8"));
        Assert.False(favourites.Remove("Missing"));
        Assert.Equal(8, favourites.Scenes.Count);
        Assert.Equal("Area8", favourites.Scenes[^1]);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{}")]
    [InlineData("{\"Scenes\":\"EoT\"}")]
    public void CorruptFilesDoNotSilentlyBecomeAnEmptyValidFile(string json)
    {
        Assert.ThrowsAny<Exception>(() => FavouriteDestinations.FromJson(json));
    }

    [Fact]
    public void DiskSaveReplacesPreviousContentsWithReadableJsonAndNoTemporaryFile()
    {
        string folder = Path.Combine(Path.GetTempPath(), "le-favourites-" + Guid.NewGuid());
        string path = Path.Combine(folder, "FavouriteTeleports.json");
        try
        {
            var favourites = new FavouriteDestinations();
            favourites.Add("EoT");
            favourites.Save(path);
            favourites.Add("Bazaar");
            favourites.Save(path);
            Assert.Equal(
                new[] { "EoT", "Bazaar" },
                FavouriteDestinations.FromJson(File.ReadAllText(path)).Scenes
            );
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }
}
