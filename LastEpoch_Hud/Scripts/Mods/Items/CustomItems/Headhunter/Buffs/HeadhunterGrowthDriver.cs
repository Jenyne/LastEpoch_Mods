using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.ModUI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Applies grown buff values on the bar poll and logs growth changes in Debug.</summary>
internal static class HeadhunterGrowthDriver
{
    public static void OnPoll(StatBuffs buffs, float[] remaining)
    {
        HeadhunterGrowthReapply reapply = HeadhunterConfigLoader.Reapply;
        if (reapply == null || HeadhunterConfigLoader.Growth == null)
        {
            return;
        }

        float before = HeadhunterConfigLoader.Growth.Factor;
        IReadOnlyList<BuffAction> actions = reapply.Check(remaining);
        HeadhunterBuffSink.Apply(buffs, actions);
        LogIfFactorChanged(before);
    }

    public static void LogIfFactorChanged(float factorBefore)
    {
        HeadhunterValueGrowth growth = HeadhunterConfigLoader.Growth;
        if (growth == null || !ModSettings.Debug.Enabled.Value)
        {
            return;
        }
        if (growth.Factor.Equals(factorBefore))
        {
            return;
        }

        Main.logger_instance?.Msg(HeadhunterGrowthLog.Format(growth.AppliedTotal, growth.Factor));
    }
}
