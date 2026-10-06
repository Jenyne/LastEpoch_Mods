using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class IdolRerollControls
{
    const string FreeName = "FreeAmber";
    const string UnlimitedName = "UnlimitedUses";

    public static void Bind(GameObject viewport, GameObject styleSource)
    {
        if (viewport.IsNullOrDestroyed() || Prefab.Child(viewport, "IdolRerollOptions") != null)
            return;
        var sample = styleSource.GetComponentInChildren<Toggle>(true);
        if (sample.IsNullOrDestroyed())
            return;
        var sourceText = sample.GetComponentInChildren<Text>(true);
        if (sourceText.IsNullOrDestroyed())
            return;
        var section = Node(viewport, "IdolRerollOptions");
        var rect = section.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, -104);
        rect.sizeDelta = new Vector2(0, 80);
        var title = Label(section, "Title", sourceText, "Idol Rerolling");
        Place(title.gameObject, 0, 24);
        AddToggle(
            section,
            sample,
            sourceText,
            FreeName,
            "No Memory Amber Cost",
            ModSettings.IdolReroll.FreeMemoryAmber,
            28
        );
        AddToggle(
            section,
            sample,
            sourceText,
            UnlimitedName,
            "Unlimited Idol Altar Uses",
            ModSettings.IdolReroll.UnlimitedUses,
            54
        );
        Main.logger_instance?.Msg("Idol reroll controls created in Scenes > Misc.");
    }

    static void AddToggle(
        GameObject parent,
        Toggle source,
        Text font,
        string name,
        string label,
        BoolSetting setting,
        float top
    )
    {
        var row = Node(parent, name);
        Place(row, top, 24);
        var toggle = row.AddComponent<Toggle>();
        var box = Node(row, "Box").AddComponent<Image>();
        var rect = box.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, .5f);
        rect.pivot = new Vector2(0, .5f);
        rect.anchoredPosition = new Vector2(8, 0);
        rect.sizeDelta = new Vector2(18, 18);
        var check = Node(box.gameObject, "Check").AddComponent<Image>();
        CopyImage(source.targetGraphic, box);
        CopyImage(source.graphic, check);
        toggle.targetGraphic = box;
        toggle.graphic = check;
        toggle.colors = source.colors;
        toggle.transition = source.transition;
        toggle.SetIsOnWithoutNotify(setting.Value);
        var caption = Label(row, "Label", font, label);
        caption.GetComponent<RectTransform>().offsetMin = new Vector2(29, 0);
        var divider = Node(row, "Separator").AddComponent<Image>();
        var line = divider.GetComponent<RectTransform>();
        line.anchorMin = new Vector2(0, 0);
        line.anchorMax = new Vector2(1, 0);
        line.sizeDelta = new Vector2(0, 1);
        divider.color = new Color(.83f, .69f, .36f);
        divider.raycastTarget = false;
    }

    static void CopyImage(Graphic source, Image target)
    {
        var image = source.IsNullOrDestroyed() ? null : source.GetComponent<Image>();
        if (image.IsNullOrDestroyed())
            return;
        target.sprite = image.sprite;
        target.type = image.type;
        target.color = image.color;
    }

    static void Save(Toggle toggle)
    {
        if (
            toggle.IsNullOrDestroyed()
            || toggle.transform.parent.IsNullOrDestroyed()
            || toggle.transform.parent.name != "IdolRerollOptions"
        )
            return;
        if (toggle.name == FreeName)
            ModSettings.IdolReroll.FreeMemoryAmber.Set(toggle.isOn);
        else if (toggle.name == UnlimitedName)
            ModSettings.IdolReroll.UnlimitedUses.Set(toggle.isOn);
    }

    [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
    static class Click
    {
        [HarmonyPostfix]
        static void Postfix(Toggle __instance) => Save(__instance);
    }

    [HarmonyPatch(typeof(Toggle), "OnSubmit")]
    static class Submit
    {
        [HarmonyPostfix]
        static void Postfix(Toggle __instance) => Save(__instance);
    }

    static GameObject Node(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return go;
    }

    static void Place(GameObject go, float top, float height)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.01f, 1);
        rect.anchorMax = new Vector2(.99f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, -top);
        rect.sizeDelta = new Vector2(0, height);
    }

    static Text Label(GameObject parent, string name, Text source, string label)
    {
        var text = Node(parent, name).AddComponent<Text>();
        text.font = source.font;
        text.fontSize = source.fontSize;
        text.fontStyle = source.fontStyle;
        text.color = source.color;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;
        Prefab.ApplyLabel(text, label);
        return text;
    }
}
