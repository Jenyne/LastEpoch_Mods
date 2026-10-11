using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI.Shell.Host;

/// <summary>Applies the F3, Esc and scene rules to the mod-menu open flag.</summary>
internal static class HudMenuToggle
{
    public static readonly KeyCode MenuKey = KeyCode.F3;

    public static bool Next(bool open, bool hudReady)
    {
        if (!Scenes.IsGameScene())
        {
            return false;
        }
        if (!hudReady)
        {
            return open;
        }
        if (Input.GetKeyDown(MenuKey))
        {
            open = !open;
        }
        if (open && Input.GetKeyDown(KeyCode.Escape))
        {
            return false;
        }
        return open;
    }
}
