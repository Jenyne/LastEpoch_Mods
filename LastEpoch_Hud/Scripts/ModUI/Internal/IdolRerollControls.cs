using UnityEngine;
using UnityEngine.UI;
using LastEpoch_Hud.Scripts.Mods.Items;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class IdolRerollControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "IdolRerollOptions") != null) return;
            var sample = viewport.GetComponentInChildren<Text>(true);
            if (sample.IsNullOrDestroyed()) return;
            var section = Node(viewport, "IdolRerollOptions", 0, 0, 1, 1);
            var rect = section.GetComponent<RectTransform>();
            var layout = section.AddComponent<LayoutElement>();
            layout.minHeight = 112; layout.preferredHeight = 112; layout.flexibleHeight = 0;
            if (viewport.GetComponent<VerticalLayoutGroup>().IsNullOrDestroyed())
            {
                var parent = viewport.GetComponent<RectTransform>();
                float bottom = parent.rect.yMax;
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
                rect.anchoredPosition = new Vector2(0, bottom - parent.rect.yMax - 8);
                rect.sizeDelta = new Vector2(0, 112);
                parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(parent.rect.height, -rect.anchoredPosition.y + 112));
            }
            Label(section, "Title", sample, "Idol Rerolling", .03f, .72f, .97f, 1);
            AddToggle(section, sample, "FreeAmber", "No Memory Amber Cost", ModSettings.IdolReroll.FreeMemoryAmber, .40f, .72f);
            AddToggle(section, sample, "UnlimitedUses", "Unlimited Idol Altar Uses", ModSettings.IdolReroll.UnlimitedUses, .08f, .40f);
        }
        static void AddToggle(GameObject parent, Text sample, string name, string label, BoolSetting setting, float bottom, float top)
        {
            var go = Node(parent, name, .03f, bottom, .97f, top);
            var toggle = go.AddComponent<Toggle>();
            var box = Node(go, "Box", 0, .14f, .05f, .86f).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(box.gameObject, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f);
            toggle.targetGraphic = box; toggle.graphic = check;
            Label(go, "Label", sample, label, .07f, 0, 1, 1);
            toggle.SetIsOnWithoutNotify(setting.Value);
            Prefab.BindToggle(toggle, new System.Action<bool>(value => { setting.Set(value); Items_IdolReroll.RefreshUI(); }));
        }
        static GameObject Node(GameObject parent, string name, float left, float bottom, float right, float top)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>(); rect.SetParent(parent.transform, false);
            rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return go;
        }
        static void Label(GameObject parent, string name, Text sample, string label, float left, float bottom, float right, float top)
        {
            var text = Node(parent, name, left, bottom, right, top).AddComponent<Text>();
            text.font = sample.font; text.fontSize = 14; text.color = sample.color;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            Prefab.ApplyLabel(text, label);
        }
    }
}
