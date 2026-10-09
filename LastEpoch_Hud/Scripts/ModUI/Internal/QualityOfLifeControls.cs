using System;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class QualityOfLifeControls
{
    public static GameObject Node(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        go.AddComponent<RectTransform>().SetParent(parent.transform, false);
        return go;
    }

    public static GameObject Section(GameObject parent, string name, float height)
    {
        var section = Node(parent, name);
        Height(section, height);
        return section;
    }

    public static void Height(GameObject section, float height)
    {
        var layout = section.GetComponent<LayoutElement>();
        if (layout.IsNullOrDestroyed())
            layout = section.AddComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = height;
        layout.flexibleHeight = 0;
        section.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
    }

    public static void Place(GameObject node, float left, float right, float top, float height)
    {
        var rect = node.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(left, 1);
        rect.anchorMax = new Vector2(right, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, -top);
        rect.sizeDelta = new Vector2(0, height);
    }

    public static Text Label(
        GameObject parent,
        string name,
        Text sample,
        string caption,
        float top,
        float height
    )
    {
        var text = Node(parent, name).AddComponent<Text>();
        Place(text.gameObject, .03f, .97f, top, height);
        text.font = sample.font;
        text.fontSize = sample.fontSize;
        text.fontStyle = sample.fontStyle;
        text.color = sample.color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        LocaleRegistry.Apply(text, caption);
        return text;
    }

    public static Button Button(
        GameObject parent,
        string name,
        Text sample,
        string caption,
        float left,
        float right,
        float top,
        Action action
    )
    {
        var go = Node(parent, name);
        Place(go, left, right, top, 24);
        var image = go.AddComponent<Image>();
        image.color = new Color(.18f, .20f, .23f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        var label = Label(go, "Label", sample, caption, 0, 24);
        label.alignment = TextAnchor.MiddleCenter;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 10;
        label.resizeTextMaxSize = 14;
        Hud_Manager.Events.Set_Button_Event(button, action);
        return button;
    }
}
