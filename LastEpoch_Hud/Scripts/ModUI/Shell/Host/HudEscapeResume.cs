using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI.Shell.Host;

/// <summary>Clicks the game's Resume button when Esc is released while a menu is shown.</summary>
internal static class HudEscapeResume
{
    private static bool _escapePressed;

    public static void Tick()
    {
        if (Refs_Manager.epoch_input_manager.IsNullOrDestroyed())
        {
            return;
        }
        if (Refs_Manager.epoch_input_manager.isControllerActive)
        {
            return;
        }
        if (GamePauseMenu.ResumeButton.IsNullOrDestroyed())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _escapePressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Escape) && _escapePressed)
        {
            GamePauseMenu.Resume();
            _escapePressed = false;
        }
    }
}
