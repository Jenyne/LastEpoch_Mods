using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.Diagnostics;

/// <summary>Decides per context whether an error is logged now or only counted.</summary>
public sealed class ErrorThrottle
{
    public const long DefaultWindowMs = 60_000;

    private readonly Dictionary<string, long> _lastLoggedMs = new();
    private readonly Dictionary<string, int> _suppressedCounts = new();
    private readonly long _windowMs;

    public ErrorThrottle(long windowMs)
    {
        _windowMs = windowMs;
    }

    /// <summary>True when the error must be logged now; suppressed is the number of repeats skipped since the last logged one.</summary>
    public bool TryReport(string context, long nowMs, out int suppressed)
    {
        context ??= string.Empty;
        suppressed = 0;
        if (!_lastLoggedMs.TryGetValue(context, out long last))
        {
            _lastLoggedMs[context] = nowMs;
            return true;
        }

        _suppressedCounts.TryGetValue(context, out int count);
        if (nowMs - last < _windowMs)
        {
            _suppressedCounts[context] = count + 1;
            return false;
        }

        suppressed = count;
        _suppressedCounts[context] = 0;
        _lastLoggedMs[context] = nowMs;
        return true;
    }
}
