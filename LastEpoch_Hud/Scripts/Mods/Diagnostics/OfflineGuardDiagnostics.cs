using System;
using System.Collections.Generic;
using System.Threading;
using HarmonyLib;
using Il2Cpp;
using Il2CppClientAppState;
using Il2CppEHG.Multiplayer;
using Il2CppLE.Data;
using Il2CppLE.Networking;
using Il2CppLE.Services;
using Il2CppLE.Services.OnlineAccess;
using LastEpoch_Hud.Scripts.Core.Diagnostics;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Diagnostics;

// Observers only: no skipped originals, changed arguments/results, saves, or mode writes.
// Startup UI behavior is inherited from the separate auto-offline test branch.
public static class OfflineGuardDiagnostics
{
    private static readonly object sync = new();
    private static readonly OfflineGuardObservation observation = new();
    private static readonly Queue<string> messages = new();
    private static OnlineAccessService onlineAccess;
    private static NetworkServiceGroup offlineNetwork;
    private static bool offlineNetworkInitialized;
    private static float nextSample;
    private static float nextHeartbeat;
    private static string lastSnapshot;
    private static int droppedMessages;
    private static int probeErrors;
    private static int offlineSaveRequests;
    private static int saveOfflineDataRequests;

    public static void Initialize() =>
        Safe(() => Log("Enabled; observation only, mutation protection is not installed."));

    // Hooks may run outside the Unity update thread. Log through the bounded queue on Tick.
    private static void Safe(Action action)
    {
        lock (sync)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                probeErrors++;
                if (probeErrors == 1)
                    Log("Observer read failed (" + ex.GetType().Name + "); details omitted.");
            }
        }
    }

    private static void Log(string message)
    {
        if (messages.Count < 128)
            messages.Enqueue(message);
        else
            droppedMessages++;
    }

    private static T? Read<T>(Func<T> getter)
        where T : struct
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static string Flag(bool? value) =>
        value.HasValue ? (value.Value ? "true" : "false") : "unknown";

    public static void SceneEvent(string kind, string name) =>
        Safe(() =>
            Log("Scene " + kind + ": " + name + "; scene changes alone do not revoke an epoch.")
        );

    public static void Tick()
    {
        float now = Time.realtimeSinceStartup;
        if (now >= nextSample)
        {
            nextSample = now + 0.5f;
            Safe(() => Sample(now));
        }

        string[] pending;
        lock (sync)
        {
            pending = messages.ToArray();
            messages.Clear();
        }
        foreach (string message in pending)
            Main.logger_instance?.Msg("[OfflineGuard] " + message);
    }

    private static void Sample(float now)
    {
        bool? online = Read(() => GameplayEnvironment.IsOnlinePlay);
        bool? established =
            onlineAccess == null ? null : Read(() => onlineAccess.IsOnlineSessionEstablished);
        bool? authenticated =
            onlineAccess == null ? null : Read(() => onlineAccess.IsAuthenticated);
        var state = Read(() => ClientStateManager.CurrentClientAppStateType);
        bool? inGame = state.HasValue ? state.Value == ClientAppStateType.InGame : null;
        bool? networkReady =
            offlineNetwork == null
                ? null
                : Read(() =>
                    offlineNetworkInitialized
                    && offlineNetwork.initialized
                    && !offlineNetwork.disposed
                    && offlineNetwork.NetworkService != null
                );

        long pointer = 0;
        string id = null;
        bool actorMatches = false;
        bool? characterOffline = null;
        if (inGame == true)
        {
            try
            {
                // Read current game references, not Refs_Manager's cached player.
                var tracker = PlayerFinder.getPlayerDataTracker();
                var actor = PlayerFinder.getPlayerActor();
                if (!tracker.IsNullOrDestroyed())
                {
                    var trackerActor = tracker.actor;
                    actorMatches =
                        !actor.IsNullOrDestroyed()
                        && !trackerActor.IsNullOrDestroyed()
                        && actor.Pointer == trackerActor.Pointer;
                    var data = tracker.charData;
                    if (data != null)
                    {
                        pointer = data.Pointer.ToInt64();
                        id = data.Id;
                        characterOffline = Read(() => data.IsOffline);
                    }
                }
            }
            catch
            {
                // Loading/unloading references are unavailable evidence, never a positive signal.
                pointer = 0;
                id = null;
                actorMatches = false;
                characterOffline = null;
            }
        }

        observation.Observe(
            new OfflineGuardEvidence(
                online,
                established,
                inGame,
                networkReady,
                actorMatches,
                pointer,
                id,
                characterOffline
            )
        );
        string snapshot =
            "epoch="
            + observation.Epoch
            + " state="
            + observation.State
            + " reason=\""
            + observation.Reason
            + "\""
            + " app="
            + (state.HasValue ? state.Value.ToString() : "unknown")
            + " onlinePlay="
            + Flag(online)
            + " onlineSession="
            + Flag(established)
            + " authenticated="
            + Flag(authenticated)
            + " offlineNetwork="
            + Flag(networkReady)
            + " localActor="
            + actorMatches
            + " requestMatch="
            + observation.MatchesRequestedCharacter(pointer, id)
            + " characterOffline="
            + Flag(characterOffline);
        if (snapshot != lastSnapshot)
        {
            Log(snapshot);
            lastSnapshot = snapshot;
        }
        if (now >= nextHeartbeat)
        {
            nextHeartbeat = now + 30f;
            Log(
                "Heartbeat epoch="
                    + observation.Epoch
                    + " state="
                    + observation.State
                    + " offlineSaveRequests="
                    + Volatile.Read(ref offlineSaveRequests)
                    + " saveOfflineDataRequests="
                    + Volatile.Read(ref saveOfflineDataRequests)
                    + " observerErrors="
                    + probeErrors
                    + " droppedMessages="
                    + droppedMessages
                    + "; save counters indicate requests, not async completion."
            );
        }
    }

    [HarmonyPatch(typeof(OfflineCharacterService), "StartPlay")]
    public class OfflineStartPlay
    {
        [HarmonyPrefix]
        static void Prefix(OfflineCharacterService __instance, CharacterData __0) =>
            Safe(() =>
            {
                observation.BeginOffline(
                    __0 == null ? 0 : __0.Pointer.ToInt64(),
                    __0?.Id,
                    __instance._dataStore != null
                );
                Log(
                    "OfflineCharacterService.StartPlay requested; epoch="
                        + observation.Epoch
                        + "; async completion not assumed."
                );
            });
    }

    [HarmonyPatch(typeof(CosmosClientCharacterService), "StartPlay")]
    public class OnlineStartPlay
    {
        [HarmonyPrefix]
        static void Prefix() =>
            Safe(() =>
            {
                observation.Revoke("Online character StartPlay requested");
                Log("Online character StartPlay requested; diagnostic epoch revoked.");
            });
    }

    [HarmonyPatch(typeof(CharacterDataTracker), "InitializeCharacter")]
    public class CharacterInitialized
    {
        [HarmonyPostfix]
        static void Postfix(CharacterDataTracker __instance) =>
            Safe(() =>
            {
                var data = __instance.charData;
                long pointer = data == null ? 0 : data.Pointer.ToInt64();
                bool matches = observation.MatchesRequestedCharacter(pointer, data?.Id);
                observation.CharacterInitialized(pointer, data?.Id);
                Log(
                    "CharacterDataTracker.InitializeCharacter returned; requestMatch="
                        + matches
                        + "."
                );
            });
    }

    [HarmonyPatch(typeof(NetworkServiceGroup), "InitializeClientOffline")]
    public class OfflineNetworkInitialize
    {
        [HarmonyPrefix]
        static void Prefix(NetworkServiceGroup __instance) =>
            Safe(() =>
            {
                offlineNetwork = __instance;
                offlineNetworkInitialized = false;
                Log("InitializeClientOffline requested.");
            });

        [HarmonyPostfix]
        static void Postfix(NetworkServiceGroup __instance) =>
            Safe(() =>
            {
                if (offlineNetwork != null && offlineNetwork.Pointer == __instance.Pointer)
                    offlineNetworkInitialized = true;
                Log(
                    "InitializeClientOffline returned; readiness will be sampled from the live group."
                );
            });
    }

    [HarmonyPatch(typeof(NetworkServiceGroup), "InitializeClient")]
    public class OnlineNetworkInitialize
    {
        [HarmonyPrefix]
        static void Prefix() =>
            Safe(() =>
            {
                offlineNetworkInitialized = false;
                observation.Revoke("InitializeClient requested");
                Log("InitializeClient requested; diagnostic epoch revoked.");
            });
    }

    [HarmonyPatch(typeof(NetworkServiceGroup), "Shutdown")]
    public class NetworkShutdown
    {
        [HarmonyPrefix]
        static void Prefix(NetworkServiceGroup __instance) =>
            Safe(() =>
            {
                bool tracked =
                    offlineNetwork != null && offlineNetwork.Pointer == __instance.Pointer;
                if (tracked)
                {
                    offlineNetwork = null;
                    offlineNetworkInitialized = false;
                }
                Log("Network Shutdown requested; trackedOfflineGroup=" + tracked + ".");
            });
    }

    [HarmonyPatch(typeof(OnlineAccessService), "Initialize")]
    public class OnlineAccessInitialize
    {
        [HarmonyPostfix]
        static void Postfix(OnlineAccessService __instance) =>
            Safe(() =>
            {
                onlineAccess = __instance;
                Log(
                    "Online access service observed; authentication alone is not an online-play veto."
                );
            });
    }

    [HarmonyPatch(typeof(OnlineAccessService), "EstablishOnlineSessionAsync")]
    public class OnlineSessionRequested
    {
        [HarmonyPrefix]
        static void Prefix(OnlineAccessService __instance) =>
            Safe(() =>
            {
                onlineAccess = __instance;
                observation.Revoke("Online session establishment requested");
                Log("Online session establishment requested; diagnostic epoch revoked.");
            });
    }

    [HarmonyPatch(typeof(GameplayEnvironment), "set_IsOnlinePlay")]
    public class GameplayModeChanged
    {
        [HarmonyPrefix]
        static void Prefix(bool __0) =>
            Safe(() =>
            {
                if (__0)
                    observation.Revoke("Online gameplay mode requested");
                Log("Gameplay mode write requested: online=" + __0 + ".");
            });
    }

    [HarmonyPatch(typeof(ClientStateManager), "Transition")]
    public class ClientTransition
    {
        [HarmonyPrefix]
        static void Prefix(ClientAppStateType __0) =>
            Safe(() =>
            {
                if (__0 == ClientAppStateType.Login || __0 == ClientAppStateType.CharacterSelect)
                    observation.EndSession("Client transition requested to " + __0);
                Log("Client transition requested to " + __0 + "; current state will be sampled.");
            });
    }

    [HarmonyPatch(typeof(InGameAppState), "Exit")]
    public class InGameExit
    {
        [HarmonyPrefix]
        static void Prefix() =>
            Safe(() =>
            {
                observation.EndSession("InGame exit requested");
                Log("InGame exit requested; character correlation cleared.");
            });
    }

    [HarmonyPatch(typeof(OfflineCharacterService), "SaveCharacter")]
    public class OfflineSaveRequested
    {
        [HarmonyPrefix]
        static void Prefix() => Interlocked.Increment(ref offlineSaveRequests);
    }

    [HarmonyPatch(typeof(InGameAppState), "SaveOfflineData")]
    public class SaveOfflineDataRequested
    {
        [HarmonyPrefix]
        static void Prefix() => Interlocked.Increment(ref saveOfflineDataRequests);
    }
}
