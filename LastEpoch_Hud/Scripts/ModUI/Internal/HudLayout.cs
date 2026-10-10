using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

// Owns the shared HUD shell. Page metadata and lifecycle behavior are declared in
// HudNavigation; individual page files own their content.
public static class HudLayout
{
    private sealed class NavigationButton
    {
        public Button Button;
        public Image Background;
        public GameObject Accent;
    }

    private sealed class NavigationSection
    {
        public HudSectionDefinition Definition;
        public Text Indicator;
        public readonly List<GameObject> PageRows = new();
        public bool Expanded;
    }

    private static readonly Dictionary<string, NavigationButton> pageButtons = new();
    private static readonly List<NavigationSection> navigationSections = new();
    private static GameObject boundHud;
    private static GameObject window;
    private static Font font;
    private static HudPageDefinition activePage;
    private static GameObject settingsPanel;
    private static Slider settingsFontSlider;
    private static Text settingsFontValue;
    private static Button lightModeButton;
    private static Button darkModeButton;

    public static void Initialize(GameObject hud)
    {
        if (hud.IsNullOrDestroyed() || boundHud == hud)
            return;

        boundHud = hud;
        pageButtons.Clear();
        navigationSections.Clear();
        HudSearch.Reset();
        activePage = null;
        HudTheme.LoadPreferences();
        HudStyler.ResetFontBaselines();
        font = FindFont(hud);

        var content = Prefab.Child(hud, "Content");
        var menu = Prefab.Child(hud, "Menu");
        if (content.IsNullOrDestroyed() || menu.IsNullOrDestroyed())
        {
            Main.logger_instance?.Warning("HudLayout: missing Content or Menu root");
            return;
        }

        window = BuildWindow(hud);
        menu.transform.SetParent(window.transform, false);
        content.transform.SetParent(window.transform, false);
        BuildHeader(window);
        BuildNavigation(menu);
        PlaceLegacyContent(content);
        foreach (var page in HudNavigation.Pages)
            page.Build(window, hud, font);

        var defaultPage = HudNavigation.Sections[0].Pages[0];
        ExpandSection(HudNavigation.Sections[0].Id, true);
        ActivateNavigationOnly(defaultPage);
        HudStyler.NormalizeSelectableGraphics(window);
        HudStyler.ApplyFontScale(window);
        RefreshSettingsControls();
        Main.logger_instance?.Msg("HudLayout: replacement navigation initialized");
    }

    private static GameObject BuildWindow(GameObject hud)
    {
        var backdrop = Node(hud, "LEHUD_Backdrop");
        Stretch(backdrop.GetComponent<RectTransform>());
        var image = backdrop.AddComponent<Image>();
        image.color = HudTheme.Backdrop;
        image.raycastTarget = false;
        backdrop.transform.SetAsFirstSibling();

        var windowObject = Node(hud, "LEHUD_Window");
        var windowRect = windowObject.GetComponent<RectTransform>();
        windowRect.anchorMin = HudTheme.WindowAnchorMin;
        windowRect.anchorMax = HudTheme.WindowAnchorMax;
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;
        var windowImage = windowObject.AddComponent<Image>();
        windowImage.color = HudTheme.Background;
        var outline = windowObject.AddComponent<Outline>();
        outline.effectColor = HudTheme.Border;
        outline.effectDistance = new Vector2(1f, -1f);
        return windowObject;
    }

    private static void BuildHeader(GameObject hud)
    {
        var header = Node(hud, "LEHUD_Header");
        var rect = header.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, HudTheme.HeaderHeight);
        var background = header.AddComponent<Image>();
        background.color = HudTheme.Header;
        background.raycastTarget = false;

        var brand = TextNode(header, "Brand", "LE HUD MOD", HudTheme.BrandFontSize);
        var brandRect = brand.GetComponent<RectTransform>();
        brandRect.anchorMin = Vector2.zero;
        brandRect.anchorMax = Vector2.one;
        brandRect.offsetMin = new Vector2(26f, 0f);
        brandRect.offsetMax = new Vector2(-HudTheme.HeaderHeight, 0f);
        brand.alignment = TextAnchor.MiddleLeft;
        brand.color = HudTheme.TextPrimary;

        var closeObject = Node(header, "Close");
        var closeRect = closeObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0.5f);
        closeRect.anchorMax = new Vector2(1f, 0.5f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-24f, 0f);
        closeRect.sizeDelta = new Vector2(48f, 48f);
        var closeImage = closeObject.AddComponent<Image>();
        closeImage.color = HudTheme.Surface;
        var outline = closeObject.AddComponent<Outline>();
        outline.effectColor = HudTheme.Border;
        outline.effectDistance = new Vector2(1f, -1f);
        var close = closeObject.AddComponent<Button>();
        close.targetGraphic = closeImage;
        close.colors = HudTheme.ButtonColors(HudTheme.Surface, HudTheme.SurfaceHover);
        Prefab.BindButton(close, new Action(Close));
        var closeText = TextNode(closeObject, "Label", "X", 25);
        Stretch(closeText.GetComponent<RectTransform>());
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = HudTheme.TextPrimary;

        var settingsObject = Node(header, "Settings");
        var settingsRect = settingsObject.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(1f, 0.5f);
        settingsRect.anchorMax = new Vector2(1f, 0.5f);
        settingsRect.pivot = new Vector2(1f, 0.5f);
        settingsRect.anchoredPosition = new Vector2(-84f, 0f);
        settingsRect.sizeDelta = new Vector2(48f, 48f);
        var settingsImage = settingsObject.AddComponent<Image>();
        settingsImage.color = HudTheme.Surface;
        HudStyler.AddPrimaryBorder(settingsObject, 1f);
        var settingsButton = settingsObject.AddComponent<Button>();
        settingsButton.targetGraphic = settingsImage;
        settingsButton.colors = HudTheme.ButtonColors(HudTheme.Surface, HudTheme.Selection);
        ButtonHook.Register(settingsButton, ToggleSettings);
        var settingsIcon = TextNode(settingsObject, "Label", "⚙", 27);
        Stretch(settingsIcon.GetComponent<RectTransform>());
        settingsIcon.alignment = TextAnchor.MiddleCenter;
        settingsIcon.color = HudTheme.TextPrimary;

        var line = Node(header, "Border");
        var lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(1f, 0f);
        lineRect.pivot = new Vector2(0.5f, 0f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0f, HudTheme.BorderWidth);
        var lineImage = line.AddComponent<Image>();
        lineImage.color = HudTheme.Border;
        lineImage.raycastTarget = false;

        BuildSettingsPanel(hud);
        HudSearchBar.Build(header, hud, boundHud, font, ActivateSearchMatch);
    }

    private static void BuildSettingsPanel(GameObject parent)
    {
        settingsPanel = Node(parent, "LEHUD_SettingsPanel");
        var rect = settingsPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -HudTheme.HeaderHeight - 12f);
        rect.sizeDelta = new Vector2(420f, 250f);
        var image = settingsPanel.AddComponent<Image>();
        image.color = HudTheme.Surface;
        HudStyler.AddPrimaryBorder(settingsPanel);

        var title = TextNode(settingsPanel, "Title", "Settings", HudTheme.CardTitleFontSize);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(22f, -54f);
        titleRect.offsetMax = new Vector2(-22f, -8f);
        title.alignment = TextAnchor.MiddleLeft;

        var divider = Node(settingsPanel, "TitleDivider");
        var dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0f, 1f);
        dividerRect.anchorMax = new Vector2(1f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 1f);
        dividerRect.anchoredPosition = new Vector2(0f, -58f);
        dividerRect.sizeDelta = new Vector2(-44f, HudTheme.BorderWidth);
        var dividerImage = divider.AddComponent<Image>();
        dividerImage.color = HudTheme.CardDivider;
        dividerImage.raycastTarget = false;

        var fontLabel = TextNode(
            settingsPanel,
            "FontSizeLabel",
            "Font Size",
            HudTheme.BodyFontSize
        );
        var fontLabelRect = fontLabel.GetComponent<RectTransform>();
        fontLabelRect.anchorMin = new Vector2(0f, 1f);
        fontLabelRect.anchorMax = new Vector2(1f, 1f);
        fontLabelRect.pivot = new Vector2(0.5f, 1f);
        fontLabelRect.offsetMin = new Vector2(22f, -98f);
        fontLabelRect.offsetMax = new Vector2(-110f, -66f);
        fontLabel.alignment = TextAnchor.MiddleLeft;

        settingsFontValue = TextNode(
            settingsPanel,
            "FontSizeValue",
            "100%",
            HudTheme.ValueFontSize
        );
        var valueRect = settingsFontValue.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(1f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-22f, -66f);
        valueRect.sizeDelta = new Vector2(82f, 32f);
        settingsFontValue.alignment = TextAnchor.MiddleRight;

        settingsFontSlider = BuildSettingsSlider(settingsPanel);

        var modeLabel = TextNode(settingsPanel, "ModeLabel", "Appearance", HudTheme.BodyFontSize);
        var modeRect = modeLabel.GetComponent<RectTransform>();
        modeRect.anchorMin = new Vector2(0f, 1f);
        modeRect.anchorMax = new Vector2(1f, 1f);
        modeRect.pivot = new Vector2(0.5f, 1f);
        modeRect.offsetMin = new Vector2(22f, -166f);
        modeRect.offsetMax = new Vector2(-22f, -134f);
        modeLabel.alignment = TextAnchor.MiddleLeft;

        lightModeButton = BuildSettingsChoice(
            settingsPanel,
            "LightMode",
            "Light Mode",
            22f,
            6f,
            () => SetLightMode(true)
        );
        darkModeButton = BuildSettingsChoice(
            settingsPanel,
            "DarkMode",
            "Dark Mode",
            6f,
            22f,
            () => SetLightMode(false)
        );
        settingsPanel.SetActive(false);
    }

    private static Slider BuildSettingsSlider(GameObject parent)
    {
        var sliderObject = Node(parent, "FontSizeSlider");
        var rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -104f);
        rect.sizeDelta = new Vector2(-44f, 28f);

        var track = Node(sliderObject, "Track");
        var trackRect = track.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0f, 0.5f);
        trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.sizeDelta = new Vector2(-8f, HudTheme.SliderTrackHeight);
        track.AddComponent<Image>().color = HudTheme.ControlTrack;

        var handleArea = Node(sliderObject, "Handle Slide Area");
        Stretch(handleArea.GetComponent<RectTransform>());
        var handle = Node(handleArea, "Handle");
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(HudTheme.SliderHandleSize, HudTheme.SliderHandleSize);
        var handleImage = handle.AddComponent<Image>();
        handleImage.color = HudTheme.ControlHandle;

        var slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 80f;
        slider.maxValue = 140f;
        slider.wholeNumbers = true;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.value = Mathf.Round(HudTheme.FontScale * 100f);
        slider.colors = HudTheme.ButtonColors(HudTheme.ControlHandle, HudTheme.ControlHandle);
        SliderHook.Register(
            slider,
            value =>
            {
                float rounded = Mathf.Round(value / 5f) * 5f;
                if (Mathf.Abs(slider.value - rounded) > 0.01f)
                {
                    slider.SetValueWithoutNotify(rounded);
                }
                HudTheme.SetFontScale(rounded / 100f);
                HudStyler.ApplyFontScale(window);
                RefreshSettingsControls();
            }
        );
        return slider;
    }

    private static Button BuildSettingsChoice(
        GameObject parent,
        string name,
        string caption,
        float leftInset,
        float rightInset,
        Action click
    )
    {
        var obj = Node(parent, name);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(name == "LightMode" ? 0f : 0.5f, 1f);
        rect.anchorMax = new Vector2(name == "LightMode" ? 0.5f : 1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2((leftInset - rightInset) * 0.5f, -180f);
        rect.sizeDelta = new Vector2(-leftInset - rightInset - 6f, 48f);
        var image = obj.AddComponent<Image>();
        image.color = HudTheme.Surface;
        HudStyler.AddPrimaryBorder(obj);
        var button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = HudTheme.ActionButtonColors(HudTheme.Surface, HudTheme.Selection);
        ButtonHook.Register(button, click);
        var label = TextNode(obj, "Label", caption, HudTheme.ValueFontSize);
        Stretch(label.GetComponent<RectTransform>());
        label.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private static void ToggleSettings()
    {
        if (settingsPanel.IsNullOrDestroyed())
            return;
        bool show = !settingsPanel.activeSelf;
        settingsPanel.SetActive(show);
        if (show)
        {
            settingsPanel.transform.SetAsLastSibling();
            RefreshSettingsControls();
        }
    }

    private static void SetLightMode(bool enabled)
    {
        HudTheme.SetLightMode(window, enabled);
        HudStyler.NormalizeSelectableGraphics(window);
        if (activePage != null)
            SetSelected(activePage.Id);
        MonolithTimelineEditor.RefreshSelection();
        RefreshSettingsControls();
    }

    private static void RefreshSettingsControls()
    {
        if (!settingsFontSlider.IsNullOrDestroyed())
            settingsFontSlider.SetValueWithoutNotify(Mathf.Round(HudTheme.FontScale * 100f));
        if (!settingsFontValue.IsNullOrDestroyed())
            settingsFontValue.text = Mathf.RoundToInt(HudTheme.FontScale * 100f) + "%";
        StyleSettingsChoice(darkModeButton, !HudTheme.LightMode);
        StyleSettingsChoice(lightModeButton, HudTheme.LightMode);
    }

    private static void StyleSettingsChoice(Button button, bool selected)
    {
        if (button.IsNullOrDestroyed())
            return;
        Color normal = selected ? HudTheme.Selection : HudTheme.Surface;
        var image = button.GetComponent<Image>();
        if (!image.IsNullOrDestroyed())
            image.color = HudTheme.SelectableTint;
        button.colors = HudTheme.ActionButtonColors(
            normal,
            selected ? HudTheme.Selection : HudTheme.SurfaceHover
        );
    }

    private static void PlaceLegacyContent(GameObject content)
    {
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(HudTheme.SidebarWidth, 0f);
        contentRect.offsetMax = new Vector2(0f, -HudTheme.HeaderHeight);
        var background = content.GetComponent<Image>();
        if (!background.IsNullOrDestroyed())
            background.color = HudTheme.Background;
    }

    private static void BuildNavigation(GameObject menu)
    {
        var menuRect = menu.GetComponent<RectTransform>();
        menuRect.anchorMin = Vector2.zero;
        menuRect.anchorMax = new Vector2(0f, 1f);
        menuRect.pivot = new Vector2(0f, 0.5f);
        menuRect.offsetMin = Vector2.zero;
        menuRect.offsetMax = new Vector2(HudTheme.SidebarWidth, -HudTheme.HeaderHeight);

        var oldContent = Prefab.Child(menu, "Content");
        if (!oldContent.IsNullOrDestroyed())
            oldContent.SetActive(false);

        var background = menu.GetComponent<Image>();
        if (background.IsNullOrDestroyed())
            background = menu.AddComponent<Image>();
        background.color = HudTheme.Sidebar;

        var rightBorder = Node(menu, "LEHUD_RightBorder");
        var borderRect = rightBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(1f, 0f);
        borderRect.anchorMax = Vector2.one;
        borderRect.pivot = new Vector2(1f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(HudTheme.BorderWidth, 0f);
        var border = rightBorder.AddComponent<Image>();
        border.color = HudTheme.Border;
        border.raycastTarget = false;

        var list = Node(menu, "LEHUD_Navigation");
        var listRect = list.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 1f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.pivot = new Vector2(0.5f, 1f);
        listRect.anchoredPosition = Vector2.zero;
        listRect.sizeDelta = Vector2.zero;
        listRect.offsetMin = new Vector2(0f, 0f);
        listRect.offsetMax = new Vector2(0f, 0f);

        var layout = list.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = list.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        foreach (var definition in HudNavigation.Sections)
        {
            var state = new NavigationSection { Definition = definition };
            navigationSections.Add(state);
            var sectionButton = CreateNavigationButton(
                list,
                "Section_" + definition.Id,
                definition.Label,
                HudTheme.SectionHeight,
                false,
                out var indicator,
                out _
            );
            state.Indicator = indicator;
            if (definition.Accordion)
                Prefab.BindButton(sectionButton, new Action(() => ToggleSection(state)));
            else
            {
                Prefab.BindButton(
                    sectionButton,
                    new Action(() => ActivateNavigationOnly(definition.Pages[0]))
                );
                pageButtons[definition.Pages[0].Id] = new NavigationButton
                {
                    Button = sectionButton,
                    Background = sectionButton.GetComponent<Image>(),
                };
            }

            foreach (var page in definition.Pages)
            {
                if (!definition.Accordion)
                    continue;
                var pageButton = CreateNavigationButton(
                    list,
                    "Page_" + page.Id,
                    page.Label,
                    HudTheme.PageHeight,
                    true,
                    out _,
                    out var accent
                );
                Prefab.BindButton(pageButton, new Action(() => ActivateNavigationOnly(page)));
                state.PageRows.Add(pageButton.gameObject);
                pageButtons[page.Id] = new NavigationButton
                {
                    Button = pageButton,
                    Background = Prefab
                        .Child(pageButton.gameObject, "ButtonSurface")
                        .GetComponent<Image>(),
                    Accent = accent,
                };
                pageButton.gameObject.SetActive(false);
            }
        }
        // Navigation rows span the full sidebar and otherwise cover this edge.
        rightBorder.transform.SetAsLastSibling();
    }

    private static Button CreateNavigationButton(
        GameObject parent,
        string name,
        string caption,
        float height,
        bool child,
        out Text indicator,
        out GameObject accent
    )
    {
        var row = Node(parent, name);
        var image = row.AddComponent<Image>();
        image.color = HudTheme.Surface;
        var button = row.AddComponent<Button>();
        var element = row.AddComponent<LayoutElement>();
        element.minHeight = height;
        element.preferredHeight = height;

        Image buttonSurface = image;
        if (child)
        {
            var surfaceObject = Node(row, "ButtonSurface");
            var surfaceRect = surfaceObject.GetComponent<RectTransform>();
            surfaceRect.anchorMin = Vector2.zero;
            surfaceRect.anchorMax = Vector2.one;
            surfaceRect.offsetMin = new Vector2(HudTheme.NavigationIndent - 10f, 0f);
            surfaceRect.offsetMax = Vector2.zero;
            buttonSurface = surfaceObject.AddComponent<Image>();
            buttonSurface.color = HudTheme.Surface;
        }
        button.targetGraphic = buttonSurface;
        button.colors = HudTheme.ButtonColors(HudTheme.Surface, HudTheme.SurfaceHover);

        var label = TextNode(
            row,
            "Label",
            caption,
            child ? HudTheme.PageFontSize : HudTheme.SectionFontSize
        );
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(child ? HudTheme.NavigationIndent : 18f, 0f);
        labelRect.offsetMax = new Vector2(-46f, 0f);
        label.alignment = TextAnchor.MiddleLeft;
        label.color = child ? HudTheme.TextPrimary : HudTheme.TextPrimary;

        indicator = null;
        if (!child)
        {
            indicator = TextNode(row, "Indicator", string.Empty, HudTheme.SectionFontSize);
            var indicatorRect = indicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(1f, 0f);
            indicatorRect.anchorMax = Vector2.one;
            indicatorRect.pivot = new Vector2(1f, 0.5f);
            indicatorRect.offsetMin = new Vector2(-44f, 0f);
            indicatorRect.offsetMax = new Vector2(-14f, 0f);
            indicator.alignment = TextAnchor.MiddleCenter;
            indicator.color = HudTheme.TextPrimary;
        }

        accent = null;
        if (child)
        {
            var gutter = Node(row, "SubmenuGutter");
            var gutterRect = gutter.GetComponent<RectTransform>();
            gutterRect.anchorMin = Vector2.zero;
            gutterRect.anchorMax = new Vector2(0f, 1f);
            gutterRect.pivot = new Vector2(0f, 0.5f);
            gutterRect.sizeDelta = new Vector2(HudTheme.BorderWidth, 0f);
            gutterRect.anchoredPosition = new Vector2(HudTheme.NavigationIndent - 12f, 0f);
            var gutterImage = gutter.AddComponent<Image>();
            gutterImage.color = HudTheme.Border;
            gutterImage.raycastTarget = false;

            accent = Node(row, "SelectedAccent");
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = Vector2.zero;
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(4f, 0f);
            accentRect.anchoredPosition = new Vector2(HudTheme.NavigationIndent - 12f, 0f);
            var accentImage = accent.AddComponent<Image>();
            accentImage.color = HudTheme.Accent;
            accentImage.raycastTarget = false;
            accent.SetActive(false);
        }

        var divider = Node(row, "Divider");
        var dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = Vector2.zero;
        dividerRect.anchorMax = new Vector2(1f, 0f);
        dividerRect.pivot = new Vector2(0.5f, 0f);
        dividerRect.offsetMin = new Vector2(child ? HudTheme.NavigationIndent - 10f : 0f, 0f);
        dividerRect.offsetMax = Vector2.zero;
        dividerRect.sizeDelta = new Vector2(dividerRect.sizeDelta.x, HudTheme.BorderWidth);
        var dividerImage = divider.AddComponent<Image>();
        dividerImage.color = child ? HudTheme.Border : HudTheme.AccentMuted;
        dividerImage.raycastTarget = false;
        return button;
    }

    private static void ExpandSection(string id, bool expanded)
    {
        foreach (var section in navigationSections)
        {
            if (!section.Definition.Accordion)
                continue;
            bool show = section.Definition.Id == id && expanded;
            if (!expanded && section.Definition.Id != id)
                show = section.Expanded;
            section.Expanded = show;
            if (!section.Indicator.IsNullOrDestroyed())
                section.Indicator.text = show ? "-" : "+";
            foreach (var row in section.PageRows)
                if (!row.IsNullOrDestroyed())
                    row.SetActive(show);
        }
    }

    private static void ToggleSection(NavigationSection section)
    {
        if (section == null || !section.Definition.Accordion)
            return;
        bool expand = !section.Expanded;
        ExpandSection(section.Definition.Id, expand);
        if (expand && section.Definition.Pages.Length > 0)
            ActivateNavigationOnly(section.Definition.Pages[0]);
    }

    private static void ActivateNavigationOnly(HudPageDefinition page, bool preserveSearch = false)
    {
        if (page == null)
            return;
        if (!preserveSearch)
        {
            HudSearch.ClearAll();
            HudSearchBar.Clear();
        }
        foreach (var candidate in HudNavigation.Pages)
            candidate.Hide();
        HideLegacyContent();
        page.Show();
        SetSelected(page.Id);
        activePage = page;
    }

    private static void ActivateSearchMatch(HudSearchMatch match, string query)
    {
        if (
            match == null
            || string.IsNullOrWhiteSpace(query)
            || !HudNavigation.TryGetPage(match.PageId, out var section, out var page)
        )
            return;
        HudSearch.ClearAll();
        ExpandSection(section.Id, true);
        ActivateNavigationOnly(page, true);
        match.Page.ApplySearch(query);
    }

    public static void RefreshActivePage()
    {
        activePage?.Refresh();
    }

    private static void HideLegacyContent()
    {
        Hud_Manager.Content.Character.Set_Active(false);
        Hud_Manager.Content.Items.Set_Active(false);
        Hud_Manager.Content.Scenes.Set_Active(false);
        Hud_Manager.Content.Skills.Set_Active(false);
        Hud_Manager.Content.OdlForceDrop.Set_Active(false);
        Hud_Manager.Content.Headhunter.Set_Active(false);
        Hud_Manager.Content.Set_Active();
    }

    private static void SetSelected(string pageId)
    {
        foreach (var pair in pageButtons)
        {
            bool selected = pair.Key == pageId;
            pair.Value.Background.color = HudTheme.SelectableTint;
            pair.Value.Button.colors = HudTheme.ButtonColors(
                selected ? HudTheme.Selection : HudTheme.Surface,
                selected ? HudTheme.Selection : HudTheme.SurfaceHover
            );
            if (!pair.Value.Accent.IsNullOrDestroyed())
                pair.Value.Accent.SetActive(selected);
        }
    }

    private static void Close()
    {
        if (Hud_Manager.mod_menu_open)
            Hud_Manager.mod_menu_open = false;
        else
            Hud_Manager.Hud_Base.Resume_Click();
    }

    private static Font FindFont(GameObject hud)
    {
        foreach (var text in hud.GetComponentsInChildren<Text>(true))
            if (!text.font.IsNullOrDestroyed())
                return text.font;
        return null;
    }

    private static GameObject Node(GameObject parent, string name) =>
        HudElements.Node(parent, name);

    private static Text TextNode(GameObject parent, string name, string caption, int size) =>
        HudElements.Text(parent, name, caption, font, size);

    private static void Stretch(RectTransform rect) => HudElements.Stretch(rect);
}
