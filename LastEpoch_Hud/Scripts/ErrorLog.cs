using System;
using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Scripts;

/// <summary>Rate-limited error log entry point for the mod.</summary>
public static class ErrorLog
{
    private static readonly ErrorThrottle _throttle = new(ErrorThrottle.DefaultWindowMs);
    private static readonly object _gate = new();

    public static void Report(Exception ex, string context)
    {
        bool shouldLog;
        int suppressed;
        lock (_gate)
        {
            shouldLog = _throttle.TryReport(context, Environment.TickCount64, out suppressed);
        }

        if (!shouldLog)
        {
            return;
        }

        MelonLoader.MelonLogger.Instance logger = Main.logger_instance;
        if (suppressed > 0)
        {
            logger?.Error(ErrorLogText.Repeated(context, suppressed));
        }

        logger?.Error(ErrorLogText.Occurrence(context, ex?.ToString()));
    }
}
