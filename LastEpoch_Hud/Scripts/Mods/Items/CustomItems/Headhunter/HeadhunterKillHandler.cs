using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.ModUI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Handles one Headhunter kill: active buffs, mechanic, sink, debug line.</summary>
internal static class HeadhunterKillHandler
{
    private static readonly HashSet<int> _active = new();
    private static readonly Func<int, string> _statName = id => ((SP)id).ToString();

    public static void Handle(KillInfo kill)
    {
        IHeadhunterMechanic mechanic = HeadhunterConfigLoader.Mechanic;
        StatBuffs buffs = HeadhunterBuffSink.PlayerBuffs();
        if (mechanic == null || buffs == null)
        {
            return;
        }

        HeadhunterBuffSink.FillActive(buffs, HeadhunterConfigLoader.Resolved.Stats, _active);
        IReadOnlyList<BuffAction> actions = mechanic.OnKill(kill, _active);
        HeadhunterBuffSink.Apply(buffs, actions);
        HeadhunterBuffBar.MarkDirty();
        LogKill(kill, actions);
    }

    private static void LogKill(KillInfo kill, IReadOnlyList<BuffAction> actions)
    {
        if (!ModSettings.Debug.Enabled.Value)
        {
            return;
        }

        Main.logger_instance?.Msg(HeadhunterKillLog.Format(kill, actions, _statName));
    }
}
