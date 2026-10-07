namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Says when the tooltip must be redrawn.</summary>
public sealed class HeadhunterHoverTracker
{
    private int _statId = -1;
    private int _layoutVersion;

    public bool Changed(int statId, int layoutVersion)
    {
        if (statId == _statId && layoutVersion == _layoutVersion)
        {
            return false;
        }

        _statId = statId;
        _layoutVersion = layoutVersion;
        return true;
    }
}
