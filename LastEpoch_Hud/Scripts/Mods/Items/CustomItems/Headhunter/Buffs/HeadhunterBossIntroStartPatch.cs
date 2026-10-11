using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;
using LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Kills;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

[HarmonyPatch(typeof(Emerging), "onEnter")]
public class HeadhunterBossIntroStartPatch
{
    // Diagnostic branch only: leave the native emergence method unpatched.
    [HarmonyPrepare]
    private static bool Prepare(System.Reflection.MethodBase original)
    {
        if (original == null)
        {
            Main.logger_instance?.Msg("[Yrun Isolation] Disabled HeadhunterBossIntroStartPatch.");
        }
        return false;
    }

    [HarmonyPostfix]
    private static void Postfix(Emerging __instance)
    {
        if (!HeadhunterBossIntroWatch.IsLong(__instance.duration))
        {
            return;
        }

        try
        {
            Actor actor = __instance.getActor();
            if (!HeadhunterBossKind.TryGet(actor, out _))
            {
                return;
            }

            HeadhunterTimerPause.OnBossIntroStart(
                __instance.Pointer.ToInt64(),
                actor.name,
                __instance.duration,
                Time.unscaledTime
            );
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "HH boss intro start");
        }
    }
}
