using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class SceneTravelProgressTests
{
    [Fact]
    public void RejectedLoadReleasesGuardWithoutWaitingForAnOperationThatNeverStarted()
    {
        var travel = new SceneTravelProgress();
        Assert.True(travel.Begin("EoT", "WE502", 0));
        Assert.True(travel.LoadRejected("Scene loading did not start", true, true));
        Assert.Equal("Scene loading did not start", travel.Failure);
        Assert.False(travel.Busy);
        Assert.False(travel.CanUnloadSource);
        Assert.True(travel.Begin("EoT", "Bazaar", 1));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void RejectedLoadCannotReleaseGuardWithoutVerifiedSourceAndAbsentTarget(
        bool sourceRetained,
        bool targetAbsent
    )
    {
        var travel = new SceneTravelProgress();
        travel.Begin("EoT", "CampaignArea", 0);
        Assert.False(travel.LoadRejected("rejected", sourceRetained, targetAbsent));
        Assert.True(travel.Busy);
        Assert.False(travel.CanUnloadSource);
        Assert.False(travel.Begin("EoT", "Bazaar", 1));
    }

    [Fact]
    public void AStartedPlacementOrAcceptedUnloadCannotUseTheRejectedLoadShortcut()
    {
        var travel = new SceneTravelProgress();
        travel.Begin("EoT", "CampaignArea", 0);
        travel.Loaded(1);
        Assert.False(travel.LoadRejected("rejected", true, true));
        Assert.True(travel.Busy);
        travel.Placed(true, 2);
        Assert.False(travel.LoadRejected("rejected", true, true));
        Assert.True(travel.CanUnloadSource);
        Assert.True(travel.Busy);
    }

    [Fact]
    public void SourceCannotUnloadUntilLoadAndVerifiedPlacementComplete()
    {
        var travel = new SceneTravelProgress();
        Assert.False(travel.Placed(true, 0));
        Assert.True(travel.Begin("EoT", "CampaignArea", 0));
        Assert.False(travel.CanUnloadSource);
        Assert.False(travel.Placed(true, 1));
        Assert.True(travel.Loaded(1));
        Assert.False(travel.Placed(false, 2));
        Assert.False(travel.CanUnloadSource);
        Assert.True(travel.Placed(true, 3));
        Assert.True(travel.CanUnloadSource);
        Assert.False(travel.Completed(false));
        Assert.True(travel.Busy);
        Assert.True(travel.Completed(true));
        Assert.False(travel.Busy);
    }

    [Fact]
    public void RapidRepeatedClicksCannotReplaceTheInFlightDestination()
    {
        var travel = new SceneTravelProgress();
        Assert.True(travel.Begin("EoT", "CampaignArea", 0));
        Assert.False(travel.Begin("EoT", "Bazaar", 1));
        Assert.Equal("CampaignArea", travel.Target);
        Assert.Equal("EoT", travel.Source);
    }

    [Theory]
    [InlineData("EoT", "EoT")]
    [InlineData("EoT", "eot")]
    [InlineData("", "EoT")]
    [InlineData("EoT", "PersistentUI")]
    [InlineData("EoT", "PCG_Echo")]
    public void InvalidRequestsRemainIdle(string source, string target)
    {
        var travel = new SceneTravelProgress();
        Assert.False(travel.Begin(source, target, 0));
        Assert.False(travel.Busy);
    }

    [Theory]
    [InlineData("PCG_EchoForest")]
    [InlineData("ArenaWave")]
    [InlineData("MonolithHub")]
    public void GeneratedAreasCanBeSourcesWithoutBecomingDestinations(string source)
    {
        var travel = new SceneTravelProgress();
        Assert.False(TravelSceneRules.IsDestination(source));
        Assert.True(travel.Begin(source, "EoT", 0));
    }

    [Fact]
    public void LoadingTimeoutRetainsTheSourceAndBlocksNewTravelUntilCleanup()
    {
        var travel = new SceneTravelProgress();
        travel.Begin("EoT", "CampaignArea", 10);
        Assert.False(travel.Expired(39, 30));
        Assert.True(travel.Expired(40, 30));
        Assert.True(travel.Recover("load timed out", 40));
        Assert.False(travel.CanUnloadSource);
        Assert.False(travel.Loaded(41));
        Assert.False(travel.Recovered(true, false));
        Assert.False(travel.Recovered(false, true));
        Assert.False(travel.Begin("EoT", "Bazaar", 42));
        Assert.True(travel.Recovered(true, true));
        Assert.True(travel.Begin("EoT", "Bazaar", 43));
    }

    [Fact]
    public void PlacementHasItsOwnDeadlineAndCanRecoverWithoutRetiringTheSource()
    {
        var travel = new SceneTravelProgress();
        travel.Begin("EoT", "CampaignArea", 0);
        travel.Loaded(29);
        Assert.False(travel.Expired(30, 8));
        Assert.True(travel.Expired(37, 8));
        Assert.True(travel.Recover("no spawn", 37));
        Assert.False(travel.Placed(true, 38));
        Assert.False(travel.CanUnloadSource);
        Assert.Equal("no spawn", travel.Failure);
    }

    [Fact]
    public void AcceptedSourceUnloadCannotBeReversedAndStaysBusyUntilFinished()
    {
        var travel = new SceneTravelProgress();
        travel.Begin("EoT", "CampaignArea", 0);
        travel.Loaded(1);
        travel.Placed(true, 2);
        Assert.True(travel.Expired(32, 30));
        Assert.False(travel.Recover("too late", 32));
        Assert.False(travel.Recovered(true, true));
        Assert.True(travel.CanUnloadSource);
        Assert.True(travel.Busy);
        Assert.True(travel.Completed(true));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidClockSamplesDoNotAdvanceATravel(double now)
    {
        var travel = new SceneTravelProgress();
        Assert.False(travel.Begin("EoT", "CampaignArea", now));
        Assert.True(travel.Begin("EoT", "CampaignArea", 5));
        Assert.False(travel.Loaded(now));
        Assert.False(travel.Expired(now, 30));
        Assert.False(travel.Expired(4, 30));
        Assert.False(travel.Expired(100, -1));
    }
}
