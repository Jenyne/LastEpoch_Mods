using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class SafeTeleportControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "SafeTeleport") != null)
                return;
            var sample = viewport.GetComponentInChildren<Text>(true);
            if (sample.IsNullOrDestroyed()) return;
            Font font = sample.font;
            var section = Node(viewport, "SafeTeleport", 0, 0, 1, 1);
            var rect = section.GetComponent<RectTransform>();
            var layout = section.AddComponent<LayoutElement>();
            layout.minHeight = 148; layout.preferredHeight = 148; layout.flexibleHeight = 0;
            if (viewport.GetComponent<VerticalLayoutGroup>().IsNullOrDestroyed())
            {
                // Older bundles may use explicit positions instead of a layout group.
                var parentRect = viewport.GetComponent<RectTransform>();
                float bottom = parentRect.rect.yMax;
                var corners = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
                for (int i = 0; i < viewport.transform.childCount; i++)
                {
                    var child = viewport.transform.GetChild(i).gameObject;
                    if (child == section || !child.activeSelf) continue;
                    var childRect = child.GetComponent<RectTransform>();
                    if (childRect.IsNullOrDestroyed()) continue;
                    childRect.GetWorldCorners(corners);
                    bottom = Mathf.Min(bottom, viewport.transform.InverseTransformPoint(corners[0]).y);
                }
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(.5f, 1);
                rect.anchoredPosition = new Vector2(0, bottom - parentRect.rect.yMax - 8);
                rect.sizeDelta = new Vector2(0, 148);
                parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(parentRect.rect.height, -rect.anchoredPosition.y + 148));
            }
            Label(section, "Title", font, "Safe Teleport", .03f, .80f, .97f, 1f);
            var toggleGo = Node(section, "Enabled", .03f, .57f, .97f, .80f);
            var toggle = toggleGo.AddComponent<Toggle>();
            var box = Node(toggleGo, "Box", 0, .1f, .075f, .9f).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(box.gameObject, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f);
            toggle.targetGraphic = box; toggle.graphic = check;
            Label(toggleGo, "Label", font, "Enable Safe Teleport", .1f, 0, 1, 1);
            var key = Node(section, "Key", .03f, .31f, .97f, .55f);
            Label(key, "Label", font, "Teleport Key", 0, 0, .40f, 1);
            Button(key, "Capture", font, .41f, .77f);
            Button(key, "Reset", font, .79f, 1f);
            Label(section, "Description", font,
                "Returns to the End of Time. Requires its waypoint. Unbound by default.",
                .03f, .01f, .97f, .29f);
            ModSettings.SafeTeleport.Group.ResolveAndBind(content);
        }

        static GameObject Node(GameObject parent, string name, float left, float bottom,
            float right, float top)
        {
            var go = new GameObject(name);
            // Match the HUD layer so the UI camera renders and raycasts these controls.
            go.layer = parent.layer;
            var r = go.AddComponent<RectTransform>();
            r.SetParent(parent.transform, false);
            r.anchorMin = new Vector2(left, bottom); r.anchorMax = new Vector2(right, top);
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            return go;
        }

        static Text Label(GameObject parent, string name, Font font, string label,
            float left, float bottom, float right, float top)
        {
            var text = Node(parent, name, left, bottom, right, top).AddComponent<Text>();
            text.font = font; text.fontSize = 14; text.color = new Color(.93f, .84f, .65f);
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            Prefab.ApplyLabel(text, label);
            return text;
        }

        static void Button(GameObject parent, string name, Font font, float left, float right)
        {
            var go = Node(parent, name, left, .05f, right, .95f);
            var image = go.AddComponent<Image>(); image.color = new Color(.18f, .20f, .23f);
            var button = go.AddComponent<Button>(); button.targetGraphic = image;
            Label(go, "Value", font, name == "Reset" ? "Clear" : "Unbound", .05f, 0, .95f, 1);
        }
    }
}
