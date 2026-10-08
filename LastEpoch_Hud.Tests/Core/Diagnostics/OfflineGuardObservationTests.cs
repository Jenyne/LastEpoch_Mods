using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

public sealed class OfflineGuardObservationTests
{
    private static OfflineGuardEvidence Loaded =>
        new(false, false, true, true, true, 10, "first", true);

    private static OfflineGuardObservation Started(bool initialized = true)
    {
        var observation = new OfflineGuardObservation();
        observation.BeginOffline(10, "first", true);
        if (initialized)
            observation.CharacterInitialized(10, "first");
        return observation;
    }

    [Fact]
    public void OfflineFlagsWithoutServiceRequestRemainUnknown()
    {
        var observation = new OfflineGuardObservation();
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Unknown, observation.State);
    }

    [Fact]
    public void AsyncRequestDoesNotTrustAnAlreadyLoadedActor()
    {
        var observation = Started(initialized: false);
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        Assert.Contains("initialization", observation.Reason);
    }

    [Fact]
    public void AllIndependentSignalsProduceOnlyADiagnosticCandidate()
    {
        var observation = Started();
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.OfflineCandidate, observation.State);
        Assert.Contains("diagnostic only", observation.Reason);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void UnavailableEvidenceDoesNotBecomeAPositiveSignal(int unavailable)
    {
        var evidence = unavailable switch
        {
            0 => Loaded with { OnlinePlay = null },
            1 => Loaded with { OnlineSessionEstablished = null },
            2 => Loaded with { InGame = null },
            3 => Loaded with { OfflineNetworkReady = null },
            4 => Loaded with { LocalActorMatches = false },
            _ => Loaded with { CharacterIsOffline = null },
        };
        var observation = Started();
        observation.Observe(evidence);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
    }

    [Fact]
    public void FileStoreExistenceIsRequiredInAdditionToModeFlags()
    {
        var observation = new OfflineGuardObservation();
        observation.BeginOffline(10, "first", false);
        observation.CharacterInitialized(10, "first");
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        Assert.Contains("file data store", observation.Reason);
    }

    [Fact]
    public void DifferentCharacterDoesNotInheritTheRequest()
    {
        var observation = Started();
        observation.Observe(Loaded with { CharacterPointer = 20, CharacterId = "second" });
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        Assert.Contains("does not match", observation.Reason);
    }

    [Fact]
    public void WrongCharacterInitializationCannotCompleteTheRequest()
    {
        var observation = Started(initialized: false);
        observation.CharacterInitialized(20, "second");
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
    }

    [Fact]
    public void CharacterCopyCanCorrelateByIdWithAllOtherSignalsStillRequired()
    {
        var observation = Started(initialized: false);
        observation.CharacterInitialized(20, "first");
        observation.Observe(Loaded with { CharacterPointer = 20 });
        Assert.Equal(OfflineGuardObservationState.OfflineCandidate, observation.State);
        observation.Observe(Loaded with { CharacterPointer = 20, OfflineNetworkReady = false });
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
    }

    [Fact]
    public void NullCharacterDoesNotMatchEvenWithTheSameId()
    {
        var observation = Started();
        Assert.False(observation.MatchesRequestedCharacter(0, "first"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void OnlineEvidenceRevokesAndDoesNotRecoverFromFlagsAlone(int signal)
    {
        var observation = Started();
        observation.Observe(Loaded);
        var evidence = signal switch
        {
            0 => Loaded with { OnlinePlay = true },
            1 => Loaded with { OnlineSessionEstablished = true },
            _ => Loaded with { CharacterIsOffline = false },
        };
        observation.Observe(evidence);
        Assert.Equal(OfflineGuardObservationState.Revoked, observation.State);
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Revoked, observation.State);
    }

    [Fact]
    public void RequestedOnlineTransitionRevokesBeforeLiveFlagsChange()
    {
        var observation = Started();
        observation.Revoke("Online session establishment requested");
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Revoked, observation.State);
    }

    [Fact]
    public void NewRequestRequiresNewInitializationEvenForTheSameCharacter()
    {
        var observation = Started();
        observation.Observe(Loaded);
        int previousEpoch = observation.Epoch;
        observation.BeginOffline(10, "first", true);
        observation.Observe(Loaded);
        Assert.Equal(previousEpoch + 1, observation.Epoch);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        observation.CharacterInitialized(10, "first");
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.OfflineCandidate, observation.State);
    }

    [Fact]
    public void EndSessionClearsCorrelationAndRejectsDelayedInitialization()
    {
        var observation = Started();
        observation.EndSession("Returning to character selection");
        observation.CharacterInitialized(10, "first");
        observation.Observe(Loaded);
        Assert.False(observation.MatchesRequestedCharacter(10, "first"));
        Assert.Equal(OfflineGuardObservationState.Revoked, observation.State);
    }

    [Fact]
    public void LeavingInGameInvalidatesAPreviouslyLoadedCandidate()
    {
        var observation = Started();
        observation.Observe(Loaded);
        observation.Observe(Loaded with { InGame = false });
        Assert.Equal(OfflineGuardObservationState.Revoked, observation.State);
        Assert.False(observation.MatchesRequestedCharacter(10, "first"));
    }

    [Fact]
    public void TemporaryZoneLoadingCanRecoverWithoutANewCharacterEpoch()
    {
        var observation = Started();
        observation.Observe(Loaded);
        int epoch = observation.Epoch;
        observation.Observe(
            Loaded with
            {
                LocalActorMatches = false,
                CharacterPointer = 0,
                CharacterId = null,
            }
        );
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        observation.Observe(Loaded);
        Assert.Equal(epoch, observation.Epoch);
        Assert.Equal(OfflineGuardObservationState.OfflineCandidate, observation.State);
    }

    [Fact]
    public void RevocationCanOnlyRecoverThroughAFreshOfflineRequest()
    {
        var observation = Started();
        observation.Revoke("Online network requested");
        observation.BeginOffline(20, "second", true);
        observation.CharacterInitialized(10, "first");
        observation.Observe(Loaded);
        Assert.Equal(OfflineGuardObservationState.Loading, observation.State);
        observation.CharacterInitialized(20, "second");
        observation.Observe(Loaded with { CharacterPointer = 20, CharacterId = "second" });
        Assert.Equal(OfflineGuardObservationState.OfflineCandidate, observation.State);
    }
}
