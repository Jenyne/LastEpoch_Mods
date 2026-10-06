using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class ProphecyRewardControls
    {
        const string ToggleName = "Toggle_ProphecyRewardMultiplier";
        const string SliderName = "Slider_ProphecyRewardMultiplier";
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "ProphecyReward") != null) return;
            var template = Prefab.Child(viewport, "FavorMultiplier");
            if (template.IsNullOrDestroyed())
            {
                Main.logger_instance?.Warning("Prophecy rewards: Favor Multiplier template missing.");
                return;
            }
            var row = UnityEngine.Object.Instantiate(template, viewport.transform);
            row.name = "ProphecyReward";
            var toggles = row.GetComponentsInChildren<Toggle>(true);
            var sliders = row.GetComponentsInChildren<Slider>(true);
            if (toggles.Length == 0 || sliders.Length == 0)
            { UnityEngine.Object.Destroy(row); return; }
            var toggle = toggles[0]; var slider = sliders[0];
            toggle.gameObject.name = ToggleName; slider.gameObject.name = SliderName;
            toggle.onValueChanged.RemoveAllListeners(); slider.onValueChanged.RemoveAllListeners();
            toggle.group = null; toggle.interactable = true; slider.interactable = true;
            slider.minValue = 1; slider.maxValue = 10; slider.wholeNumbers = true;
            var label = Prefab.Child(toggle.gameObject, "Label");
            if (!label.IsNullOrDestroyed())
                Prefab.ApplyLabel(label.GetComponent<Text>(), "Prophecy Reward Multiplier");
            var setting = ModSettings.ProphecyRewards.Multiplier;
            toggle.SetIsOnWithoutNotify(setting.Enabled);
            slider.SetValueWithoutNotify(Mathf.Clamp(Mathf.RoundToInt(setting.Value), 1, 10));
            setting.Changed += () =>
            {
                if (!toggle.IsNullOrDestroyed()) toggle.SetIsOnWithoutNotify(setting.Enabled);
                if (!slider.IsNullOrDestroyed()) slider.SetValueWithoutNotify(Mathf.Clamp(Mathf.RoundToInt(setting.Value), 1, 10));
                UpdateValue(row, setting.Value);
            };
            UpdateValue(row, setting.Value);
            MelonLoader.MelonCoroutines.Start(Position(viewport, row));
            row.SetActive(true);
            Main.logger_instance?.Msg("Prophecy Reward Multiplier bound under Character > Cheats (1–10).");
        }

        static void UpdateValue(GameObject row, float value)
        {
            if (row.IsNullOrDestroyed()) return;
            foreach (var text in row.GetComponentsInChildren<Text>(true))
                if (text.gameObject.name == "Value") text.text = Mathf.Clamp(Mathf.RoundToInt(value), 1, 10).ToString();
        }

        [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
        internal static class ToggleClick
        {
            [HarmonyPostfix]
            static void Postfix(Toggle __instance)
            {
                if (__instance.IsNullOrDestroyed() || __instance.name != ToggleName || !__instance.interactable) return;
                ModSettings.ProphecyRewards.Multiplier.SetEnabled(__instance.isOn);
            }
        }

        [HarmonyPatch(typeof(Slider), "Set", new System.Type[] { typeof(float), typeof(bool) })]
        internal static class SliderChanged
        {
            [HarmonyPostfix]
            static void Postfix(Slider __instance, bool sendCallback)
            {
                if (!sendCallback || __instance.IsNullOrDestroyed() || __instance.name != SliderName) return;
                ModSettings.ProphecyRewards.Multiplier.SetValue(Mathf.Clamp(Mathf.RoundToInt(__instance.value), 1, 10));
            }
        }

        static System.Collections.IEnumerator Position(GameObject viewport, GameObject row)
        {
            while (!viewport.IsNullOrDestroyed() && !viewport.activeInHierarchy) yield return null;
            if (viewport.IsNullOrDestroyed() || row.IsNullOrDestroyed()) yield break;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var rect = row.GetComponent<RectTransform>();
            var content = viewport.GetComponent<RectTransform>();
            float rowHeight = Mathf.Max(40, rect.rect.height);
            if (!viewport.GetComponent<VerticalLayoutGroup>().IsNullOrDestroyed())
            {
                var layout = row.GetComponent<LayoutElement>();
                if (layout.IsNullOrDestroyed()) layout = row.AddComponent<LayoutElement>();
                layout.preferredHeight = rowHeight;
                row.transform.SetAsLastSibling();
                LayoutRebuilder.MarkLayoutForRebuild(content);
                yield break;
            }
            // Freeze existing geometry before increasing scroll height, so stretched
            // anchors do not move the current rows when the new row is appended.
            float height = content.rect.height;
            var rects = new System.Collections.Generic.List<RectTransform>();
            var positions = new System.Collections.Generic.List<Vector2>();
            var sizes = new System.Collections.Generic.List<Vector2>();
            float bottom = 0;
            for (int i = 0; i < viewport.transform.childCount; i++)
            {
                var child = viewport.transform.GetChild(i).GetComponent<RectTransform>();
                if (child.IsNullOrDestroyed() || child.gameObject == row) continue;
                var position = new Vector2(child.localPosition.x, child.localPosition.y) - content.rect.center;
                position.y -= height / 2;
                rects.Add(child); positions.Add(position); sizes.Add(child.rect.size);
                if (child.gameObject.activeSelf)
                    bottom = Mathf.Min(bottom, position.y - child.pivot.y * child.rect.height);
            }
            float nextHeight = Mathf.Max(height, -bottom + rowHeight + 8);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nextHeight);
            for (int i = 0; i < rects.Count; i++)
            {
                rects[i].anchorMin = rects[i].anchorMax = new Vector2(.5f, 1);
                rects[i].sizeDelta = sizes[i]; rects[i].anchoredPosition = positions[i];
            }
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.sizeDelta = new Vector2(0, rowHeight);
            rect.anchoredPosition = new Vector2(0, bottom - 4);
        }
    }
}
