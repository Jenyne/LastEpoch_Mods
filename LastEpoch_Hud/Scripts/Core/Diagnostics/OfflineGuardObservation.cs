namespace LastEpoch_Hud.Scripts.Core.Diagnostics;

// Diagnostic classification only. This is deliberately not a mutation permission API.
public enum OfflineGuardObservationState
{
    Unknown,
    Loading,
    OfflineCandidate,
    Revoked,
}

public readonly record struct OfflineGuardEvidence(
    bool? OnlinePlay,
    bool? OnlineSessionEstablished,
    bool? InGame,
    bool? OfflineNetworkReady,
    bool LocalActorMatches,
    long CharacterPointer,
    string CharacterId,
    bool? CharacterIsOffline
);

// The runtime adapter serializes access. Ids/pointers are compared but never logged.
public sealed class OfflineGuardObservation
{
    private long requestedPointer;
    private string requestedId;
    private bool fileStorePresent;
    private bool characterInitialized;
    private bool wasCandidate;
    private string veto;

    public int Epoch { get; private set; }
    public OfflineGuardObservationState State { get; private set; }
    public string Reason { get; private set; } = "No offline character play request observed";

    public void BeginOffline(long pointer, string id, bool hasFileStore)
    {
        Epoch++;
        requestedPointer = pointer;
        requestedId = id;
        fileStorePresent = hasFileStore;
        characterInitialized = false;
        wasCandidate = false;
        veto = null;
        Set(
            OfflineGuardObservationState.Loading,
            "Offline StartPlay requested; awaiting live evidence"
        );
    }

    public bool MatchesRequestedCharacter(long pointer, string id) =>
        requestedPointer != 0
        && pointer != 0
        && (
            pointer == requestedPointer || (!string.IsNullOrEmpty(requestedId) && id == requestedId)
        );

    public void CharacterInitialized(long pointer, string id)
    {
        if (MatchesRequestedCharacter(pointer, id))
            characterInitialized = true;
    }

    public void Revoke(string reason)
    {
        veto = reason;
        Set(OfflineGuardObservationState.Revoked, reason);
    }

    public void EndSession(string reason)
    {
        Epoch++;
        requestedPointer = 0;
        requestedId = null;
        characterInitialized = false;
        wasCandidate = false;
        Revoke(reason);
    }

    public void Observe(OfflineGuardEvidence evidence)
    {
        if (evidence.OnlinePlay == true)
            Revoke("GameplayEnvironment reports online play");
        else if (evidence.OnlineSessionEstablished == true)
            Revoke("Online access reports an established session");
        else if (wasCandidate && evidence.InGame == false && veto == null)
            EndSession("Loaded offline candidate left InGame state");

        if (veto != null)
            return;
        if (requestedPointer == 0)
        {
            Set(OfflineGuardObservationState.Unknown, "No offline character play request observed");
            return;
        }

        string missing = null;
        if (!fileStorePresent)
            missing = "Offline file data store not observed";
        else if (evidence.OnlinePlay != false)
            missing = "Gameplay mode unavailable";
        else if (evidence.OnlineSessionEstablished != false)
            missing = "Online session state unavailable";
        else if (evidence.InGame != true)
            missing = "Client has not reached InGame state";
        else if (evidence.OfflineNetworkReady != true)
            missing = "Live offline network initialization not confirmed";
        else if (!evidence.LocalActorMatches)
            missing = "Live local actor and tracker not matched";
        else if (!MatchesRequestedCharacter(evidence.CharacterPointer, evidence.CharacterId))
            missing = "Loaded character does not match this offline play request";
        else if (!characterInitialized)
            missing = "Matching character initialization not observed after StartPlay";
        else if (evidence.CharacterIsOffline == false)
        {
            Revoke("Matching loaded character reports IsOffline=false");
            return;
        }
        else if (evidence.CharacterIsOffline != true)
            missing = "Loaded character offline marker unavailable";

        if (missing != null)
            Set(OfflineGuardObservationState.Loading, missing);
        else
        {
            wasCandidate = true;
            Set(
                OfflineGuardObservationState.OfflineCandidate,
                "Offline signals agree; diagnostic only"
            );
        }
    }

    private void Set(OfflineGuardObservationState state, string reason)
    {
        State = state;
        Reason = reason;
    }
}
