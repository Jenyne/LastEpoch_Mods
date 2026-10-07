using System;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Decides when to look at a config file and whether its last-write stamp changed.</summary>
public sealed class ConfigChangeDetector
{
    private readonly IntervalGate _gate;
    private DateTime? _remembered;

    public ConfigChangeDetector(double intervalSeconds)
    {
        _gate = new IntervalGate(intervalSeconds);
    }

    public void Remember(DateTime? writeTimeUtc)
    {
        _remembered = writeTimeUtc;
    }

    public bool IsCheckDue(double now)
    {
        return _gate.IsDue(now);
    }

    public bool HasChanged(DateTime? writeTimeUtc)
    {
        if (writeTimeUtc == null || writeTimeUtc == _remembered)
        {
            return false;
        }

        _remembered = writeTimeUtc;
        return true;
    }
}
