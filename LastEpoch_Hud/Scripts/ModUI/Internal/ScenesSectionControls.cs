using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class ScenesSectionControls
{
    public static void Bind(GameObject content)
    {
        var center = Prefab.Child(content, "Center");
        var dungeonPanel = Prefab.Child(center, "Scenes_Dungeons_Content");
        var minimapPanel = Prefab.Child(center, "Scenes_Minimap_Content");
        var dungeons = Prefab.ViewportContent(content, "Center", "Scenes_Dungeons_Content");
        var minimap = Prefab.ViewportContent(content, "Center", "Scenes_Minimap_Content");
        if (
            center.IsNullOrDestroyed()
            || dungeons.IsNullOrDestroyed()
            || minimap.IsNullOrDestroyed()
        )
            return;
        var dungeonTitle = Prefab.Child(center, "Title");
        var minimapTitle = Prefab.Child(center, "Title (1)");
        if (dungeonTitle.IsNullOrDestroyed() || minimapTitle.IsNullOrDestroyed())
            return;
        var misc = Prefab.Child(center, "Scenes_Misc_Content");
        if (misc.IsNullOrDestroyed())
        {
            misc = new GameObject("Scenes_Misc_Content");
            misc.layer = center.layer;
            misc.AddComponent<RectTransform>().SetParent(center.transform, false);
            var source = minimapPanel.GetComponent<Image>();
            if (!source.IsNullOrDestroyed())
            {
                var image = misc.AddComponent<Image>();
                image.sprite = source.sprite;
                image.type = source.type;
                image.color = source.color;
                image.raycastTarget = false;
            }
        }
        var miscViewport = Prefab.Child(misc, "Viewport");
        if (miscViewport.IsNullOrDestroyed())
            miscViewport = Node(misc, "Viewport");
        Stretch(miscViewport);
        var miscContent = Prefab.Child(miscViewport, "Content");
        if (miscContent.IsNullOrDestroyed())
            miscContent = Node(miscViewport, "Content");
        Place(miscContent, 0, 88);
        var miscTitle = Prefab.Child(center, "MiscTitle");
        if (miscTitle.IsNullOrDestroyed())
        {
            miscTitle = Object.Instantiate(dungeonTitle, center.transform, false);
            miscTitle.name = "MiscTitle";
        }
        Title(dungeonTitle, "Dungeons");
        Title(miscTitle, "Misc");
        Title(minimapTitle, "Minimap");
        // Fit the center column to its actual controls instead of filling the screen.
        const float header = 28;
        const float dungeonHeight = 108;
        const float miscHeight = 104;
        const float minimapHeight = 52;
        const float gap = 8;
        const float total = 3 * header + dungeonHeight + miscHeight + minimapHeight + 2 * gap + 8;
        var centerRect = center.GetComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(centerRect.anchorMin.x, centerRect.anchorMax.y);
        centerRect.pivot = new Vector2(.5f, 1);
        centerRect.anchoredPosition = Vector2.zero;
        centerRect.sizeDelta = new Vector2(0, total);
        float top = 4;
        Place(dungeonTitle, top, header);
        Place(dungeonPanel, top += header, dungeonHeight);
        Place(miscTitle, top += dungeonHeight + gap, header);
        Place(misc, top += header, miscHeight);
        Place(minimapTitle, top += miscHeight + gap, header);
        Place(minimapPanel, top + header, minimapHeight);
        FitScrollPanel(dungeonPanel);
        FitScrollPanel(minimapPanel);
        DungeonRevealControls.Bind(content, dungeons);
        // Existing key paths now resolve through the Misc viewport. Values/save keys stay the same.
        var safe = Prefab.Child(minimap, "SafeTeleport");
        if (!safe.IsNullOrDestroyed())
        {
            safe.transform.SetParent(miscContent.transform, false);
            Place(safe, 0, miscHeight);
            ModSettings.SafeTeleport.Group.ResolveAndBind(content);
        }
        else
            SafeTeleportControls.Bind(content, miscContent);
    }

    static GameObject Node(GameObject parent, string name)
    {
        var node = new GameObject(name);
        node.layer = parent.layer;
        node.AddComponent<RectTransform>().SetParent(parent.transform, false);
        return node;
    }

    static void Stretch(GameObject node)
    {
        var rect = node.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static void FitScrollPanel(GameObject panel)
    {
        var scroll = panel.GetComponent<ScrollRect>();
        if (scroll.IsNullOrDestroyed())
            return;
        scroll.horizontal = false;
        scroll.vertical = false;
        if (!scroll.horizontalScrollbar.IsNullOrDestroyed())
            scroll.horizontalScrollbar.gameObject.SetActive(false);
        if (!scroll.verticalScrollbar.IsNullOrDestroyed())
            scroll.verticalScrollbar.gameObject.SetActive(false);
        scroll.horizontalScrollbar = null;
        scroll.verticalScrollbar = null;
        if (!scroll.viewport.IsNullOrDestroyed())
            Stretch(scroll.viewport.gameObject);
        if (!scroll.content.IsNullOrDestroyed())
        {
            var rect = scroll.content;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        }
    }

    static void Place(GameObject node, float top, float height)
    {
        var rect = node.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.007f, 1);
        rect.anchorMax = new Vector2(.993f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, -top);
        rect.sizeDelta = new Vector2(0, height);
    }

    static void Title(GameObject node, string caption)
    {
        foreach (var label in node.GetComponentsInChildren<Text>(true))
        {
            label.fontSize = 16;
            LocaleRegistry.Apply(label, caption);
        }
        foreach (var label in node.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(true))
        {
            label.fontSize = 16;
            LocaleRegistry.Apply(label, caption);
        }
    }
}
