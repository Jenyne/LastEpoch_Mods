using System;
using HarmonyLib;
using Il2CppEHG.Multiplayer;
using Il2CppLE.UI.Login;
using Il2CppLE.UI.Login.UnityUI;
using LastEpoch_Hud.Scripts.Core.Login;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Login;

public class Login_AutoLoginOffline
{
    private static readonly OfflineStartupAttempt attempt = new();
    private static LandingZonePanel landingPanel;
    private static float visitStarted;
    private static float transitionStarted;
    private static bool watchingTransition;
    private static bool reportedWait;

    public static bool CanRun()
    {
        var save = Save_Manager.instance;
        return !save.IsNullOrDestroyed()
            && save.initialized
            && save.data.Login.Enable_AutoLoginOffline;
    }

    public static void Tick()
    {
        try
        {
            if (watchingTransition)
            {
                if (Scenes.IsCharacterSelection() && !GameplayEnvironment.IsOnlinePlay)
                {
                    watchingTransition = false;
                    Main.logger_instance?.Msg("[Offline] Offline character selection reached.");
                }
                else if (Time.realtimeSinceStartup - transitionStarted >= 30f)
                {
                    watchingTransition = false;
                    Main.logger_instance?.Warning(
                        "[Offline] Character selection was not confirmed within 30 seconds. "
                            + "No automatic retry will be made; check the game screen and log."
                    );
                }
            }

            // Nothing to search for every frame: the lifecycle hook supplies the panel.
            if (landingPanel.IsNullOrDestroyed() || !CanRun())
                return;

            var panel = landingPanel;
            if (!panel.isActiveAndEnabled || !panel.IsActive)
            {
                attempt.TryBegin(false, Time.realtimeSinceStartup);
                return;
            }

            if (!panel.playOnlineButton.IsNullOrDestroyed())
                panel.playOnlineButton.gameObject.SetActive(false);

            var controller = panel.loginController;
            var offlineButton = panel.playOfflineButton;
            bool ready =
                controller != null
                && controller.FSM != null
                && controller.FSM.CurrentState == LoginController.LoginUIState.LandingZone
                && !panel._transitioningToSteamRequired
                && !offlineButton.IsNullOrDestroyed()
                && offlineButton.isActiveAndEnabled
                && offlineButton.IsInteractable()
                && (
                    panel.offlineActivityIndicator.IsNullOrDestroyed()
                    || !panel.offlineActivityIndicator.activeInHierarchy
                )
                && (
                    panel.onlineActivityIndicator.IsNullOrDestroyed()
                    || !panel.onlineActivityIndicator.activeInHierarchy
                );

            if (attempt.TryBegin(ready, Time.realtimeSinceStartup))
            {
                Main.logger_instance?.Msg(
                    "[Offline] Selecting Play Offline through the game's landing flow."
                );
                // Let EHG perform its async transition and load local characters.
                // Do not force a mode flag or jump scenes ourselves.
                panel.OnPlayOfflineClicked();
            }
            else if (
                !attempt.Attempted
                && !reportedWait
                && Time.realtimeSinceStartup - visitStarted >= 15f
            )
            {
                reportedWait = true;
                Main.logger_instance?.Warning(
                    "[Offline] Still waiting for the landing screen to be ready. "
                        + "Play Offline remains available for manual selection."
                );
            }
        }
        catch (Exception ex)
        {
            attempt.MarkAttempted();
            landingPanel = null;
            watchingTransition = false;
            Main.logger_instance?.Warning(
                "[Offline] Auto-selection stopped: " + ex.Message + ". Use Play Offline manually."
            );
        }
    }

    [HarmonyPatch(typeof(LandingZonePanel), "OnOnEnable")]
    public class LandingReady
    {
        [HarmonyPostfix]
        static void Postfix(LandingZonePanel __instance)
        {
            // Schedule only: the surrounding OnEnable has not finished yet.
            landingPanel = __instance;
            attempt.BeginVisit();
            visitStarted = Time.realtimeSinceStartup;
            reportedWait = false;
        }
    }

    [HarmonyPatch(typeof(LandingZonePanel), "OnDisable")]
    public class LandingClosed
    {
        [HarmonyPostfix]
        static void Postfix(LandingZonePanel __instance)
        {
            if (landingPanel == __instance)
            {
                landingPanel = null;
                attempt.MarkAttempted();
            }
        }
    }

    [HarmonyPatch(typeof(LandingZonePanel), "OnPlayOfflineClicked")]
    public class OfflineClicked
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            attempt.MarkAttempted();
            if (CanRun())
            {
                watchingTransition = true;
                transitionStarted = Time.realtimeSinceStartup;
            }
        }
    }

    [HarmonyPatch(typeof(LandingZonePanel), "OnPlayOnlineClicked")]
    public class OnlineClicked
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            // Cover controller/events even if the hidden button is invoked directly.
            var save = Save_Manager.instance;
            if (save.IsNullOrDestroyed() || !save.initialized)
                return false;
            return !CanRun();
        }
    }
}
