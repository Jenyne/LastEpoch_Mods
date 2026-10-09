using System.Collections.Generic;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

// Single source of truth for the HUD's visual language. Runtime-built chrome and
// legacy prefab controls both consume these tokens so the look can be tuned here.
public static class HudTheme
{
    private readonly struct Palette
    {
        public readonly Color Backdrop, Background, Surface, Hover, Pressed, Selection;
        public readonly Color Accent, AccentSoft, AccentMuted, CardDivider;
        public readonly Color Text, TextMuted, Handle, Disabled;

        public Palette(
            Color backdrop,
            Color background,
            Color surface,
            Color hover,
            Color pressed,
            Color selection,
            Color accent,
            Color accentSoft,
            Color accentMuted,
            Color cardDivider,
            Color text,
            Color textMuted,
            Color handle,
            Color disabled
        )
        {
            Backdrop = backdrop;
            Background = background;
            Surface = surface;
            Hover = hover;
            Pressed = pressed;
            Selection = selection;
            Accent = accent;
            AccentSoft = accentSoft;
            AccentMuted = accentMuted;
            CardDivider = cardDivider;
            Text = text;
            TextMuted = textMuted;
            Handle = handle;
            Disabled = disabled;
        }
    }

    private readonly struct LegacyFontMetrics
    {
        public readonly int Size, Minimum, Maximum;
        public LegacyFontMetrics(Text text)
        {
            Size = text.fontSize;
            Minimum = text.resizeTextMinSize;
            Maximum = text.resizeTextMaxSize;
        }
    }

    private readonly struct TmpFontMetrics
    {
        public readonly float Size, Minimum, Maximum;
        public TmpFontMetrics(TMP_Text text)
        {
            Size = text.fontSize;
            Minimum = text.fontSizeMin;
            Maximum = text.fontSizeMax;
        }
    }

    private static readonly Palette DarkPalette = new(
        Rgba(0, 0, 0, 166),
        Rgba(21, 21, 21, 255),       // #151515 main background
        Rgba(14, 14, 16, 255),       // #0E0E10 cards and controls
        Rgba(35, 35, 39, 255),
        Rgba(52, 43, 34, 255),
        Rgba(40, 40, 44, 255),       // selected items are visibly lighter
        Rgba(213, 180, 122, 255),    // #D5B47A
        Rgba(213, 180, 122, 166),
        Rgba(213, 180, 122, 205),
        Rgba(255, 255, 255, 102),
        Rgba(250, 250, 250, 255),
        Rgba(250, 250, 250, 184),
        Color.white,
        Rgba(85, 83, 80, 160)
    );

    private static readonly Palette LightPalette = new(
        Rgba(0, 0, 0, 120),
        Rgba(241, 241, 243, 255),
        Rgba(255, 255, 255, 255),
        Rgba(235, 228, 218, 255),
        Rgba(222, 207, 184, 255),
        Rgba(231, 221, 206, 255),
        Rgba(151, 101, 31, 255),
        Rgba(151, 101, 31, 166),
        Rgba(151, 101, 31, 205),
        Rgba(0, 0, 0, 72),
        Rgba(24, 24, 27, 255),
        Rgba(63, 63, 70, 210),
        Rgba(55, 55, 60, 255),
        Rgba(145, 145, 150, 170)
    );

    private static readonly Dictionary<int, LegacyFontMetrics> LegacyFontSizes = new();
    private static readonly Dictionary<int, TmpFontMetrics> TmpFontSizes = new();
    private static bool preferencesLoaded;
    private static bool lightMode;
    private static float fontScale = 1f;

    private static Palette Current => lightMode ? LightPalette : DarkPalette;
    public static bool LightMode => lightMode;
    public static float FontScale => fontScale;

    public static Color Backdrop => Current.Backdrop;
    public static Color Background => Current.Background;
    public static Color Header => Current.Background;
    public static Color Sidebar => Current.Background;
    public static Color Surface => Current.Surface;
    public static Color SurfaceRaised => Current.Surface;
    public static Color SurfaceHover => Current.Hover;
    public static Color SurfacePressed => Current.Pressed;
    public static Color Selection => Current.Selection;
    public static Color Accent => Current.Accent;
    public static Color AccentBright => Current.Accent;
    public static Color AccentSoft => Current.AccentSoft;
    public static Color AccentMuted => Current.AccentMuted;
    public static Color Border => Current.Accent;
    public static Color Divider => Current.Accent;
    public static Color CardDivider => Current.CardDivider;
    public static Color TextPrimary => Current.Text;
    public static Color TextSecondary => Current.Text;
    public static Color TextMuted => Current.TextMuted;
    public static Color ControlHandle => Current.Handle;
    public static Color ControlTrack => Current.Accent;
    public static Color ControlBox => Current.Surface;
    public static Color ControlCheck => Current.Accent;
    public static Color InputBackground => Current.Surface;
    public static Color ItemSelection => Current.Selection;
    public static Color ControlDisabled => Current.Disabled;

    public static readonly Vector2 WindowAnchorMin = new(0.08f, 0.08f);
    public static readonly Vector2 WindowAnchorMax = new(0.92f, 0.92f);

    public const float HeaderHeight = 72f;
    public const float SidebarWidth = 270f;
    public const float ContentPadding = 24f;
    public const float CardGap = 18f;
    public const float CardHorizontalInset = 34f;
    public const float BorderWidth = 1f;
    public const float ControlBorderWidth = 2f;
    public const float SectionHeight = 52f;
    public const float PageHeight = 44f;
    public const float NavigationIndent = 44f;
    public const float CardTitleHeight = 54f;
    public const float RowHeight = 46f;
    public const float SliderRowHeight = 74f;
    public const float ButtonRowHeight = 44f;
    public const float ToggleSize = 22f;
    public const float SliderCardPadding = 34f;
    public const float SliderCardRowHeight = 82f;
    public const float SliderCardRowGap = 12f;
    public const float SliderTrackHeight = 6f;
    public const float SliderHandleSize = 18f;
    public const float SliderValueWidth = 126f;
    public const float SliderScrollbarWidth = 12f;
    public const float ActionButtonHeight = 54f;
    public const float ActionButtonGap = 12f;

    public const int BrandFontSize = 34;
    public const int SectionFontSize = 24;
    public const int PageFontSize = 20;
    public const int CardTitleFontSize = 25;
    public const int BodyFontSize = 17;
    public const int ValueFontSize = 16;
    public const int SliderCardTitleFontSize = 32;

    public static ColorBlock ButtonColors(Color normal, Color selected)
    {
        return new ColorBlock
        {
            normalColor = normal,
            highlightedColor = SurfaceHover,
            pressedColor = SurfacePressed,
            selectedColor = selected,
            disabledColor = ControlDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.08f,
        };
    }

    public static ColorBlock ActionButtonColors(Color normal, Color selected)
    {
        var colors = ButtonColors(normal, selected);
        colors.highlightedColor = SurfaceHover;
        colors.pressedColor = AccentMuted;
        colors.fadeDuration = 0.04f;
        return colors;
    }

    public static void LoadPreferences()
    {
        if (preferencesLoaded)
            return;
        preferencesLoaded = true;
        lightMode = PlayerPrefs.GetInt("LEHUD.LightMode", 0) != 0;
        fontScale = Mathf.Clamp(PlayerPrefs.GetFloat("LEHUD.FontScale", 1f), 0.8f, 1.4f);
    }

    public static void SetLightMode(GameObject root, bool enabled)
    {
        LoadPreferences();
        if (lightMode == enabled)
            return;
        Palette previous = Current;
        lightMode = enabled;
        ApplyPalette(root, previous, Current);
        NormalizeSelectableGraphics(root);
        PlayerPrefs.SetInt("LEHUD.LightMode", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetFontScale(GameObject root, float scale)
    {
        LoadPreferences();
        fontScale = Mathf.Clamp(Mathf.Round(scale * 20f) / 20f, 0.8f, 1.4f);
        ApplyFontScale(root);
        PlayerPrefs.SetFloat("LEHUD.FontScale", fontScale);
        PlayerPrefs.Save();
    }

    public static void ApplyFontScale(GameObject root)
    {
        if (root.IsNullOrDestroyed())
            return;
        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            int id = text.GetInstanceID();
            if (!LegacyFontSizes.TryGetValue(id, out var baseline))
            {
                baseline = new LegacyFontMetrics(text);
                LegacyFontSizes[id] = baseline;
            }
            text.fontSize = Mathf.Max(8, Mathf.RoundToInt(baseline.Size * fontScale));
            text.resizeTextMinSize = Mathf.Max(8, Mathf.RoundToInt(baseline.Minimum * fontScale));
            text.resizeTextMaxSize = Mathf.Max(8, Mathf.RoundToInt(baseline.Maximum * fontScale));
        }
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            int id = text.GetInstanceID();
            if (!TmpFontSizes.TryGetValue(id, out var baseline))
            {
                baseline = new TmpFontMetrics(text);
                TmpFontSizes[id] = baseline;
            }
            text.fontSize = Mathf.Max(8f, baseline.Size * fontScale);
            text.fontSizeMin = Mathf.Max(8f, baseline.Minimum * fontScale);
            text.fontSizeMax = Mathf.Max(8f, baseline.Maximum * fontScale);
        }
        Canvas.ForceUpdateCanvases();
    }

    public static void ResetFontBaselines()
    {
        LegacyFontSizes.Clear();
        TmpFontSizes.Clear();
    }

    // ColorBlock values are the final visual colors. Keeping a tinted base
    // Graphic would multiply the two colors and make #0E0E10 appear black.
    public static void NormalizeSelectableGraphics(GameObject root)
    {
        if (root.IsNullOrDestroyed())
            return;
        foreach (var selectable in root.GetComponentsInChildren<Selectable>(true))
        {
            Color normal = selectable.colors.normalColor;
            bool themed = selectable is Slider
                || Same(normal, Current.Surface)
                || Same(normal, Current.Background)
                || Same(normal, Current.Selection)
                || Same(normal, Current.Hover)
                || Same(normal, Current.Accent)
                || Same(normal, Current.AccentMuted);
            if (themed && !selectable.targetGraphic.IsNullOrDestroyed())
                selectable.targetGraphic.color = Color.white;
        }
    }

    private static void ApplyPalette(GameObject root, Palette from, Palette to)
    {
        if (root.IsNullOrDestroyed())
            return;
        foreach (var image in root.GetComponentsInChildren<Image>(true))
            image.color = Map(image.color, from, to);
        foreach (var text in root.GetComponentsInChildren<Text>(true))
            text.color = Map(text.color, from, to);
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            text.color = Map(text.color, from, to);
        foreach (var outline in root.GetComponentsInChildren<Outline>(true))
            outline.effectColor = Map(outline.effectColor, from, to);
        foreach (var selectable in root.GetComponentsInChildren<Selectable>(true))
        {
            var colors = selectable.colors;
            colors.normalColor = Map(colors.normalColor, from, to);
            colors.highlightedColor = Map(colors.highlightedColor, from, to);
            colors.pressedColor = Map(colors.pressedColor, from, to);
            colors.selectedColor = Map(colors.selectedColor, from, to);
            colors.disabledColor = Map(colors.disabledColor, from, to);
            selectable.colors = colors;
        }
    }

    private static Color Map(Color value, Palette from, Palette to)
    {
        if (Same(value, from.Backdrop)) return to.Backdrop;
        if (Same(value, from.Background)) return to.Background;
        if (Same(value, from.Surface)) return to.Surface;
        if (Same(value, from.Hover)) return to.Hover;
        if (Same(value, from.Pressed)) return to.Pressed;
        if (Same(value, from.Selection)) return to.Selection;
        if (Same(value, from.Accent)) return to.Accent;
        if (Same(value, from.AccentSoft)) return to.AccentSoft;
        if (Same(value, from.AccentMuted)) return to.AccentMuted;
        if (Same(value, from.CardDivider)) return to.CardDivider;
        if (Same(value, from.Text)) return to.Text;
        if (Same(value, from.TextMuted)) return to.TextMuted;
        if (Same(value, from.Handle)) return to.Handle;
        if (Same(value, from.Disabled)) return to.Disabled;
        return value;
    }

    private static bool Same(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 0.004f
        && Mathf.Abs(a.g - b.g) < 0.004f
        && Mathf.Abs(a.b - b.b) < 0.004f
        && Mathf.Abs(a.a - b.a) < 0.004f;

    private static Color Rgba(byte r, byte g, byte b, byte a) => new Color32(r, g, b, a);
}

internal static class HudStyler
{
    // Selectable color transitions tint their target Graphic, which makes an
    // Outline attached to that same Graphic look muted. Independent edge
    // images retain the exact primary color in every interaction state.
    public static void AddPrimaryBorder(GameObject target, float thickness = HudTheme.ControlBorderWidth)
    {
        if (target.IsNullOrDestroyed())
            return;
        float half = thickness * 0.5f;
        AddBorderEdge(target, "LEHUD_BorderTop", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -half), new Vector2(0f, thickness));
        AddBorderEdge(target, "LEHUD_BorderBottom", Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, half), new Vector2(0f, thickness));
        AddBorderEdge(target, "LEHUD_BorderLeft", Vector2.zero, new Vector2(0f, 1f), new Vector2(half, 0f), new Vector2(thickness, 0f));
        AddBorderEdge(target, "LEHUD_BorderRight", new Vector2(1f, 0f), Vector2.one, new Vector2(-half, 0f), new Vector2(thickness, 0f));
    }

    private static void AddBorderEdge(
        GameObject target,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta
    )
    {
        var edge = Prefab.Child(target, name);
        if (edge.IsNullOrDestroyed())
        {
            edge = new GameObject(name);
            edge.layer = target.layer;
            edge.AddComponent<RectTransform>().SetParent(target.transform, false);
            var image = edge.AddComponent<Image>();
            image.color = HudTheme.Border;
            image.raycastTarget = false;
        }
        var rect = edge.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        var edgeImage = edge.GetComponent<Image>();
        edgeImage.color = HudTheme.Border;
        edgeImage.raycastTarget = false;
        edge.transform.SetAsLastSibling();
    }

    public static void ApplyPanel(GameObject panel)
    {
        if (panel.IsNullOrDestroyed())
            return;

        var background = panel.GetComponent<Image>();
        if (background.IsNullOrDestroyed())
            background = panel.AddComponent<Image>();
        background.color = HudTheme.Surface;

        var outline = panel.GetComponent<Outline>();
        if (outline.IsNullOrDestroyed())
            outline = panel.AddComponent<Outline>();
        outline.effectColor = HudTheme.Border;
        outline.effectDistance = new Vector2(HudTheme.BorderWidth, -HudTheme.BorderWidth);
        outline.useGraphicAlpha = false;

        foreach (var text in panel.GetComponentsInChildren<Text>(true))
            ApplyText(text, IsTitle(text.gameObject));
        foreach (var text in panel.GetComponentsInChildren<TMP_Text>(true))
            ApplyText(text, IsTitle(text.gameObject));
        foreach (var button in panel.GetComponentsInChildren<Button>(true))
            ApplyButton(button);
        foreach (var slider in panel.GetComponentsInChildren<Slider>(true))
            ApplySlider(slider);
        foreach (var toggle in panel.GetComponentsInChildren<Toggle>(true))
            ApplyToggle(toggle);
        foreach (var dropdown in panel.GetComponentsInChildren<Dropdown>(true))
            ApplyDropdown(dropdown);
        foreach (var scrollbar in panel.GetComponentsInChildren<Scrollbar>(true))
            ApplyScrollbar(scrollbar);
        foreach (var scroll in panel.GetComponentsInChildren<ScrollRect>(true))
        {
            var image = scroll.GetComponent<Image>();
            if (!image.IsNullOrDestroyed())
                image.color = HudTheme.Surface;
            if (!scroll.viewport.IsNullOrDestroyed())
            {
                var viewportImage = scroll.viewport.GetComponent<Image>();
                if (!viewportImage.IsNullOrDestroyed())
                    viewportImage.color = HudTheme.Surface;
            }
        }
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            var child = panel.transform.GetChild(i).gameObject;
            if (!child.name.Contains("Title", System.StringComparison.Ordinal))
                continue;
            var image = child.GetComponent<Image>();
            if (!image.IsNullOrDestroyed())
                image.color = HudTheme.Surface;
        }
        if (panel.name != "Old_ForceDrop_Content")
            ApplyRows(panel);
    }

    public static void ApplyButton(Button button)
    {
        if (button.IsNullOrDestroyed())
            return;
        var image = button.GetComponent<Image>();
        if (!image.IsNullOrDestroyed())
            image.color = HudTheme.SurfaceRaised;
        button.colors = HudTheme.ActionButtonColors(HudTheme.SurfaceRaised, HudTheme.Selection);
        foreach (var text in button.GetComponentsInChildren<Text>(true))
        {
            text.color = HudTheme.TextPrimary;
            text.fontSize = Mathf.Max(text.fontSize, HudTheme.ValueFontSize);
        }
        foreach (var text in button.GetComponentsInChildren<TMP_Text>(true))
        {
            text.color = HudTheme.TextPrimary;
            text.fontSize = Mathf.Max(text.fontSize, HudTheme.ValueFontSize);
        }
    }

    public static void ApplyText(Text text, bool title = false)
    {
        if (text.IsNullOrDestroyed())
            return;
        text.color = title ? HudTheme.TextPrimary : HudTheme.TextSecondary;
        text.fontSize = title
            ? Mathf.Max(text.fontSize, HudTheme.CardTitleFontSize)
            : Mathf.Max(text.fontSize, HudTheme.BodyFontSize);
    }

    public static void ApplyText(TMP_Text text, bool title = false)
    {
        if (text.IsNullOrDestroyed())
            return;
        text.color = title ? HudTheme.TextPrimary : HudTheme.TextSecondary;
        text.fontSize = title
            ? Mathf.Max(text.fontSize, HudTheme.CardTitleFontSize)
            : Mathf.Max(text.fontSize, HudTheme.BodyFontSize);
    }

    public static void ApplySlider(Slider slider)
    {
        if (slider.IsNullOrDestroyed())
            return;
        var background = Prefab.Child(slider.gameObject, "Background");
        var backgroundImage = background.IsNullOrDestroyed() ? null : background.GetComponent<Image>();
        if (!backgroundImage.IsNullOrDestroyed())
            backgroundImage.color = HudTheme.ControlTrack;
        if (!slider.fillRect.IsNullOrDestroyed())
        {
            var fill = slider.fillRect.GetComponent<Image>();
            if (!fill.IsNullOrDestroyed())
                fill.color = HudTheme.Accent;
        }
        if (!slider.targetGraphic.IsNullOrDestroyed())
            slider.targetGraphic.color = HudTheme.ControlHandle;
    }

    public static void ApplyToggle(Toggle toggle)
    {
        if (toggle.IsNullOrDestroyed())
            return;
        if (!toggle.targetGraphic.IsNullOrDestroyed())
            toggle.targetGraphic.color = HudTheme.SurfaceRaised;
        if (!toggle.graphic.IsNullOrDestroyed())
            toggle.graphic.color = HudTheme.Accent;
        toggle.colors = HudTheme.ButtonColors(HudTheme.SurfaceRaised, HudTheme.SurfaceHover);
    }

    private static void ApplyRows(GameObject panel)
    {
        Prefab.ForEachDescendant(
            panel,
            candidate =>
            {
                if (candidate.name != "Content" || candidate.transform.parent == null)
                    return;
                if (candidate.transform.parent.gameObject.name != "Viewport")
                    return;

                foreach (var group in candidate.GetComponents<LayoutGroup>())
                    group.enabled = false;
                var vertical = candidate.GetComponent<VerticalLayoutGroup>();
                if (vertical.IsNullOrDestroyed())
                    vertical = candidate.AddComponent<VerticalLayoutGroup>();
                vertical.enabled = true;
                vertical.padding = new RectOffset(12, 12, 8, 8);
                vertical.spacing = 0f;
                vertical.childAlignment = TextAnchor.UpperLeft;
                vertical.childControlWidth = true;
                vertical.childControlHeight = true;
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;

                var fitter = candidate.GetComponent<ContentSizeFitter>();
                if (fitter.IsNullOrDestroyed())
                    fitter = candidate.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                for (int i = 0; i < candidate.transform.childCount; i++)
                {
                    var row = candidate.transform.GetChild(i).gameObject;
                    if (row.name.StartsWith("Border", System.StringComparison.Ordinal))
                    {
                        row.SetActive(false);
                        continue;
                    }
                    ApplyRow(row);
                }
            }
        );
    }

    private static void ApplyRow(GameObject row)
    {
        var toggle = DirectComponent<Toggle>(row);
        var slider = DirectComponent<Slider>(row);
        var dropdown = DirectComponent<Dropdown>(row);
        var button = row.GetComponent<Button>();
        float height = slider.IsNullOrDestroyed()
            ? (button.IsNullOrDestroyed() ? HudTheme.RowHeight : HudTheme.ButtonRowHeight)
            : HudTheme.SliderRowHeight;

        var element = row.GetComponent<LayoutElement>();
        if (element.IsNullOrDestroyed())
            element = row.AddComponent<LayoutElement>();
        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleHeight = 0f;

        var rowImage = row.GetComponent<Image>();
        if (rowImage.IsNullOrDestroyed())
            rowImage = row.AddComponent<Image>();
        rowImage.color = HudTheme.Surface;
        rowImage.raycastTarget = false;

        var oldDivider = Prefab.Child(row, "LEHUD_RowDivider");
        if (oldDivider.IsNullOrDestroyed())
        {
            oldDivider = new GameObject("LEHUD_RowDivider");
            oldDivider.layer = row.layer;
            oldDivider.AddComponent<RectTransform>().SetParent(row.transform, false);
            var divider = oldDivider.AddComponent<Image>();
            divider.color = HudTheme.Divider;
            divider.raycastTarget = false;
        }
        var dividerRect = oldDivider.GetComponent<RectTransform>();
        dividerRect.anchorMin = Vector2.zero;
        dividerRect.anchorMax = new Vector2(1f, 0f);
        dividerRect.pivot = new Vector2(0.5f, 0f);
        dividerRect.anchoredPosition = Vector2.zero;
        dividerRect.sizeDelta = new Vector2(0f, HudTheme.BorderWidth);
        oldDivider.transform.SetAsLastSibling();

        if (!button.IsNullOrDestroyed())
        {
            rowImage.raycastTarget = true;
            ApplyButton(button);
            var outline = row.GetComponent<Outline>();
            if (outline.IsNullOrDestroyed())
                outline = row.AddComponent<Outline>();
            outline.effectColor = HudTheme.Border;
            outline.effectDistance = new Vector2(1f, -1f);
        }

        if (!toggle.IsNullOrDestroyed())
            ArrangeToggle(toggle, slider);
        if (!slider.IsNullOrDestroyed())
            ArrangeSlider(slider);
        if (!dropdown.IsNullOrDestroyed())
            ArrangeDropdown(row, dropdown);
    }

    private static void ArrangeToggle(Toggle toggle, Slider slider)
    {
        var toggleRect = toggle.GetComponent<RectTransform>();
        toggleRect.anchorMin = Vector2.zero;
        toggleRect.anchorMax = Vector2.one;
        toggleRect.offsetMin = Vector2.zero;
        toggleRect.offsetMax = Vector2.zero;

        var boxObject = Prefab.Child(toggle.gameObject, "Background");
        if (!boxObject.IsNullOrDestroyed())
        {
            var boxRect = boxObject.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(1f, 1f);
            boxRect.anchorMax = new Vector2(1f, 1f);
            boxRect.pivot = new Vector2(1f, 1f);
            boxRect.anchoredPosition = new Vector2(-14f, slider.IsNullOrDestroyed() ? -12f : -10f);
            boxRect.sizeDelta = new Vector2(HudTheme.ToggleSize, HudTheme.ToggleSize);
            var box = boxObject.GetComponent<Image>();
            if (!box.IsNullOrDestroyed())
                box.color = HudTheme.ControlBox;
            var outline = boxObject.GetComponent<Outline>();
            if (outline.IsNullOrDestroyed())
                outline = boxObject.AddComponent<Outline>();
            outline.effectColor = HudTheme.Border;
            outline.effectDistance = new Vector2(1f, -1f);
        }

        var labelObject = Prefab.Child(toggle.gameObject, "Label");
        if (!labelObject.IsNullOrDestroyed())
        {
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.anchoredPosition = new Vector2(14f, slider.IsNullOrDestroyed() ? -8f : -7f);
            labelRect.sizeDelta = new Vector2(-96f, 30f);
            var label = labelObject.GetComponent<Text>();
            if (!label.IsNullOrDestroyed())
            {
                label.alignment = TextAnchor.MiddleLeft;
                label.color = HudTheme.TextPrimary;
            }
        }

        var valueObject = Prefab.Child(toggle.gameObject, "Value");
        if (!valueObject.IsNullOrDestroyed())
        {
            var valueRect = valueObject.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1f, 1f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.pivot = new Vector2(1f, 1f);
            valueRect.anchoredPosition = new Vector2(-52f, -7f);
            valueRect.sizeDelta = new Vector2(116f, 30f);
            var value = valueObject.GetComponent<Text>();
            if (!value.IsNullOrDestroyed())
            {
                value.alignment = TextAnchor.MiddleRight;
                value.color = HudTheme.TextPrimary;
            }
        }
        ApplyToggle(toggle);
    }

    private static void ArrangeSlider(Slider slider)
    {
        var rect = slider.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 13f);
        rect.sizeDelta = new Vector2(-28f, 18f);
        ApplySlider(slider);
    }

    private static void ArrangeDropdown(GameObject row, Dropdown dropdown)
    {
        var dropdownRect = dropdown.GetComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.42f, 0.5f);
        dropdownRect.anchorMax = new Vector2(1f, 0.5f);
        dropdownRect.pivot = new Vector2(1f, 0.5f);
        dropdownRect.anchoredPosition = new Vector2(-14f, 0f);
        dropdownRect.sizeDelta = new Vector2(-14f, 32f);
        foreach (var text in row.GetComponentsInChildren<Text>(true))
            text.color = HudTheme.TextPrimary;
    }

    private static T DirectComponent<T>(GameObject root)
        where T : Component
    {
        var own = root.GetComponent<T>();
        if (!own.IsNullOrDestroyed())
            return own;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            var component = root.transform.GetChild(i).GetComponent<T>();
            if (!component.IsNullOrDestroyed())
                return component;
        }
        return null;
    }

    public static void ApplyDropdown(Dropdown dropdown)
    {
        if (dropdown.IsNullOrDestroyed())
            return;
        var image = dropdown.GetComponent<Image>();
        if (!image.IsNullOrDestroyed())
            image.color = HudTheme.SurfaceRaised;
        dropdown.colors = HudTheme.ButtonColors(HudTheme.SurfaceRaised, HudTheme.Selection);
        if (!dropdown.template.IsNullOrDestroyed())
        {
            foreach (var childImage in dropdown.template.GetComponentsInChildren<Image>(true))
            {
                string name = childImage.gameObject.name;
                if (name.Contains("Checkmark", System.StringComparison.OrdinalIgnoreCase))
                    childImage.color = HudTheme.Accent;
                else if (name.Contains("Handle", System.StringComparison.OrdinalIgnoreCase))
                    childImage.color = HudTheme.AccentMuted;
                else
                    childImage.color = HudTheme.ControlBox;
            }
            foreach (var option in dropdown.template.GetComponentsInChildren<Toggle>(true))
            {
                if (!option.targetGraphic.IsNullOrDestroyed())
                    option.targetGraphic.color = HudTheme.ControlBox;
                if (!option.graphic.IsNullOrDestroyed())
                    option.graphic.color = HudTheme.Accent;
                option.colors = HudTheme.ButtonColors(HudTheme.ControlBox, HudTheme.SurfaceHover);
            }
            foreach (var optionText in dropdown.template.GetComponentsInChildren<Text>(true))
                optionText.color = HudTheme.TextPrimary;
        }
    }

    private static void ApplyScrollbar(Scrollbar scrollbar)
    {
        if (scrollbar.IsNullOrDestroyed())
            return;
        var image = scrollbar.GetComponent<Image>();
        if (!image.IsNullOrDestroyed())
            image.color = HudTheme.Surface;
        if (!scrollbar.targetGraphic.IsNullOrDestroyed())
            scrollbar.targetGraphic.color = HudTheme.AccentMuted;
        scrollbar.colors = HudTheme.ButtonColors(HudTheme.AccentMuted, HudTheme.Accent);
    }

    private static bool IsTitle(GameObject obj)
    {
        if (obj.IsNullOrDestroyed())
            return false;
        var current = obj.transform;
        for (int i = 0; i < 3 && current != null; i++, current = current.parent)
            if (current.gameObject.name.Contains("Title"))
                return true;
        return false;
    }
}
