using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class InfiniteForgingPotentialControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed()) return;
            var panel = Prefab.Child(viewport, "ForginPotencial");
            if (panel.IsNullOrDestroyed() || Prefab.Child(panel, "InfiniteForgingPotential") != null) return;
            var original = Prefab.Child(panel, "Toggle_Items_Craft_ForginPotencial");
            var sample = panel.GetComponentInChildren<Text>(true);
            if (original.IsNullOrDestroyed() || sample.IsNullOrDestroyed()) return;
            var oldRect = original.GetComponent<RectTransform>();
            if (!oldRect.IsNullOrDestroyed()) oldRect.anchorMax = new Vector2(.66f, oldRect.anchorMax.y);
            var go = Node(panel, "InfiniteForgingPotential", .69f, .58f, .99f, .98f);
            var toggle = go.AddComponent<Toggle>();
            var box = Node(go, "Box", 0, .12f, .20f, .88f).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(box.gameObject, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f); toggle.targetGraphic = box; toggle.graphic = check;
            var label = Node(go, "Label", .25f, 0, 1, 1).AddComponent<Text>();
            label.font = sample.font; label.fontSize = 13; label.color = sample.color;
            label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false;
            Prefab.ApplyLabel(label, "Infinite");
            toggle.SetIsOnWithoutNotify(ModSettings.InfiniteForgingPotential.Enabled.Value);
            Prefab.BindToggle(toggle, new System.Action<bool>(v => ModSettings.InfiniteForgingPotential.Enabled.Set(v)));
        }
        static GameObject Node(GameObject parent, string name, float left, float bottom, float right, float top)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>(); rect.SetParent(parent.transform, false);
            rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return go;
        }
    }
}
