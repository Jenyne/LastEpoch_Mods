using System;
using System.Collections.Generic;
using System.Globalization;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // One shared adapter for legacy and dynamically-created slider rows.
    public static class NumericSliderInputs
    {
        sealed class Entry
        {
            public Slider slider;
            public Text label;
            public TMP_InputField input;
            public UnityEngine.Events.UnityAction<string> submit;
        }

        static readonly Dictionary<int, Entry> entries = new Dictionary<int, Entry>();
        static GameObject root;
        static float nextScan;

        public static void Tick(GameObject hud)
        {
            if (hud.IsNullOrDestroyed()) { return; }
            if (root != hud)
            {
                entries.Clear();
                root = hud;
                nextScan = 0f;
            }

            if (Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + 1f;
                Scan(hud);
            }

            foreach (var entry in entries.Values)
            {
                if (entry.slider.IsNullOrDestroyed() || entry.input.IsNullOrDestroyed()) { continue; }
                bool visible = entry.slider.gameObject.activeInHierarchy;
                entry.input.gameObject.SetActive(visible);
                if (!visible) { continue; }
                if (!entry.label.IsNullOrDestroyed()) { entry.label.gameObject.SetActive(false); }
                entry.input.interactable = entry.slider.interactable;
                if (!entry.input.isFocused)
                {
                    string value = Format(entry.slider);
                    if (entry.input.text != value) { entry.input.SetTextWithoutNotify(value); }
                }
            }
        }

        static void Scan(GameObject hud)
        {
            TMP_InputField template = null;
            foreach (var candidate in hud.GetComponentsInChildren<TMP_InputField>(true))
            {
                if (!candidate.IsNullOrDestroyed() && candidate.name == "InputField" &&
                    candidate.transform.parent != null && candidate.transform.parent.name == "Name")
                {
                    template = candidate;
                    break;
                }
            }
            if (template.IsNullOrDestroyed()) { return; }

            foreach (var slider in hud.GetComponentsInChildren<Slider>(true))
            {
                if (slider.IsNullOrDestroyed() || entries.ContainsKey(slider.GetInstanceID())) { continue; }
                // Existing amount-only controls intentionally hide their sliders.
                if (!slider.gameObject.activeSelf) { continue; }
                Text valueLabel = FindValueLabel(slider);
                if (valueLabel.IsNullOrDestroyed()) { continue; }
                try { Attach(slider, valueLabel, template); }
                catch (Exception ex)
                {
                    Main.logger_instance?.Warning("Numeric input setup failed for " + slider.name + ": " + ex.Message);
                }
            }
        }

        static Text FindValueLabel(Slider slider)
        {
            Transform parent = slider.transform.parent;
            for (int depth = 0; depth < 4 && parent != null; depth++, parent = parent.parent)
            {
                // Stop before crossing into another row or an entire settings panel.
                if (parent.GetComponentsInChildren<Slider>(true).Length != 1) { return null; }
                foreach (var text in parent.GetComponentsInChildren<Text>(true))
                {
                    if (!text.IsNullOrDestroyed() && text.name == "Value") { return text; }
                }
            }
            return null;
        }

        static void Attach(Slider slider, Text label, TMP_InputField template)
        {
            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, label.transform.parent);
            clone.name = "NumericInput_" + slider.GetInstanceID();
            TMP_InputField input = clone.GetComponent<TMP_InputField>();
            RectTransform source = label.GetComponent<RectTransform>();
            RectTransform rect = clone.GetComponent<RectTransform>();
            // Fixed right-aligned dimensions avoid inheriting narrow label anchors.
            rect.anchorMin = new Vector2(1f, source.anchorMin.y);
            rect.anchorMax = new Vector2(1f, source.anchorMax.y);
            rect.pivot = new Vector2(1f, source.pivot.y);
            rect.anchoredPosition = new Vector2(-6f, source.anchoredPosition.y);
            rect.localScale = Vector3.one;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(24f, source.rect.height));

            LayoutElement layout = clone.GetComponent<LayoutElement>();
            if (!layout.IsNullOrDestroyed()) { layout.ignoreLayout = true; }
            Image background = clone.GetComponent<Image>();
            if (!background.IsNullOrDestroyed())
            {
                background.sprite = null;
                background.type = Image.Type.Simple;
                background.color = new Color(0.11f, 0.13f, 0.16f, 1f);
            }
            ColorBlock colors = input.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            input.colors = colors;
            if (!input.placeholder.IsNullOrDestroyed()) { input.placeholder.gameObject.SetActive(false); }
            if (!input.textViewport.IsNullOrDestroyed())
            {
                input.textViewport.anchorMin = Vector2.zero;
                input.textViewport.anchorMax = Vector2.one;
                input.textViewport.offsetMin = new Vector2(4f, 1f);
                input.textViewport.offsetMax = new Vector2(-4f, -1f);
            }
            input.onValueChanged.RemoveAllListeners();
            input.onEndEdit = new TMP_InputField.SubmitEvent();
            input.enabled = true;
            input.readOnly = false;
            input.interactable = slider.interactable;
            input.contentType = slider.wholeNumbers ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber;
            input.characterLimit = 16;
            input.SetTextWithoutNotify(Format(slider));
            if (!input.targetGraphic.IsNullOrDestroyed()) { input.targetGraphic.raycastTarget = true; }
            if (!input.textComponent.IsNullOrDestroyed())
            {
                input.textComponent.raycastTarget = false;
                input.textComponent.fontSize = Mathf.Max(14f, label.fontSize);
                input.textComponent.enableAutoSizing = true;
                input.textComponent.fontSizeMin = 10f;
                input.textComponent.fontSizeMax = Mathf.Max(14f, label.fontSize);
                input.textComponent.color = label.color;
                input.textComponent.margin = Vector4.zero;
                RectTransform textRect = input.textComponent.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                input.textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
                input.textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
            }
            CanvasGroup group = input.GetComponent<CanvasGroup>();
            if (!group.IsNullOrDestroyed())
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            var entry = new Entry { slider = slider, label = label, input = input };
            entry.submit = (UnityEngine.Events.UnityAction<string>)(text => Commit(entry, entry.input.text));
            input.onEndEdit.AddListener(entry.submit);
            entries.Add(slider.GetInstanceID(), entry);
            label.gameObject.SetActive(false);
            clone.SetActive(slider.gameObject.activeInHierarchy);
        }

        static string Format(Slider slider)
        {
            return slider.value.ToString(slider.wholeNumbers ? "0" : "0.##", CultureInfo.InvariantCulture);
        }

        static void Commit(Entry entry, string text)
        {
            if (entry.slider.IsNullOrDestroyed() || entry.input.IsNullOrDestroyed()) { return; }
            if (entry.slider.interactable &&
                float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) &&
                !float.IsNaN(value) && !float.IsInfinity(value))
            {
                value = Mathf.Clamp(value, entry.slider.minValue, entry.slider.maxValue);
                if (entry.slider.wholeNumbers) { value = Mathf.Round(value); }
                else { value = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero); }
                value = Mathf.Clamp(value, entry.slider.minValue, entry.slider.maxValue);
                // Use the normal setter so existing config, labels and Harmony hooks run.
                entry.slider.value = value;
            }
            entry.input.SetTextWithoutNotify(Format(entry.slider));
        }
    }
}
