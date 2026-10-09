using System;
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
            int count = 0;
            foreach (var listener in UnityEngine.Object.FindObjectsOfType<UIMouseListener>())
            {
                if (listener.IsNullOrDestroyed() || !listener.isActiveAndEnabled)
                    continue;
                if (count++ >= 8)
                    break;
                var rect = listener.GetComponent<RectTransform>();
                Main.logger_instance?.Msg(
                    "[HoverTrace] activeListener="
                        + Path(listener.transform)
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
        }
        catch (Exception ex)
        {
            Main.logger_instance?.Warning(
                "[HoverTrace] Snapshot unavailable: " + ex.GetType().Name
            );
        }
    }

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
