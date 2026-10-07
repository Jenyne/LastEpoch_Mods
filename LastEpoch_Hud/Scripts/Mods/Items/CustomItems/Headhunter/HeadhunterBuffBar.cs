using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Polls the live Headhunter buffs and feeds the bar view.</summary>
internal static class HeadhunterBuffBar
{
    private static readonly RefreshGate _gate = new(1.0);
    private static readonly HeadhunterBuffBarModel _model = new();
    private static float[] _remaining = Array.Empty<float>();

    public static void MarkDirty()
    {
        _gate.MarkDirty();
    }

    public static void Tick(double now)
    {
        if (!_gate.ShouldRefresh(now))
        {
            return;
        }

        try
        {
            Refresh();
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter buff bar");
        }
    }

    private static void Refresh()
    {
        StatBuffs buffs = HeadhunterBuffSink.PlayerBuffs();
        if (!ShouldShow(buffs))
        {
            HeadhunterBuffBarView.Hide();
            return;
        }

        HeadhunterResolvedConfig config = HeadhunterConfigLoader.Resolved;
        float[] remaining = RemainingFor(config.Stats.Count);
        HeadhunterBuffSink.FillRemaining(buffs, config.Stats, remaining);
        HeadhunterBuffBarView.Show(_model.Build(config.Stats, remaining, config.DurationSeconds));
    }

    private static bool ShouldShow(StatBuffs buffs)
    {
        if (!Scenes.IsGameScene() || buffs == null)
        {
            return false;
        }

        if (HeadhunterConfigLoader.Resolved == null)
        {
            return false;
        }

        return HeadhunterKillSource.IsHeadhunterWorn();
    }

    private static float[] RemainingFor(int count)
    {
        if (_remaining.Length != count)
        {
            _remaining = new float[count];
        }

        return _remaining;
    }
}
