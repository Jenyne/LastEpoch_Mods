using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Debug-only one-shot trial of candidate player glows; research spike, removed with the real aura.</summary>
internal static class HeadhunterAuraProbe
{
    private const double ShowSeconds = 4;
    private const double GapSeconds = 2;
    private const float WeakTint = 0.3f;
    private const float StrongTint = 1.5f;
    private const int CandidateCount = 6;

    private static int _next;
    private static HeadhunterAuraCandidate? _active;
    private static RendererManager _manager;
    private static HeadhunterRendererSnapshot _snapshot;
    private static double _until;

    public static void Tick(double now, int liveBuffs)
    {
        if (!ModSettings.Debug.Enabled.Value)
        {
            Revert();
            return;
        }

        if (_next >= CandidateCount)
        {
            return;
        }

        if (_active.HasValue && (liveBuffs == 0 || now >= _until))
        {
            Revert();
            _until = now + GapSeconds;
            return;
        }

        if (liveBuffs == 0 || now < _until)
        {
            return;
        }

        TryApply(now);
    }

    public static void Revert()
    {
        if (!_active.HasValue)
        {
            return;
        }

        HeadhunterAuraCandidate candidate = _active.Value;
        RevertCandidate(candidate, _manager, _snapshot);
        Log(candidate + " reverted");

        _active = null;
        _manager = null;
        _snapshot = null;
        Advance();
    }

    private static void TryApply(double now)
    {
        RendererManager manager = Refs_Manager.player_visuals?.rendererManager;
        if (manager.IsNullOrDestroyed())
        {
            return;
        }

        if (_next == 0)
        {
            Log("start");
        }

        var candidate = (HeadhunterAuraCandidate)_next;
        var snapshot = HeadhunterRendererSnapshot.Capture(manager);
        try
        {
            Apply(candidate, manager);
        }
        catch (Exception ex)
        {
            Log(candidate + " failed: " + ex.Message);
            ErrorLog.Report(ex, "Headhunter aura probe");
            RevertCandidate(candidate, manager, snapshot);
            Advance();
            return;
        }

        _active = candidate;
        _manager = manager;
        _snapshot = snapshot;
        _until = now + ShowSeconds;
        Log(candidate + " applied renderers=" + snapshot.Count + MaterialInfo(snapshot));
    }

    private static string MaterialInfo(HeadhunterRendererSnapshot snapshot)
    {
        Material material = snapshot.FirstMaterial();
        if (material.IsNullOrDestroyed())
        {
            return " monsterFx=none shader=none";
        }

        Shader shader = material.shader;
        string shaderName = shader.IsNullOrDestroyed() ? "none" : shader.name;
        return " monsterFx="
            + material.HasProperty(RendererManager.EffectMonsterEffects)
            + " shader="
            + shaderName;
    }

    private static void RevertCandidate(
        HeadhunterAuraCandidate candidate,
        RendererManager manager,
        HeadhunterRendererSnapshot snapshot
    )
    {
        try
        {
            Undo(candidate, manager);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter aura probe undo");
        }

        try
        {
            snapshot.Restore();
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter aura probe restore");
        }
    }

    private static void Advance()
    {
        _next++;
        if (_next >= CandidateCount)
        {
            Log("done");
        }
    }

    private static void Apply(HeadhunterAuraCandidate candidate, RendererManager manager)
    {
        switch (candidate)
        {
            case HeadhunterAuraCandidate.RareGlow:
                manager.ApplyMonsterRarityGlow(Actor.Rarity.Rare);
                break;
            case HeadhunterAuraCandidate.RarityBlockWeak:
                manager.ApplyMonsterRarityPropertyBlock(RaritySettings(WeakTint));
                break;
            case HeadhunterAuraCandidate.RarityBlockStrong:
                manager.ApplyMonsterRarityPropertyBlock(RaritySettings(StrongTint));
                break;
            case HeadhunterAuraCandidate.MonsterGlow:
                manager.ApplyMonsterGlow(RendererManager.GlowType.WindowFireEmpowered);
                break;
            case HeadhunterAuraCandidate.Outline:
                Refs_Manager.player_visuals.actorOutlineVFX.enableOutlining();
                break;
            case HeadhunterAuraCandidate.MaterialOverride:
                manager.ApplyMaterialOverride(
                    RendererManager.MaterialOverrideType.RageOfMorditasGauntlet
                );
                break;
        }
    }

    private static void Undo(HeadhunterAuraCandidate candidate, RendererManager manager)
    {
        if (manager.IsNullOrDestroyed())
        {
            return;
        }

        switch (candidate)
        {
            case HeadhunterAuraCandidate.RareGlow:
                manager.ApplyMonsterRarityGlow(Actor.Rarity.Normal);
                break;
            case HeadhunterAuraCandidate.MonsterGlow:
                manager.ApplyMonsterGlow(RendererManager.GlowType.None);
                break;
            case HeadhunterAuraCandidate.Outline:
                Refs_Manager.player_visuals.actorOutlineVFX.disableOutlining();
                break;
            case HeadhunterAuraCandidate.MaterialOverride:
                manager.ApplyMaterialOverride(RendererManager.MaterialOverrideType.None);
                break;
        }
    }

    private static MonsterRarityColorSettings RaritySettings(float tint)
    {
        MonsterRarityColorSettings rare = MaterialList.getRareMonsterRaritySettings();
        Log(
            "rare settings color="
                + rare.color
                + " bias="
                + rare.bias
                + " power="
                + rare.power
                + " normalInfluence="
                + rare.normalInfluence
        );
        return new MonsterRarityColorSettings
        {
            color = rare.color * tint,
            bias = rare.bias,
            power = rare.power,
            normalInfluence = rare.normalInfluence,
        };
    }

    private static void Log(string message)
    {
        if (!ModSettings.Debug.Enabled.Value)
        {
            return;
        }

        Main.logger_instance?.Msg("HH aura probe: " + message);
    }
}
