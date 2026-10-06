using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class ScenesSectionControls
{
    public static void Bind(GameObject content)
    {
        var center = Prefab.Child(content, "Center");
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
            var source = minimap.GetComponent<Image>();
            if (!source.IsNullOrDestroyed())
            {
                var image = misc.AddComponent<Image>();
                image.sprite = source.sprite;
                image.type = source.type;
                image.color = source.color;
                image.raycastTarget = false;
            }
        }
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
        const float dungeonHeight = 88;
        const float miscHeight = 88;
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
        Place(dungeons, top += header, dungeonHeight);
        Place(miscTitle, top += dungeonHeight + gap, header);
        Place(misc, top += header, miscHeight);
        Place(minimapTitle, top += miscHeight + gap, header);
        Place(minimap, top + header, minimapHeight);
        DungeonRevealControls.Bind(content, dungeons);
        // Existing key paths now resolve through the Misc viewport. Values/save keys stay the same.
        var safe = Prefab.Child(minimap, "SafeTeleport");
        if (!safe.IsNullOrDestroyed())
        {
            safe.transform.SetParent(misc.transform, false);
            Place(safe, 0, miscHeight);
            ModSettings.SafeTeleport.Group.ResolveAndBind(content);
        }
        else
            SafeTeleportControls.Bind(content, misc);
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
