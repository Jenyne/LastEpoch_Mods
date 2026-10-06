using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    internal static class DropRateControls
    {
        public static void Bind(GameObject content, GameObject viewport)
        {
            if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "NaturalDropRates") != null) return;
            var sample = viewport.GetComponentInChildren<Text>(true);
            if (sample.IsNullOrDestroyed()) return;
            var section = Node(viewport, "NaturalDropRates", 0, 0, 1, 1);
            var rect = section.GetComponent<RectTransform>();
            var layout = section.AddComponent<LayoutElement>();
            const float height = 288;
            layout.minHeight = height; layout.preferredHeight = height; layout.flexibleHeight = 0;
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
                rect.sizeDelta = new Vector2(0, height);
                parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(parent.rect.height, -rect.anchoredPosition.y + height));
            }
            Label(section, "Title", sample, "Natural Drop Rates", .03f, .89f, .97f, 1);
            Row(section, sample, "Unique", "Unique Drop Rate", ModSettings.DropRates.Unique, .71f, .89f);
            Row(section, sample, "Set", "Set Drop Rate", ModSettings.DropRates.Set, .53f, .71f);
            Row(section, sample, "Exalted", "Exalted Affix Chance", ModSettings.DropRates.Exalted, .35f, .53f);
            Row(section, sample, "T7", "T7 Affix Chance", ModSettings.DropRates.T7, .17f, .35f);
            Label(section, "Description", sample, "100% = normal. Range: 0–1000%. Forced rarity takes priority.", .03f, .01f, .97f, .17f);
        }
        static int Percent(FloatSetting setting) => float.IsNaN(setting.Value) || float.IsInfinity(setting.Value)
            ? 100 : Mathf.Clamp(Mathf.RoundToInt(setting.Value), 0, 1000);
        static void Row(GameObject parent, Text sample, string name, string label, FloatSetting setting, float bottom, float top)
        {
            var row = Node(parent, name + "RateRow", .03f, bottom, .97f, top);
            var toggleGo = Node(row, "Enabled", 0, .56f, .05f, .94f);
            var toggle = toggleGo.AddComponent<Toggle>();
            var box = Node(toggleGo, "Box", 0, 0, 1, 1).AddComponent<Image>();
            box.color = new Color(.22f, .24f, .27f);
            var check = Node(toggleGo, "Check", .2f, .2f, .8f, .8f).AddComponent<Image>();
            check.color = new Color(.9f, .73f, .4f); toggle.targetGraphic = box; toggle.graphic = check;
            Label(row, "Label", sample, label, .07f, .52f, .99f, 1);
            var sliderGo = Node(row, "RateSlider", .07f, .08f, .76f, .44f);
            var slider = sliderGo.AddComponent<Slider>();
            var track = Node(sliderGo, "Track", 0, .35f, 1, .65f).AddComponent<Image>();
            track.color = new Color(.22f, .24f, .27f);
            var fillArea = Node(sliderGo, "FillArea", 0, .35f, 1, .65f);
            var fill = Node(fillArea, "Fill", 0, 0, 1, 1).AddComponent<Image>();
            fill.color = new Color(.7f, .57f, .3f); fill.raycastTarget = false;
            var handleArea = Node(sliderGo, "HandleArea", 0, 0, 1, 1);
            var handle = Node(handleArea, "Handle", 0, 0, 0, 1).AddComponent<Image>();
            handle.rectTransform.sizeDelta = new Vector2(12, 0);
            handle.color = new Color(.93f, .84f, .65f);
            slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.minValue = 0; slider.maxValue = 1000; slider.wholeNumbers = true;
            var fieldGo = Node(row, "PercentInput", .79f, .04f, .94f, .48f);
            var background = fieldGo.AddComponent<Image>(); background.color = new Color(.11f, .13f, .16f);
            var field = fieldGo.AddComponent<InputField>(); field.targetGraphic = background;
            field.textComponent = Label(fieldGo, "InputText", sample, "", .06f, 0, .94f, 1);
            field.contentType = InputField.ContentType.IntegerNumber;
            Label(row, "PercentSign", sample, "%", .95f, .04f, 1, .48f);
            void Refresh()
            {
                slider.SetValueWithoutNotify(Percent(setting));
                field.SetTextWithoutNotify(Percent(setting).ToString(CultureInfo.InvariantCulture));
                slider.interactable = setting.Enabled; field.interactable = setting.Enabled;
                toggle.SetIsOnWithoutNotify(setting.Enabled);
            }
            Refresh();
            Prefab.BindToggle(toggle, new Action<bool>(value => { setting.SetEnabled(value); Refresh(); }));
            SliderHook.Register(slider, value =>
            {
                if (float.IsNaN(value) || float.IsInfinity(value)) return;
                setting.SetValue(Mathf.Clamp(Mathf.RoundToInt(value), 0, 1000));
                Refresh();
            });
            field.onEndEdit.AddListener(new Action<string>(text =>
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    setting.SetValue(Mathf.Clamp(value, 0, 1000));
                Refresh();
            }));
        }
        static GameObject Node(GameObject parent, string name, float left, float bottom, float right, float top)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>(); rect.SetParent(parent.transform, false);
            rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return go;
        }
        static Text Label(GameObject parent, string name, Text sample, string label, float left, float bottom, float right, float top)
        {
            var text = Node(parent, name, left, bottom, right, top).AddComponent<Text>();
            text.font = sample.font; text.fontSize = 13; text.color = sample.color;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            Prefab.ApplyLabel(text, label);
            return text;
        }
    }
}
