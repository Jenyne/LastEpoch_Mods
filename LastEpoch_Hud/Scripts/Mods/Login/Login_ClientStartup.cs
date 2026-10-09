using System;
using Il2CppInterop.Runtime;
using LastEpoch_Hud.Scripts.Core.Login;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Login;

internal static class Login_ClientStartup
{
    static readonly ClientStartupReadiness readiness = new();
    static Application.LogCallback callback;
    public static bool Ready => callback != null && readiness.ReadyForOfflineClick;
    public static string State => readiness.State ?? "not confirmed";

    public static void Initialize()
    {
        if (callback != null)
            return;
        readiness.Reset();
        try
        {
            callback = DelegateSupport.ConvertDelegate<Application.LogCallback>(
                (Action<string, string, LogType>)HandleLog
            );
            if (callback == null)
                throw new InvalidOperationException("Unity log callback could not be created.");
            Application.add_logMessageReceived(callback);
        }
        catch (Exception ex)
        {
            callback = null;
            Main.logger_instance?.Warning(
                "[Offline] Startup completion listener unavailable: " + ex.Message
            );
        }
    }

    public static void Tick()
    {
        if (
            callback != null
            && (readiness.State == "CharacterSelect" || readiness.State == "InGame")
        )
            Stop();
    }

    static void HandleLog(string message, string stackTrace, LogType type)
    {
        // Main-thread log callback: no native client/session/actor inspection,
        // task polling, scene changes or async work from this notification.
        if (callback != null && readiness.Observe(message) && readiness.ReadyForOfflineClick)
            Main.logger_instance?.Msg(
                "[Offline] Client startup reached Login; waiting for landing readiness."
            );
    }

    public static void Stop()
    {
        var previous = callback;
        callback = null;
        readiness.Reset();
        if (previous == null)
            return;
        try
        {
            Application.remove_logMessageReceived(previous);
        }
        catch (Exception ex)
        {
            Main.logger_instance?.Warning(
                "[Offline] Startup listener cleanup failed: " + ex.Message
            );
        }
    }
}
