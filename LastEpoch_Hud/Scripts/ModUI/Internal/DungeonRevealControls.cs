using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class DungeonRevealControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "DungeonReveal") != null)
                return;
            var sample = viewport.GetComponentInChildren<Text>(true);
            if (sample.IsNullOrDestroyed()) return;
            Font font = sample.font;
            var section = Node(viewport, "DungeonReveal", 0, 0, 1, 1);
            var rect = section.GetComponent<RectTransform>();
            var layout = section.AddComponent<LayoutElement>();
            layout.minHeight = 76; layout.preferredHeight = 76; layout.flexibleHeight = 0;
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
                rect.sizeDelta = new Vector2(0, 76);
                parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(parentRect.rect.height, -rect.anchoredPosition.y + 76));
            }
            Label(section, "Title", font, "Dungeon Objective Reveal", .03f, .61f, .97f, 1f);
            var toggleGo = Node(section, "Enabled", .03f, .06f, .97f, .59f);
            var toggle = toggleGo.AddComponent<Toggle>();
            var box = Node(toggleGo, "Box", 0, .1f, .075f, .9f).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(box.gameObject, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f); toggle.targetGraphic = box; toggle.graphic = check;
            Label(toggleGo, "Label", font, "Reveal Dungeon Objectives", .1f, 0, 1, 1);
            toggle.SetIsOnWithoutNotify(ModSettings.DungeonReveal.Enabled.Value);
            Prefab.BindToggle(toggle, new System.Action<bool>(v => ModSettings.DungeonReveal.Enabled.Set(v)));
        }

        static GameObject Node(GameObject parent, string name, float left, float bottom,
            float right, float top)
        {
            var go = new GameObject(name);
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

    }
}
