using System;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LastEpoch_Hud.Scripts.Mods.Items;

// Test-branch ground-hover guard and bounded diagnostics. Preserve native closes
// for clicks, cancellation, menus, item removal and tooltip replacement.
internal static class GroundTooltipDiagnostics
{
    [ThreadStatic]
    static int openingDepth;
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

    static bool PointerOnOpener(TooltipItemManager manager)
    {
        var parameters = manager.activeParameters;
        var opener = parameters.Opener;
        var highlighted = TooltipItem.highlightedTooltipItem;
        if (
            opener.IsNullOrDestroyed()
            || !opener.activeInHierarchy
            || highlighted.IsNullOrDestroyed()
            || !highlighted.isActiveAndEnabled
            || highlighted.slotType != TooltipItemManager.SlotType.GROUND
            || highlighted.button.IsNullOrDestroyed()
            || !highlighted.button.interactable
        )
            return false;
        var events = EventSystem.current;
        if (events.IsNullOrDestroyed())
            return false;
        var pointer = new PointerEventData(events) { position = Input.mousePosition };
        var hits = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
        events.RaycastAll(pointer, hits);
        if (hits.Count == 0 || hits[0].gameObject.IsNullOrDestroyed())
            return false;
        var hit = hits[0].gameObject;
        return (hit == opener || hit.transform.IsChildOf(opener.transform))
            && (hit == highlighted.gameObject || hit.transform.IsChildOf(highlighted.transform));
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
        [HarmonyPrefix]
        static void Prefix(out bool __state)
        {
            openingDepth++;
            __state = true;
        }

        [HarmonyFinalizer]
        static Exception Finalizer(
            TooltipItemManager __instance,
            TooltipItemManager.SlotType __1,
            bool __state,
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
            if (__state)
                openingDepth--;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TooltipItemManager), nameof(TooltipItemManager.CloseTooltip))]
    static class Close
    {
        [HarmonyPrefix]
        static bool Prefix(TooltipItemManager __instance)
        {
            try
            {
                bool ground =
                    __instance.active
                    && !__instance.activeParameters.IsNullOrDestroyed()
                    && __instance.activeParameters.SlotType == TooltipItemManager.SlotType.GROUND;
                if (!ground)
                    return true;
                bool pointer = PointerOnOpener(__instance);
                bool mouseHeld = Input.GetMouseButton(0) || Input.GetMouseButton(1);
                bool blocked = UIMouseListener.WorldActionsDisabled();
                bool keep = GroundTooltipRules.KeepHoveredGroundTooltip(
                    Scenes.IsGameScene() && Application.isFocused && __instance.isActiveAndEnabled,
                    ground,
                    !__instance.activeParameters.Item.IsNullOrDestroyed(),
                    pointer,
                    openingDepth > 0,
                    mouseHeld,
                    Input.GetKey(KeyCode.Escape),
                    blocked
                );
                Trace(
                    "close "
                        + (keep ? "kept" : "allowed")
                        + ", stage="
                        + __instance.activeInitializationStage
                        + ", pointerOnOpener="
                        + pointer
                        + ", mouseHeld="
                        + mouseHeld
                        + ", worldBlocked="
                        + blocked
                        + ", caller="
                        + Environment.StackTrace
                );
                return !keep;
            }
            catch
            {
                return true;
            }
        }
    }
}
