using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppEHG.Multiplayer;
using Il2CppLE.UI.MTXStore;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics;

/// <summary>Offline: swallows flyout populate errors and hides the loading overlay.</summary>
[HarmonyPatch(
    typeof(CosmeticSelectionFlyoutPanelUI),
    nameof(CosmeticSelectionFlyoutPanelUI.PopulateFlyoutWindowAsync)
)]
public class CosmeticFlyoutPopulatePatch
{
    [HarmonyFinalizer]
    private static Exception Finalizer(
        Exception __exception,
        CosmeticSelectionFlyoutPanelUI __instance
    )
    {
        if (GameplayEnvironment.IsOnlinePlay)
        {
            return __exception;
        }

        if (__exception == null)
        {
            return null;
        }

        Main.logger_instance?.Warning(
            "PopulateFlyoutWindowAsync exception suppressed: " + __exception.Message
        );
        if (!__instance.IsNullOrDestroyed() && !__instance._loadingOverlay.IsNullOrDestroyed())
        {
            __instance._loadingOverlay.SetActive(false);
        }
        return null;
    }
}
