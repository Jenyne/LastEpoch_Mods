namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Says when Headhunter run state must reset: after a scene load or a player change, once a player exists. It is the trigger memory itself and never resets.</summary>
public sealed class HeadhunterRunResetWatch
{
    private bool _sceneLoaded;
    private long _playerId;

    /// <summary>Flags a scene load.</summary>
    public void MarkSceneLoaded()
    {
        _sceneLoaded = true;
    }

    /// <summary>True once per scene load or player change. 0 means no player yet.</summary>
    public bool ShouldReset(long playerId)
    {
        if (playerId == 0)
        {
            return false;
        }

        if (!_sceneLoaded && playerId == _playerId)
        {
            return false;
        }

        _sceneLoaded = false;
        _playerId = playerId;
        return true;
    }
}
