using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Scales the player model by distinct buff count and restores it.</summary>
internal static class HeadhunterModelScaler
{
    private static readonly HeadhunterSizeTracker _tracker = new();
    private static ActorVisuals _visuals;
    private static Transform _model;
    private static Transform _applied;
    private static Vector3 _original;
    private static Vector3 _written;

    public static void Apply(int liveBuffs)
    {
        float factor = HeadhunterConfigLoader.Current.ModelSize.Factor(liveBuffs);
        Transform model = CurrentModel();
        HeadhunterSizeAction action = _tracker.Next(IdOf(model), factor, ScaleIntact());
        if (action == HeadhunterSizeAction.None)
        {
            return;
        }

        if (action == HeadhunterSizeAction.RestoreThenRescale)
        {
            RestoreApplied();
        }

        _applied = null;
        if (model.IsNullOrDestroyed() || factor == HeadhunterSizeCurve.NormalFactor)
        {
            return;
        }

        Scale(model, factor);
        Log(liveBuffs, factor);
    }

    public static void Restore()
    {
        try
        {
            if (_tracker.ShouldRestore(ScaleIntact()))
            {
                RestoreApplied();
            }
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter model restore");
        }

        _tracker.Reset();
        _applied = null;
    }

    private static Transform CurrentModel()
    {
        ActorVisuals visuals = Refs_Manager.player_visuals;
        if (visuals.IsNullOrDestroyed())
        {
            return null;
        }
        if (visuals == _visuals && !_model.IsNullOrDestroyed())
        {
            return _model;
        }

        _visuals = visuals;
        _model = FindModel(visuals);
        return _model;
    }

    private static Transform FindModel(ActorVisuals visuals)
    {
        ActorModelIndicator indicator = visuals.visualFormChangerManager?.actorModel;
        if (indicator.IsNullOrDestroyed())
        {
            indicator = visuals.GetComponentInChildren<ActorModelIndicator>(true);
        }

        return indicator.IsNullOrDestroyed() ? null : indicator.transform;
    }

    private static bool ScaleIntact()
    {
        if (_applied is null)
        {
            return true;
        }

        return !_applied.IsNullOrDestroyed() && _applied.localScale == _written;
    }

    private static void Scale(Transform model, float factor)
    {
        _original = model.localScale;
        _written = _original * factor;
        model.localScale = _written;
        _applied = model;
    }

    private static void RestoreApplied()
    {
        if (_applied.IsNullOrDestroyed())
        {
            return;
        }

        _applied.localScale = _original;
    }

    private static int IdOf(Transform model)
    {
        return model.IsNullOrDestroyed() ? HeadhunterSizeTracker.NoModel : model.GetInstanceID();
    }

    private static void Log(int buffs, float factor)
    {
        if (!ModSettings.Debug.Enabled.Value)
        {
            return;
        }

        Main.logger_instance?.Msg("Headhunter size: " + buffs + " buff(s), x" + factor);
    }
}
