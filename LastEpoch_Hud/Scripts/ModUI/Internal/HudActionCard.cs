using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

// Shared renderer for single-card pages made from full-width action buttons.
internal sealed class HudActionCard
{
    internal sealed class Definition
    {
        public string Id;
        public string Label;
        public Action Click;
    }

    private readonly GameObject root;
    private readonly Font font;

    private HudActionCard(
        GameObject parent,
        Font inheritedFont,
        string name,
        string title,
        IReadOnlyList<Definition> definitions
    )
    {
        font = inheritedFont;
        root = Node(parent, name);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = new Vector2(
            HudTheme.SidebarWidth + HudTheme.ContentPadding,
            HudTheme.ContentPadding
        );
        rootRect.offsetMax = new Vector2(
            -HudTheme.ContentPadding,
            -HudTheme.HeaderHeight - HudTheme.ContentPadding
        );

        var card = Node(root, "Card");
        Stretch(card.GetComponent<RectTransform>());
        card.GetComponent<RectTransform>().offsetMin = new Vector2(
            HudTheme.CardHorizontalInset,
            0f
        );
        card.GetComponent<RectTransform>().offsetMax = new Vector2(
            -HudTheme.CardHorizontalInset,
            0f
        );
        var cardImage = card.AddComponent<Image>();
        cardImage.color = HudTheme.Surface;
        var cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = HudTheme.Border;
        cardOutline.effectDistance = new Vector2(HudTheme.BorderWidth, -HudTheme.BorderWidth);
        cardOutline.useGraphicAlpha = false;

        var titleText = TextNode(card, "Title", title, HudTheme.SliderCardTitleFontSize);
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(HudTheme.SliderCardPadding, -76f);
        titleRect.offsetMax = new Vector2(-HudTheme.SliderCardPadding, -20f);
        titleText.alignment = TextAnchor.MiddleLeft;

        var divider = Node(card, "TitleDivider");
        var dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0f, 1f);
        dividerRect.anchorMax = new Vector2(1f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 1f);
        dividerRect.anchoredPosition = new Vector2(0f, -78f);
        dividerRect.sizeDelta = new Vector2(-HudTheme.SliderCardPadding * 2f, HudTheme.BorderWidth);
        var dividerImage = divider.AddComponent<Image>();
        dividerImage.color = HudTheme.CardDivider;
        dividerImage.raycastTarget = false;

        var body = Node(card, "Actions");
        var bodyRect = body.GetComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(HudTheme.SliderCardPadding, HudTheme.SliderCardPadding);
        bodyRect.offsetMax = new Vector2(-HudTheme.SliderCardPadding, -100f);
        var layout = body.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = HudTheme.ActionButtonGap;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        foreach (var definition in definitions)
            AddButton(body, definition);

        root.SetActive(false);
    }

    public static HudActionCard Build(
        GameObject parent,
        Font font,
        string name,
        string title,
        IReadOnlyList<Definition> definitions
    )
    {
        if (parent.IsNullOrDestroyed())
            return null;
        return new HudActionCard(parent, font, name, title, definitions);
    }

    public void Show()
    {
        if (!root.IsNullOrDestroyed())
            root.SetActive(true);
    }

    public void Hide()
    {
        if (!root.IsNullOrDestroyed())
            root.SetActive(false);
    }

    private void AddButton(GameObject parent, Definition definition)
    {
        var buttonObject = Node(parent, "Button_" + definition.Id);
        var element = buttonObject.AddComponent<LayoutElement>();
        element.minHeight = HudTheme.ActionButtonHeight;
        element.preferredHeight = HudTheme.ActionButtonHeight;
        element.flexibleHeight = 0f;

        var image = buttonObject.AddComponent<Image>();
        image.color = HudTheme.Surface;
        HudStyler.AddPrimaryBorder(buttonObject);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = HudTheme.ActionButtonColors(HudTheme.Surface, HudTheme.Selection);
        Prefab.BindButton(button, definition.Click);

        var label = TextNode(buttonObject, "Label", definition.Label, HudTheme.BodyFontSize);
        Stretch(label.GetComponent<RectTransform>());
        label.alignment = TextAnchor.MiddleCenter;
        label.color = HudTheme.TextPrimary;
    }

    private GameObject Node(GameObject parent, string name)
    {
        var node = new GameObject(name);
        node.layer = parent.layer;
        node.AddComponent<RectTransform>().SetParent(parent.transform, false);
        return node;
    }

    private Text TextNode(GameObject parent, string name, string caption, int size)
    {
        var node = Node(parent, name);
        var text = node.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = FontStyle.Normal;
        text.color = HudTheme.TextPrimary;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        LocaleRegistry.Apply(text, caption);
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
