using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

[HarmonyPatch(typeof(Emerging), "onExit")]
public class HeadhunterBossIntroEndPatch
{
    // Diagnostic branch only: leave the native emergence method unpatched.
    [HarmonyPrepare]
    private static bool Prepare(System.Reflection.MethodBase original)
    {
        if (original == null)
        {
            Main.logger_instance?.Msg("[Yrun Isolation] Disabled HeadhunterBossIntroEndPatch.");
        }
        return false;
    }

    [HarmonyPrefix]
    private static void Prefix(Emerging __instance)
    {
        if (!HeadhunterTimerPause.HasBossIntro)
        {
            return;
        }

        try
        {
            HeadhunterTimerPause.OnBossIntroEnd(__instance.Pointer.ToInt64(), Time.unscaledTime);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "HH boss intro end");
        }
    }
}
