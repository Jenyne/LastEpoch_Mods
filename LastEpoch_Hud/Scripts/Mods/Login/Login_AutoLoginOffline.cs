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
    private static bool reportedBlockedClick;
    private static bool dispatchingAutomatic;

    public static void Tick()
    {
        try
        {
            Login_ClientStartup.Tick();
            if (watchingTransition)
            {
                if (Scenes.IsCharacterSelection() && !GameplayEnvironment.IsOnlinePlay)
                {
                    watchingTransition = false;
                    Login_ClientStartup.Stop();
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
            if (landingPanel.IsNullOrDestroyed())
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
                Login_ClientStartup.Ready
                && !watchingTransition
                && controller != null
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
                dispatchingAutomatic = true;
                try
                {
                    panel.OnPlayOfflineClicked();
                }
                finally
                {
                    dispatchingAutomatic = false;
                }
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
                        + "Client state: "
                        + Login_ClientStartup.State
                        + ". Offline selection waits for startup completion."
                );
            }
        }
        catch (Exception ex)
        {
            attempt.MarkAttempted();
            landingPanel = null;
            watchingTransition = false;
            Main.logger_instance?.Warning(
                "[Offline] Auto-selection stopped: "
                    + ex.Message
                    + ". Offline selection still requires startup completion."
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
            Login_ClientStartup.Initialize();
            landingPanel = __instance;
            attempt.BeginVisit();
            visitStarted = Time.realtimeSinceStartup;
            reportedWait = false;
            reportedBlockedClick = false;
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
        static bool Prefix()
        {
            if (
                !Login_ClientStartup.Ready
                || watchingTransition
                || (attempt.Attempted && !dispatchingAutomatic)
            )
            {
                if (!reportedBlockedClick)
                {
                    reportedBlockedClick = true;
                    Main.logger_instance?.Msg(
                        "[Offline] Deferred offline click until client startup is ready; state="
                            + Login_ClientStartup.State
                    );
                }
                return false;
            }
            attempt.MarkAttempted();
            watchingTransition = true;
            transitionStarted = Time.realtimeSinceStartup;
            return true;
        }
    }

    [HarmonyPatch(typeof(LandingZonePanel), "OnPlayOnlineClicked")]
    public class OnlineClicked
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            // Cover controller/events even if the hidden button is invoked directly.
            return false;
        }
    }
}
