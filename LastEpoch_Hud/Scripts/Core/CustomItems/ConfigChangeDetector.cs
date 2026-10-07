using System;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Decides when to look at a config file and whether its last-write stamp changed.</summary>
public sealed class ConfigChangeDetector
{
    private readonly double _intervalSeconds;
    private double _nextCheck;
    private DateTime? _remembered;

    public ConfigChangeDetector(double intervalSeconds)
    {
        _intervalSeconds = intervalSeconds;
    }

    public void Remember(DateTime? writeTimeUtc)
    {
        _remembered = writeTimeUtc;
    }

    public bool IsCheckDue(double now)
    {
        if (now < _nextCheck)
        {
            return false;
        }

        _nextCheck = now + _intervalSeconds;
        return true;
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
