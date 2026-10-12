using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items;

// Test-branch diagnostics only: observe the native ground-tooltip path without
// forcing tooltips open or swallowing exceptions. Bounded per scene.
internal static class GroundTooltipDiagnostics
{
    static string scene;
    static int remaining;

    static void Trace(string message)
    {
        if (scene != Scenes.SceneName)
        {
            scene = Scenes.SceneName;
            remaining = 60;
        }
        if (remaining > 0)
        {
            remaining--;
            Main.logger_instance?.Msg("[Ground Tooltip] frame=" + Time.frameCount + " " + message);
        }
    }

    [HarmonyPatch(typeof(TooltipItem), nameof(TooltipItem.OnPointerEnter))]
    static class Hover
    {
        [HarmonyFinalizer]
        static Exception Finalizer(TooltipItem __instance, Exception __exception)
        {
            try
            {
                if (
                    !__instance.IsNullOrDestroyed()
                    && __instance.slotType == TooltipItemManager.SlotType.GROUND
                )
                    Trace(
                        "hover="
                            + __instance.name
                            + ", button="
                            + (
                                __instance.button.IsNullOrDestroyed()
                                    ? "missing"
                                    : __instance.button.interactable.ToString()
                            )
                            + ", highlighted="
                            + (TooltipItem.highlightedTooltipItem == __instance)
                            + ", worldBlocked="
                            + UIMouseListener.WorldActionsDisabled()
                            + ", error="
                            + (__exception?.ToString() ?? "none")
                    );
            }
            catch { }
            return __exception;
        }
    }

    [HarmonyPatch(
        typeof(TooltipItemManager),
        nameof(TooltipItemManager.OpenTooltip),
        new Type[]
        {
            typeof(ItemDataUnpacked),
            typeof(TooltipItemManager.SlotType),
            typeof(Vector2),
            typeof(Vector3),
            typeof(GameObject),
            typeof(Vector2),
        }
    )]
    static class Open
    {
        [HarmonyFinalizer]
        static Exception Finalizer(
            TooltipItemManager __instance,
            TooltipItemManager.SlotType __1,
            Exception __exception
        )
        {
            try
            {
                if (__1 == TooltipItemManager.SlotType.GROUND)
                    Trace(
                        "open active="
                            + __instance.active
                            + ", stage="
                            + __instance.activeInitializationStage
                            + ", ready="
                            + __instance.IsTooltipReady
                            + ", error="
                            + (__exception?.ToString() ?? "none")
                    );
            }
            catch { }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.CloseTooltip))]
    static class Close
    {
        [HarmonyPrefix]
        static void Prefix(TooltipItemManager __instance)
        {
            try
            {
                if (
                    __instance.active
                    && !__instance.activeParameters.IsNullOrDestroyed()
                    && __instance.activeParameters.SlotType == TooltipItemManager.SlotType.GROUND
                )
                    Trace(
                        "close active="
                            + __instance.active
                            + ", stage="
                            + __instance.activeInitializationStage
                            + ", hovered="
                            + (
                                !TooltipItem.highlightedTooltipItem.IsNullOrDestroyed()
                                && TooltipItem.highlightedTooltipItem.slotType
                                    == TooltipItemManager.SlotType.GROUND
                            )
                            + ", leftHeld="
                            + Input.GetMouseButton(0)
                            + ", rightHeld="
                            + Input.GetMouseButton(1)
                            + ", worldBlocked="
                            + UIMouseListener.WorldActionsDisabled()
                            + ", caller="
                            + Environment.StackTrace
                    );
            }
            catch { }
        }
    }
}
