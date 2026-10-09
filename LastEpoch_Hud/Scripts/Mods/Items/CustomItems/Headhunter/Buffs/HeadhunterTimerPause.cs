using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;
using LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Bar;
using LastEpoch_Hud.Scripts.ModUI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Applies the zone, arrival and cinematic pause to the player's HH buffs.</summary>
internal static class HeadhunterTimerPause
{
    private static float[] _live = Array.Empty<float>();

    private static readonly HeadhunterZonePause _zone = new();

    public static HeadhunterTimerFreeze Freeze => _zone.Freeze;

    public static void OnSceneLoaded(string sceneName, double now)
    {
        bool nonCombat = IsNonCombat(sceneName);
        HeadhunterPauseChange change = _zone.OnScene(sceneName, nonCombat, now);
        if (ModSettings.Debug.Enabled.Value)
        {
            Main.logger_instance?.Msg(HeadhunterPauseLog.Zone(sceneName, nonCombat, change));
        }

        ApplyPending();
        HeadhunterBuffBar.MarkDirty();
    }

    /// <summary>Polls arrival and cinematic state in combat zones.</summary>
    public static void Tick(double now)
    {
        if (!_zone.IsPollDue(now))
        {
            return;
        }

        PollArrival(now);
        PollCinematic(now);
    }

    /// <summary>Drops the arrival watch and kept timers after HH buffs were removed.</summary>
    public static void Clear()
    {
        _zone.Clear();
    }

    public static void ApplyPending()
    {
        if (!Freeze.IsApplyPending)
        {
            return;
        }

        TryApply();
    }

    /// <summary>Resumes timers once arrival protection ends.</summary>
    private static void PollArrival(double now)
    {
        if (!_zone.IsWatchingArrival)
        {
            return;
        }

        HeadhunterArrivalState state = ReadArrivalState();
        if (!_zone.TryEndArrival(state, now, out double held))
        {
            return;
        }

        ApplyPending();
        HeadhunterBuffBar.MarkDirty();
        if (ModSettings.Debug.Enabled.Value)
        {
            Main.logger_instance?.Msg(HeadhunterPauseLog.Arrival(_zone.Scene, held, state));
        }
    }

    /// <summary>Freezes or resumes timers when a cinematic starts or ends.</summary>
    private static void PollCinematic(double now)
    {
        bool active = ReadCinematic();
        if (!_zone.TryCinematic(active, now, out double held))
        {
            return;
        }

        ApplyPending();
        HeadhunterBuffBar.MarkDirty();
        if (ModSettings.Debug.Enabled.Value)
        {
            Main.logger_instance?.Msg(HeadhunterPauseLog.Cinematic(_zone.Scene, active, held));
        }
    }

    /// <summary>Reads the game's cinematic flag; a missing input manager reads as no cinematic.</summary>
    private static bool ReadCinematic()
    {
        try
        {
            EpochInputManager input = Refs_Manager.epoch_input_manager;
            return !input.IsNullOrDestroyed() && input.cinematicActive;
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter cinematic check");
            return false;
        }
    }

    /// <summary>Reads the game's arrival protection.</summary>
    private static HeadhunterArrivalState ReadArrivalState()
    {
        try
        {
            PlayerZoneTransitionHandler handler = Refs_Manager.player_health.IsNullOrDestroyed()
                ? null
                : Refs_Manager.player_health.playerZoneTransition;
            if (handler.IsNullOrDestroyed())
            {
                return HeadhunterArrivalState.Missing;
            }

            return handler.damageable()
                ? HeadhunterArrivalState.Damageable
                : HeadhunterArrivalState.Protected;
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter arrival check");
            return HeadhunterArrivalState.Missing;
        }
    }

    private static bool IsNonCombat(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        try
        {
            return SceneList.IsNonCombatZone(sceneName);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter zone check");
            return false;
        }
    }

    private static void TryApply()
    {
        StatBuffs buffs = HeadhunterBuffSink.PlayerBuffs();
        HeadhunterResolvedConfig config = HeadhunterConfigLoader.Resolved;
        if (buffs == null || config == null)
        {
            return;
        }

        try
        {
            ApplyTo(buffs, config);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter timer pause");
        }
    }

    private static void ApplyTo(StatBuffs buffs, HeadhunterResolvedConfig config)
    {
        if (_live.Length != config.Stats.Count)
        {
            _live = new float[config.Stats.Count];
        }

        HeadhunterBuffSink.FillRemaining(buffs, config.Stats, _live);
        IReadOnlyList<BuffAction> actions = Freeze.Apply(config, _live);
        HeadhunterBuffSink.Apply(buffs, actions);
        HeadhunterBuffBar.MarkDirty();
        if (ModSettings.Debug.Enabled.Value)
        {
            Main.logger_instance?.Msg(HeadhunterPauseLog.Applied(Freeze.IsHolding, actions.Count));
        }
    }
}
