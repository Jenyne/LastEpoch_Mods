using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Probe;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Probe;

[HarmonyPatch(typeof(Emerging), "onExit")]
public class HeadhunterProbeEmergeEndPatch
{
    [HarmonyPrefix]
    private static void Prefix(Emerging __instance)
    {
        if (!HeadhunterProbe.IsOn)
        {
            return;
        }

        try
        {
            Actor actor = __instance.getActor();
            if (!HeadhunterProbe.TryBossKind(actor, out KillKind kind))
            {
                return;
            }

            HeadhunterProbe.Write(
                HeadhunterProbeLog.EmergeEnd(
                    HeadhunterProbe.Scene,
                    actor.name,
                    kind,
                    __instance.timeSpentEmerging,
                    __instance.duration,
                    __instance.SkippedEmerging
                )
            );
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "HH probe emerge end");
        }
    }
}
