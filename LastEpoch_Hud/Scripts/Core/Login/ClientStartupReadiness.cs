using System;

namespace LastEpoch_Hud.Scripts.Core.Login;

// Consume the game's completed application-state notification, not a panel's
// early OnEnable or the start of its additive scene load.
public sealed class ClientStartupReadiness
{
    const string Prefix = "ClientStateManager: Application state changed to ";
    public string State { get; private set; }
    public bool ReadyForOfflineClick => State == "Login";

    public bool Observe(string message)
    {
        if (
            message == null
            || !message.StartsWith(Prefix, StringComparison.Ordinal)
            || !message.EndsWith(".", StringComparison.Ordinal)
        )
            return false;
        string state = message.Substring(Prefix.Length, message.Length - Prefix.Length - 1);
        if (string.IsNullOrWhiteSpace(state) || state.Contains('\n') || state.Contains('\r'))
            return false;
        bool changed = State != state;
        State = state;
        return changed;
    }

    public void Reset() => State = null;
}
