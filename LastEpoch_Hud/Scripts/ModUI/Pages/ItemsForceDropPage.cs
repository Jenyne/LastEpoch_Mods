using LastEpoch_Hud.Scripts.ModUI.Shell;
using UnityEngine;
using FD = LastEpoch_Hud.Scripts.Hud_Manager.Content.OdlForceDrop;

namespace LastEpoch_Hud.Scripts.ModUI.Pages;

// Items > Force Drop. The feature still reads the original prefab controls as
// its game-facing data model (hidden once at init), while ForceDropBuilder
// supplies the runtime view inside the redesigned HUD shell.
internal static class ItemsForceDropPage
{
    public static void Show()
    {
        if (FD.content_obj.IsNullOrDestroyed())
            return;

        FD.Set_Active(true);
        Hud_Manager.Content.Set_Active();
    }

    public static void Hide()
    {
        FD.Set_Active(false);
        Hud_Manager.Content.Set_Active();
    }
}
