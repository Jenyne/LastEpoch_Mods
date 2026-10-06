using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class InfiniteForgingPotentialControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed()
                || Prefab.Child(viewport, "InfiniteForgingPotential") != null) return;
            var panel = Prefab.Child(viewport, "ForginPotencial");
            if (panel.IsNullOrDestroyed()) return;
            var original = Prefab.Child(panel, "Toggle_Items_Craft_ForginPotencial");
            if (original.IsNullOrDestroyed()) return;
            var row = Node(viewport, "InfiniteForgingPotential", 0, 1, 1, 1);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.pivot = new Vector2(.5f, 1);
            rowRect.sizeDelta = new Vector2(0, 34);
            row.AddComponent<LayoutElement>().preferredHeight = 34;
            // Reuse the native prefab's checkbox geometry, sprites, font and transitions.
            var control = UnityEngine.Object.Instantiate(original, row.transform);
            control.name = "Toggle_InfiniteForgingPotential";
            var rect = control.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = new Vector2(.74f, 1);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var toggle = control.GetComponent<Toggle>();
            if (toggle.IsNullOrDestroyed()) { UnityEngine.Object.Destroy(row); return; }
            toggle.group = null; toggle.interactable = true;
            toggle.onValueChanged.RemoveAllListeners();
            var value = Prefab.Child(control, "Value");
            if (!value.IsNullOrDestroyed()) value.SetActive(false);
            var labelObject = Prefab.Child(control, "Label");
            var label = labelObject.IsNullOrDestroyed() ? null : labelObject.GetComponent<Text>();
            if (!label.IsNullOrDestroyed()) Prefab.ApplyLabel(label, "Infinite Forging Potential");

            var border = panel.GetComponent<Image>();
            if (!border.IsNullOrDestroyed())
            {
                var background = row.AddComponent<Image>();
                background.sprite = border.sprite; background.type = border.type;
                background.color = border.color; background.raycastTarget = false;
            }
            // Clone a regular HUD button so this action follows the same theme too.
            var templates = content.GetComponentsInChildren<Button>(true);
            if (templates.Length > 0)
            {
                var buttonObject = UnityEngine.Object.Instantiate(templates[0].gameObject, row.transform);
                buttonObject.name = "Btn_Craft_DeselectAll";
                var buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(.76f, .12f);
                buttonRect.anchorMax = new Vector2(.99f, .88f);
                buttonRect.offsetMin = buttonRect.offsetMax = Vector2.zero;
                var button = buttonObject.GetComponent<Button>();
                button.onClick.RemoveAllListeners(); button.interactable = true;
                var text = buttonObject.GetComponentInChildren<Text>(true);
                if (!text.IsNullOrDestroyed()) Prefab.ApplyLabel(text, "Deselect All");
                Prefab.BindButton(button, new System.Action(() => DeselectAll(viewport)));
                buttonObject.SetActive(true);
            }
            ModSettings.InfiniteForgingPotential.Enabled.Changed += enabled =>
            {
                if (!toggle.IsNullOrDestroyed()) toggle.SetIsOnWithoutNotify(enabled);
            };
            toggle.SetIsOnWithoutNotify(ModSettings.InfiniteForgingPotential.Enabled.Value);
            Prefab.BindToggle(toggle, new System.Action<bool>(enabled =>
            {
                ModSettings.InfiniteForgingPotential.Enabled.Set(enabled);
                Main.logger_instance?.Msg("Infinite Forging Potential: " + (enabled ? "enabled" : "disabled"));
            }));
            MelonLoader.MelonCoroutines.Start(PositionRow(viewport, row));
            Main.logger_instance?.Msg("Infinite Forging Potential checkbox bound in Items > Crafting.");
        }

        static void DeselectAll(GameObject viewport)
        {
            // Legacy crafting settings update on pointer clicks, not value-change events.
            // Clear their saved flags explicitly; merely unchecking the visuals is insufficient.
            if (!Scripts.Save_Manager.instance.IsNullOrDestroyed())
            {
                var settings = Scripts.Save_Manager.instance.data.Items.CraftingSlot;
                settings.Enable_ForginPotencial = false;
                settings.Enable_Implicit_0 = false;
                settings.Enable_Implicit_1 = false;
                settings.Enable_Implicit_2 = false;
                settings.Enable_Seal_Tier = false;
                settings.Enable_Seal_Value = false;
                settings.Enable_Affix_0_Tier = false;
                settings.Enable_Affix_1_Tier = false;
                settings.Enable_Affix_2_Tier = false;
                settings.Enable_Affix_3_Tier = false;
                settings.Enable_Affix_0_Value = false;
                settings.Enable_Affix_1_Value = false;
                settings.Enable_Affix_2_Value = false;
                settings.Enable_Affix_3_Value = false;
                settings.Enable_UniqueMod_0 = false;
                settings.Enable_UniqueMod_1 = false;
                settings.Enable_UniqueMod_2 = false;
                settings.Enable_UniqueMod_3 = false;
                settings.Enable_UniqueMod_4 = false;
                settings.Enable_UniqueMod_5 = false;
                settings.Enable_UniqueMod_6 = false;
                settings.Enable_UniqueMod_7 = false;
                settings.Enable_LegendaryPotencial = false;
                settings.Enable_WeaverWill = false;
                Scripts.Save_Manager.instance.data.Items.CraftingSlot = settings;
                Scripts.Save_Manager.instance.Save();
            }
            var toggles = viewport.GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle.IsNullOrDestroyed() || toggle.gameObject.name == "Toggle_InfiniteForgingPotential") continue;
                toggle.SetIsOnWithoutNotify(false);
            }
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
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + 38);
            for (int i = 0; i < rects.Count; i++)
            {
                var child = rects[i];
                child.anchorMin = child.anchorMax = new Vector2(.5f, 1);
                child.sizeDelta = sizes[i];
                // The old pivot's position, measured from the old content top.
                child.anchoredPosition = new Vector2(centers[i].x, centers[i].y - height / 2 - 38);
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
