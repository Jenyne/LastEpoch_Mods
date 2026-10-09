using System;
using System.Collections.Generic;
using System.Text;
using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LastEpoch_Hud.Scripts.Mods.UI;

// Manual evidence collection only. No changes to raycast, hover, or world-action state.
internal static class GroundHoverDiagnostics
{
    private static float nextAllowed;

    public static void Tick()
    {
        if (
            !Scenes.IsGameScene()
            || !Input.GetKeyDown(KeyCode.F9)
            || Time.realtimeSinceStartup < nextAllowed
        )
            return;
        nextAllowed = Time.realtimeSinceStartup + 2f;
        try
        {
            var events = EventSystem.current;
            if (events.IsNullOrDestroyed())
            {
                Main.logger_instance?.Msg("[HoverTrace] No EventSystem.");
                return;
            }
            var pointer = new PointerEventData(events) { position = Input.mousePosition };
            var hits = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
            events.RaycastAll(pointer, hits);
            bool meter =
                !DamageMeter.DamageMeter_obj.IsNullOrDestroyed()
                && DamageMeter.DamageMeter_obj.activeInHierarchy;
            Main.logger_instance?.Msg(
                "[HoverTrace] F9 snapshot: UI-over-pointer="
                    + events.IsPointerOverGameObject()
                    + "; raycastHits="
                    + hits.Count
                    + "; meterVisible="
                    + meter
                    + "; hudVisible="
                    + (
                        !Hud_Manager.hud_object.IsNullOrDestroyed()
                        && Hud_Manager.hud_object.activeInHierarchy
                    )
                    + "; selected="
                    + (
                        events.currentSelectedGameObject.IsNullOrDestroyed()
                            ? "none"
                            : Path(events.currentSelectedGameObject.transform)
                    )
                    + "; mouseButtons="
                    + Input.GetMouseButton(0)
                    + "/"
                    + Input.GetMouseButton(1)
            );
            for (int i = 0; i < Math.Min(5, hits.Count); i++)
            {
                var hit = hits[i].gameObject;
                if (hit.IsNullOrDestroyed())
                    continue;
                var listener = hit.GetComponentInParent<UIMouseListener>();
                Main.logger_instance?.Msg(
                    "[HoverTrace] hit["
                        + i
                        + "]="
                        + Path(hit.transform)
                        + "; raycaster="
                        + (
                            hits[i].module.IsNullOrDestroyed()
                                ? "none"
                                : hits[i].module.GetType().Name
                        )
                        + "; listener="
                        + (listener.IsNullOrDestroyed() ? "none" : Path(listener.transform))
                        + "; allowWorldActions="
                        + (
                            listener.IsNullOrDestroyed()
                                ? "n/a"
                                : listener.allowWorldActions.ToString()
                        )
                );
            }
            int active = 0,
                blockers = 0;
            var ownedListeners = new List<UIMouseListener>();
            var blockingListeners = new List<UIMouseListener>();
            // Include hidden owned listeners; native buff/loot listeners must not
            // exhaust the sample before actual blockers or mod panels are examined.
            foreach (var listener in UnityEngine.Object.FindObjectsOfType<UIMouseListener>(true))
            {
                if (listener.IsNullOrDestroyed())
                    continue;
                bool enabled = listener.isActiveAndEnabled;
                bool blocks = enabled && !listener.allowWorldActions;
                if (enabled)
                    active++;
                if (blocks)
                    blockers++;
                bool owned =
                    Under(listener.transform, Hud_Manager.hud_object)
                    || Under(listener.transform, DamageMeter.DamageMeter_obj);
                if (owned)
                    ownedListeners.Add(listener);
                else if (blocks)
                    blockingListeners.Add(listener);
            }
            ownedListeners.AddRange(blockingListeners);
            for (int i = 0; i < Math.Min(12, ownedListeners.Count); i++)
            {
                var listener = ownedListeners[i];
                bool owned =
                    Under(listener.transform, Hud_Manager.hud_object)
                    || Under(listener.transform, DamageMeter.DamageMeter_obj);
                var rect = listener.GetComponent<RectTransform>();
                Main.logger_instance?.Msg(
                    "[HoverTrace] candidateListener="
                        + Path(listener.transform)
                        + "; owned="
                        + owned
                        + "; enabled="
                        + listener.isActiveAndEnabled
                        + "; rect="
                        + (
                            rect.IsNullOrDestroyed()
                                ? "none"
                                : rect.rect.width + "x" + rect.rect.height
                        )
                        + "; allowWorldActions="
                        + listener.allowWorldActions
                );
            }
            Main.logger_instance?.Msg(
                "[HoverTrace] listener totals: active=" + active + "; blocking=" + blockers
            );
        }
        catch (Exception ex)
        {
            Main.logger_instance?.Warning(
                "[HoverTrace] Snapshot unavailable: " + ex.GetType().Name
            );
        }
    }

    private static bool Under(Transform node, GameObject root) =>
        !root.IsNullOrDestroyed() && node.IsChildOf(root.transform);

    private static string Path(Transform node)
    {
        var path = new StringBuilder();
        for (int depth = 0; depth < 10 && !node.IsNullOrDestroyed(); depth++, node = node.parent)
        {
            if (path.Length > 0)
                path.Insert(0, "/");
            path.Insert(0, node.name);
        }
        return path.ToString();
    }
}
