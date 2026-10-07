using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Detects a change in the ordered stat ids shown on the bar.</summary>
public sealed class HeadhunterBarChangeTracker
{
    private readonly List<int> _last = new();

    public bool Update(IReadOnlyList<HeadhunterBarEntry> entries)
    {
        if (!IsDifferent(entries))
        {
            return false;
        }

        _last.Clear();
        for (int i = 0; i < entries.Count; i++)
        {
            _last.Add(entries[i].StatId);
        }

        return true;
    }

    private bool IsDifferent(IReadOnlyList<HeadhunterBarEntry> entries)
    {
        if (entries.Count != _last.Count)
        {
            return true;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].StatId != _last[i])
            {
                return true;
            }
        }

        return false;
    }
}
