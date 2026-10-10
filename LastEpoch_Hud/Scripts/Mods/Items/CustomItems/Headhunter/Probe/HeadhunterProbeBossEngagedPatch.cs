using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Probe;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Probe;

[HarmonyPatch(typeof(BossfightRespawnManager), "BossEngaged")]
public class HeadhunterProbeBossEngagedPatch
{
    [HarmonyPostfix]
    private static void Postfix(Actor actor, bool calledToArms, bool diedBeforeActivating)
    {
        if (!HeadhunterProbe.IsOn)
        {
            return;
        }

        try
        {
            string name = actor.IsNullOrDestroyed() ? null : actor.name;
            HeadhunterProbe.Write(
                HeadhunterProbeLog.BossEngaged(
                    HeadhunterProbe.Scene,
                    name,
                    calledToArms,
                    diedBeforeActivating
                )
            );
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "HH probe boss engaged");
        }
    }
}
