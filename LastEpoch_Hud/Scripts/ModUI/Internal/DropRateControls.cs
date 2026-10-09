using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class DropRateControls
    {
        static readonly HashSet<int> pending = new();
        static readonly Dictionary<int, Action<bool>> toggles = new();
        static readonly Dictionary<int, Action<float>> sliders = new();
        static readonly Dictionary<int, Action<string>> inputs = new();
        static bool reportedMissingContent;

        public static void Bind(GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed())
            {
                if (!reportedMissingContent)
                    Main.logger_instance?.Warning(
                        "[DropRates] Controls not bound: live Drop content is unavailable."
                    );
                reportedMissingContent = true;
                return;
            }
            reportedMissingContent = false;
            if (
                Prefab.Child(viewport, "NaturalDropRates") != null
                || !pending.Add(viewport.GetInstanceID())
            )
                return;
            MelonCoroutines.Start(BindWhenVisible(viewport, viewport.GetInstanceID()));
        }

        static IEnumerator BindWhenVisible(GameObject viewport, int id)
        {
            try
            {
                while (!viewport.IsNullOrDestroyed() && !viewport.activeInHierarchy)
                    yield return null;
                if (viewport.IsNullOrDestroyed())
                    yield break;
                yield return null;
                Canvas.ForceUpdateCanvases();
                Build(viewport);
            }
            finally
            {
                pending.Remove(id);
            }
        }

        static void Build(GameObject viewport)
        {
            var sample = viewport.GetComponentInChildren<Text>(true);
            if (sample.IsNullOrDestroyed() || sample.font.IsNullOrDestroyed())
            {
                Main.logger_instance?.Warning(
                    "[DropRates] Controls not bound: no native menu font sample."
                );
                return;
            }
            toggles.Clear();
            sliders.Clear();
            inputs.Clear();
            var section = Node(viewport, "NaturalDropRates", 0, 0, 1, 1);
            var rect = section.GetComponent<RectTransform>();
            var layout = section.AddComponent<LayoutElement>();
            const float height = 288;
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0;
            if (viewport.GetComponent<VerticalLayoutGroup>().IsNullOrDestroyed())
            {
                // Measure only after the inactive menu has a live layout. Preserve
                // legacy anchored rows while increasing the scroll content height.
                var parent = viewport.GetComponent<RectTransform>();
                var children = new List<(RectTransform Rect, Vector3 Position, Vector2 Size)>();
                float oldHeight = parent.rect.height;
                float oldTop = parent.rect.yMax;
                Vector3 oldWorldTop = parent.TransformPoint(
                    new Vector3(parent.rect.center.x, oldTop, 0)
                );
                float bottom = oldTop;
                var corners =
                    new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
                for (int i = 0; i < viewport.transform.childCount; i++)
                {
                    var child = viewport.transform.GetChild(i).gameObject;
                    if (child == section)
                        continue;
                    var childRect = child.GetComponent<RectTransform>();
                    if (childRect.IsNullOrDestroyed())
                        continue;
                    children.Add((childRect, childRect.localPosition, childRect.rect.size));
                    if (!child.activeSelf)
                        continue;
                    childRect.GetWorldCorners(corners);
                    bottom = Mathf.Min(
                        bottom,
                        viewport.transform.InverseTransformPoint(corners[0]).y
                    );
                }
                float offset = bottom - oldTop - 8;
                parent.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    Mathf.Max(oldHeight, -offset + height)
                );
                // Content pivots differ between HUD versions. Keep its visible top
                // fixed when growing it, rather than shifting legacy rows upward.
                parent.position +=
                    oldWorldTop
                    - parent.TransformPoint(new Vector3(parent.rect.center.x, parent.rect.yMax, 0));
                foreach (var child in children)
                {
                    child.Rect.anchorMin = child.Rect.anchorMax = new Vector2(.5f, 1);
                    child.Rect.sizeDelta = child.Size;
                    child.Rect.anchoredPosition = new Vector2(
                        child.Position.x - parent.rect.center.x,
                        child.Position.y - oldTop
                    );
                }
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(.5f, 1);
                rect.anchoredPosition = new Vector2(0, offset);
                rect.sizeDelta = new Vector2(0, height);
            }
            Label(section, "Title", sample, "Natural Drop Rates", .03f, .89f, .97f, 1);
            Row(
                section,
                sample,
                "Unique",
                "Unique Drop Rate",
                ModSettings.DropRates.Unique,
                .71f,
                .89f
            );
            Row(section, sample, "Set", "Set Drop Rate", ModSettings.DropRates.Set, .53f, .71f);
            Row(
                section,
                sample,
                "Exalted",
                "Exalted Affix Chance",
                ModSettings.DropRates.Exalted,
                .35f,
                .53f
            );
            Row(section, sample, "T7", "T7 Affix Chance", ModSettings.DropRates.T7, .17f, .35f);
            Label(
                section,
                "Description",
                sample,
                "100% = normal. Range: 0–1000%. Forced rarity takes priority.",
                .03f,
                .01f,
                .97f,
                .17f
            );
            LayoutRebuilder.MarkLayoutForRebuild(viewport.GetComponent<RectTransform>());
            Main.logger_instance?.Msg(
                "[DropRates] Four Natural Drop Rates controls bound in Items > Drop."
            );
        }

        static int Percent(FloatSetting setting) =>
            float.IsNaN(setting.Value) || float.IsInfinity(setting.Value)
                ? 100
                : Mathf.Clamp(Mathf.RoundToInt(setting.Value), 0, 1000);

        static void Row(
            GameObject parent,
            Text sample,
            string name,
            string label,
            FloatSetting setting,
            float bottom,
            float top
        )
        {
            var row = Node(parent, name + "RateRow", .03f, bottom, .97f, top);
            var toggleGo = Node(row, "Enabled", 0, .56f, .05f, .94f);
            var toggle = toggleGo.AddComponent<Toggle>();
            var box = Node(toggleGo, "Box", 0, 0, 1, 1).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(toggleGo, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f);
            toggle.targetGraphic = box;
            toggle.graphic = check;
            Label(row, "Label", sample, label, .07f, .52f, .99f, 1);
            var sliderGo = Node(row, "RateSlider", .07f, .08f, .76f, .44f);
            var slider = sliderGo.AddComponent<Slider>();
            var track = Node(sliderGo, "Track", 0, .35f, 1, .65f).AddComponent<Image>();
            track.color = new Color(.22f, .24f, .27f);
            var fillArea = Node(sliderGo, "FillArea", 0, .35f, 1, .65f);
            var fill = Node(fillArea, "Fill", 0, 0, 1, 1).AddComponent<Image>();
            fill.color = new Color(.7f, .57f, .3f);
            fill.raycastTarget = false;
            var handleArea = Node(sliderGo, "HandleArea", 0, 0, 1, 1);
            var handle = Node(handleArea, "Handle", 0, 0, 0, 1).AddComponent<Image>();
            handle.rectTransform.sizeDelta = new Vector2(12, 0);
            handle.color = new Color(.93f, .84f, .65f);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0;
            slider.maxValue = 1000;
            slider.wholeNumbers = true;
            var fieldGo = Node(row, "PercentInput", .79f, .04f, .94f, .48f);
            var background = fieldGo.AddComponent<Image>();
            background.color = new Color(.11f, .13f, .16f);
            var field = fieldGo.AddComponent<InputField>();
            field.targetGraphic = background;
            field.textComponent = Label(fieldGo, "InputText", sample, "", .06f, 0, .94f, 1);
            field.contentType = InputField.ContentType.IntegerNumber;
            Label(row, "PercentSign", sample, "%", .95f, .04f, 1, .48f);
            void Refresh()
            {
                slider.SetValueWithoutNotify(Percent(setting));
                field.SetTextWithoutNotify(Percent(setting).ToString(CultureInfo.InvariantCulture));
                slider.interactable = setting.Enabled;
                field.interactable = setting.Enabled;
                toggle.SetIsOnWithoutNotify(setting.Enabled);
            }
            Refresh();
            setting.Changed += () =>
            {
                if (!row.IsNullOrDestroyed())
                    Refresh();
            };
            toggles[toggle.GetInstanceID()] = value =>
            {
                setting.SetEnabled(value);
                Refresh();
            };
            sliders[slider.GetInstanceID()] = value =>
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                setting.SetValue(Mathf.Clamp(Mathf.RoundToInt(value), 0, 1000));
                Refresh();
            };
            inputs[field.GetInstanceID()] = text =>
            {
                if (
                    int.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int value
                    )
                )
                    setting.SetValue(Mathf.Clamp(value, 0, 1000));
                Refresh();
            };
        }

        // Native pointer/drag paths do not reliably invoke managed event listeners.
        // Only controls registered by this section are handled.
        [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
        static class ToggleClick
        {
            [HarmonyPostfix]
            static void Postfix(Toggle __instance)
            {
                if (
                    __instance.interactable
                    && toggles.TryGetValue(__instance.GetInstanceID(), out var action)
                )
                    action(__instance.isOn);
            }
        }

        [HarmonyPatch(typeof(Slider), "Set", new Type[] { typeof(float), typeof(bool) })]
        static class SliderSet
        {
            [HarmonyPostfix]
            static void Postfix(Slider __instance, bool __1)
            {
                if (
                    __1
                    && __instance.interactable
                    && sliders.TryGetValue(__instance.GetInstanceID(), out var action)
                )
                    action(__instance.value);
            }
        }

        [HarmonyPatch(typeof(InputField), "DeactivateInputField")]
        static class InputCommit
        {
            [HarmonyPostfix]
            static void Postfix(InputField __instance)
            {
                if (
                    __instance.interactable
                    && inputs.TryGetValue(__instance.GetInstanceID(), out var action)
                )
                    action(__instance.text);
            }
        }

        static GameObject Node(
            GameObject parent,
            string name,
            float left,
            float bottom,
            float right,
            float top
        )
        {
            var go = new GameObject(name);
            go.layer = parent.layer;
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent.transform, false);
            rect.anchorMin = new Vector2(left, bottom);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        static Text Label(
            GameObject parent,
            string name,
            Text sample,
            string label,
            float left,
            float bottom,
            float right,
            float top
        )
        {
            var text = Node(parent, name, left, bottom, right, top).AddComponent<Text>();
            text.font = sample.font;
            text.fontSize = 13;
            text.color = sample.color;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            Prefab.ApplyLabel(text, label);
            return text;
        }
    }
}
