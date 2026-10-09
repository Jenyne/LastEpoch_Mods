namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

/// <summary>Combines zone type, arrival protection and cinematics into the HH timer freeze.</summary>
public sealed class HeadhunterZonePause
{
    public const double PollSeconds = 0.25;

    private readonly HeadhunterArrivalWatch _arrival = new();
    private readonly HeadhunterCinematicWatch _cinematic = new();
    private bool _nonCombat;
    private double _nextPoll = double.MaxValue;

    public HeadhunterTimerFreeze Freeze { get; } = new();

    public string Scene => _arrival.Scene;

    public bool IsWatchingArrival => _arrival.IsWatching;

    public HeadhunterPauseChange OnScene(string scene, bool nonCombat, double now)
    {
        _nonCombat = nonCombat;
        _cinematic.Reset();
        _arrival.Begin(scene, nonCombat, now);
        _nextPoll = nonCombat ? double.MaxValue : now + PollSeconds;
        return Freeze.Request(IsPausedNow());
    }

    /// <summary>True in a combat zone when the poll interval passed; schedules the next poll.</summary>
    public bool IsPollDue(double now)
    {
        if (now < _nextPoll)
        {
            return false;
        }

        _nextPoll = now + PollSeconds;
        return true;
    }

    public bool TryEndArrival(HeadhunterArrivalState state, double now, out double heldSeconds)
    {
        if (!_arrival.TryEnd(state, now, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Applies a cinematic start or end to the freeze; true when the flag changed.</summary>
    public bool TryCinematic(bool active, double now, out double heldSeconds)
    {
        if (!_cinematic.TryChange(active, now, out heldSeconds))
        {
            return false;
        }

        Freeze.Request(IsPausedNow());
        return true;
    }

    /// <summary>Drops the arrival watch, the cinematic and kept timers after HH buffs were removed.</summary>
    public void Clear()
    {
        _arrival.Cancel();
        _cinematic.Reset();
        Freeze.Request(IsPausedNow());
        Freeze.ClearTimers();
    }

    private bool IsPausedNow()
    {
        return HeadhunterPauseRule.IsPaused(_nonCombat, _arrival.IsWatching, _cinematic.IsActive);
    }
}
