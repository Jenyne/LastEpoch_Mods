using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.UI.Tooltips;
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

    [ThreadStatic]
    static string closeSource;

    [ThreadStatic]
    static CloseAllScope closeAllScope;

    sealed class CloseAllScope
    {
        internal CloseAllScope Parent;
        internal UITooltipItem ProtectedItem;
    }

    static string scene;
    static int remaining;

    static void Trace(string message)
    {
        if (scene != Scenes.SceneName)
        {
            scene = Scenes.SceneName;
            remaining = 120;
        }
        if (remaining > 0)
        {
            remaining--;
            Main.logger_instance?.Msg("[Ground Tooltip] frame=" + Time.frameCount + " " + message);
        }
    }

    static string ObjectPath(GameObject obj)
    {
        if (obj.IsNullOrDestroyed())
            return "missing";
        string path = obj.name + "#" + obj.GetInstanceID();
        var parent = obj.transform.parent;
        for (int depth = 0; depth < 4 && !parent.IsNullOrDestroyed(); depth++)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    static bool PointerOnOpener(TooltipItemManager manager, out string detail)
    {
        var opener = manager.activeParameters.Opener;
        var highlighted = TooltipItem.highlightedTooltipItem;
        detail =
            "opener="
            + ObjectPath(opener)
            + ", highlighted="
            + (highlighted.IsNullOrDestroyed() ? "missing" : ObjectPath(highlighted.gameObject));
        if (opener.IsNullOrDestroyed() || !opener.activeInHierarchy)
        {
            detail += ", reason=opener inactive";
            return false;
        }
        if (
            highlighted.IsNullOrDestroyed()
            || !highlighted.isActiveAndEnabled
            || highlighted.slotType != TooltipItemManager.SlotType.GROUND
        )
        {
            detail += ", reason=no active ground highlight";
            return false;
        }
        if (highlighted.button.IsNullOrDestroyed() || !highlighted.button.interactable)
        {
            detail += ", reason=label button unavailable";
            return false;
        }
        var events = EventSystem.current;
        if (events.IsNullOrDestroyed())
        {
            detail += ", reason=no event system";
            return false;
        }
        var pointer = new PointerEventData(events) { position = Input.mousePosition };
        var hits = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
        events.RaycastAll(pointer, hits);
        detail +=
            ", mouse="
            + pointer.position
            + ", labelPosition="
            + opener.transform.position
            + ", hits="
            + hits.Count;
        for (int i = 0; i < Math.Min(3, hits.Count); i++)
            detail += ", hit" + i + "=" + ObjectPath(hits[i].gameObject);
        if (hits.Count == 0 || hits[0].gameObject.IsNullOrDestroyed())
        {
            detail += ", reason=no top hit";
            return false;
        }
        var hit = hits[0].gameObject;
        bool onOpener = hit == opener || hit.transform.IsChildOf(opener.transform);
        bool onHighlight =
            hit == highlighted.gameObject || hit.transform.IsChildOf(highlighted.transform);
        detail += ", onOpener=" + onOpener + ", onHighlight=" + onHighlight;
        return onOpener && onHighlight;
    }

    // The native implementation closes all tooltips before clearing the highlight.
    // Record that source without changing pointer-exit behavior.
    [HarmonyPatch(typeof(TooltipItem), nameof(TooltipItem.OnPointerExit))]
    static class Exit
    {
        [HarmonyPrefix]
        static void Prefix(TooltipItem __instance, PointerEventData __0, out string __state)
        {
            __state = closeSource;
            if (
                __instance.IsNullOrDestroyed()
                || __instance.slotType != TooltipItemManager.SlotType.GROUND
            )
                return;
            closeSource = "ground pointer exit";
            try
            {
                Trace(
                    "exit label="
                        + ObjectPath(__instance.gameObject)
                        + ", eventHit="
                        + (
                            __0.IsNullOrDestroyed()
                                ? "missing event"
                                : ObjectPath(__0.pointerCurrentRaycast.gameObject)
                        )
                        + ", highlighted="
                        + (TooltipItem.highlightedTooltipItem == __instance)
                );
            }
            catch { }
        }

        [HarmonyFinalizer]
        static Exception Finalizer(string __state, Exception __exception)
        {
            closeSource = __state;
            return __exception;
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

    // CloseAllTooltips independently clears the legacy view after closing its manager.
    // Protect only that exact view when the manager rejected this same close operation.
    [HarmonyPatch(typeof(TooltipSystem), nameof(TooltipSystem.CloseAllTooltips))]
    static class CloseAll
    {
        [HarmonyPrefix]
        static void Prefix(out CloseAllScope __state)
        {
            __state = new CloseAllScope { Parent = closeAllScope };
            closeAllScope = __state;
        }

        [HarmonyFinalizer]
        static Exception Finalizer(CloseAllScope __state, Exception __exception)
        {
            closeAllScope = __state.Parent;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(UITooltipItem), nameof(UITooltipItem.CloseTooltip))]
    static class CloseVisibleItem
    {
        [HarmonyPrefix]
        static bool Prefix(UITooltipItem __instance)
        {
            try
            {
                var protectedItem = closeAllScope?.ProtectedItem;
                if (protectedItem.IsNullOrDestroyed() || protectedItem != __instance)
                    return true;
                Trace(
                    "visible close kept after rejected manager close, view="
                        + ObjectPath(__instance.gameObject)
                );
                return false;
            }
            catch
            {
                return true;
            }
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
                bool pointer = PointerOnOpener(__instance, out string pointerDetail);
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
                if (closeAllScope != null)
                    closeAllScope.ProtectedItem = keep ? __instance.itemTooltip : null;
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
                        + ", source="
                        + (closeSource ?? "other native close")
                        + ", "
                        + pointerDetail
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
