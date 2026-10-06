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
            var sourceToggle = original.GetComponent<Toggle>();
            if (Prefab.Child(viewport, "InfiniteForgingPotential") != null) return;
            var go = Node(viewport, "InfiniteForgingPotential", 0, 1, 1, 1);
            var rowRect = go.GetComponent<RectTransform>();
            rowRect.pivot = new Vector2(.5f, 1);
            rowRect.sizeDelta = new Vector2(0, 30);
            go.AddComponent<LayoutElement>().preferredHeight = 30;
            var toggle = go.AddComponent<Toggle>();
            var box = Node(go, "Box", .03f, .5f, .03f, .5f).AddComponent<Image>();
            box.rectTransform.sizeDelta = new Vector2(18, 18);
            var sourceBox = sourceToggle.IsNullOrDestroyed() ? null : sourceToggle.targetGraphic as Image;
            box.sprite = sourceBox.IsNullOrDestroyed() ? null : sourceBox.sprite;
            box.type = sourceBox.IsNullOrDestroyed() ? Image.Type.Simple : sourceBox.type;
            box.color = sourceBox.IsNullOrDestroyed() ? new Color(.22f, .24f, .27f) : sourceBox.color;
            var check = Node(box.gameObject, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            var sourceCheck = sourceToggle.IsNullOrDestroyed() ? null : sourceToggle.graphic as Image;
            check.sprite = sourceCheck.IsNullOrDestroyed() ? null : sourceCheck.sprite;
            check.color = sourceCheck.IsNullOrDestroyed() ? new Color(.9f, .73f, .4f) : sourceCheck.color;
            box.raycastTarget = true; check.raycastTarget = false;
            toggle.targetGraphic = box; toggle.graphic = check;
            if (!sourceToggle.IsNullOrDestroyed())
            { toggle.colors = sourceToggle.colors; toggle.transition = sourceToggle.transition; }
            var label = Node(go, "Label", .10f, 0, .98f, 1).AddComponent<Text>();
            label.font = sample.font; label.fontSize = 13; label.color = sample.color;
            label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false;
            Prefab.ApplyLabel(label, "Infinite Forging Potential");
            ModSettings.InfiniteForgingPotential.Enabled.Changed += value =>
            {
                if (!toggle.IsNullOrDestroyed()) toggle.SetIsOnWithoutNotify(value);
            };
            Main.logger_instance?.Msg("Infinite Forging Potential checkbox bound in Items > Crafting.");
            MelonLoader.MelonCoroutines.Start(PositionRow(viewport, go));
            toggle.SetIsOnWithoutNotify(ModSettings.InfiniteForgingPotential.Enabled.Value);
            Prefab.BindToggle(toggle, new System.Action<bool>(v => ModSettings.InfiniteForgingPotential.Enabled.Set(v)));
        }
        static System.Collections.IEnumerator PositionRow(GameObject viewport, GameObject row)
        {
            while (!viewport.IsNullOrDestroyed() && !viewport.activeInHierarchy) yield return null;
            if (viewport.IsNullOrDestroyed() || row.IsNullOrDestroyed()) yield break;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var contentRect = viewport.GetComponent<RectTransform>();
            if (contentRect.IsNullOrDestroyed()) yield break;
            if (!viewport.GetComponent<VerticalLayoutGroup>().IsNullOrDestroyed())
            {
                row.transform.SetAsFirstSibling();
                LayoutRebuilder.MarkLayoutForRebuild(contentRect);
                yield break;
            }
            // Preserve existing row geometry before enlarging the scroll content.
            var rects = new System.Collections.Generic.List<RectTransform>();
            var centers = new System.Collections.Generic.List<Vector2>();
            var sizes = new System.Collections.Generic.List<Vector2>();
            for (int i = 0; i < viewport.transform.childCount; i++)
            {
                var child = viewport.transform.GetChild(i).GetComponent<RectTransform>();
                if (child.IsNullOrDestroyed() || child.gameObject == row) continue;
                rects.Add(child);
                centers.Add(new Vector2(child.localPosition.x, child.localPosition.y) - contentRect.rect.center);
                sizes.Add(child.rect.size);
            }
            float height = contentRect.rect.height;
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + 34);
            for (int i = 0; i < rects.Count; i++)
            {
                var child = rects[i];
                child.anchorMin = child.anchorMax = new Vector2(.5f, 1);
                child.sizeDelta = sizes[i];
                // The old pivot's position, measured from the old content top.
                child.anchoredPosition = new Vector2(centers[i].x, centers[i].y - height / 2 - 34);
            }
            row.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        static GameObject Node(GameObject parent, string name, float left, float bottom, float right, float top)
        {
            var go = new GameObject(name);
            go.layer = parent.layer;
            var rect = go.AddComponent<RectTransform>(); rect.SetParent(parent.transform, false);
            rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return go;
        }
    }
}
