namespace LastEpoch_Hud.Scripts.Core.Login;

// Require continuous readiness, then consume the attempt before calling game code.
// A manual click also consumes it so LateUpdate cannot start a second transition.
public sealed class OfflineStartupAttempt
{
    private int readyFrames;
    private double readySince;
    public bool Attempted { get; private set; }

    public void BeginVisit()
    {
        Attempted = false;
        readyFrames = 0;
    }

    public void MarkAttempted() => Attempted = true;

    public bool TryBegin(bool ready, double now)
    {
        if (Attempted)
            return false;
        if (!ready)
        {
            readyFrames = 0;
            return false;
        }
        if (readyFrames++ == 0)
            readySince = now;
        if (readyFrames < 2 || now - readySince < 0.25)
            return false;
        MarkAttempted();
        return true;
    }
}
