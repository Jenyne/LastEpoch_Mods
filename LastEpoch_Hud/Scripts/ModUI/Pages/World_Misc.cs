using System;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class World_Misc
{
    private static HudFormPage page;

    public static void Build(GameObject parent, GameObject hud, Font font)
    {
        page = HudFormPage.Build(parent, hud, font, "World_Misc");
        if (page == null)
            return;

        var dungeons = page.AddCard("Dungeons", "Dungeons");
        page.AddToggle(
            dungeons,
            "RevealObjectives",
            "Reveal Dungeon Objectives",
            () => ModSettings.DungeonReveal.Enabled.Value,
            ModSettings.DungeonReveal.Enabled.Set
        );
        AddToggle(
            dungeons,
            "EnterWithoutKey",
            "Enter Without Key",
            () => Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey,
            v => Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey = v
        );

        var misc = page.AddCard("Misc", "Misc");
        page.AddToggle(
            misc,
            "SafeTeleport",
            "Enable Safe Teleport",
            () => ModSettings.SafeTeleport.Enabled.Value,
            ModSettings.SafeTeleport.Enabled.Set
        );
        page.AddKeybind(misc, "SafeTeleportKey", "Teleport Key", ModSettings.SafeTeleport.Key);
        page.AddText(
            misc,
            "SafeTeleportNote",
            "End of Time waypoint required. Bind a key or modifier + key."
        );

        page.AddToggle(
            misc,
            "TravelAnywhere",
            "Enable Travel Anywhere",
            () => ModSettings.TravelAnywhere.Enabled.Value,
            value => ModSettings.TravelAnywhere.Enabled.Set(value)
        );

        var minimap = page.AddCard("Minimap", "Minimap");
        AddToggle(
            minimap,
            "MaxZoomOut",
            "Max Zoom Out",
            () => Save_Manager.instance.data.Scenes.Minimap.Enable_MaxZoomOut,
            v => Save_Manager.instance.data.Scenes.Minimap.Enable_MaxZoomOut = v
        );
        AddToggle(
            minimap,
            "RemoveFog",
            "Remove Fog Of War",
            () => Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar,
            v => Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar = v
        );
    }

    public static void Show() => page?.Show();

    public static void Hide() => page?.Hide();

    public static void Refresh() => page?.RefreshValues();

    private static void AddToggle(
        HudFormPage.Card card,
        string id,
        string label,
        Func<bool> read,
        Action<bool> write
    ) =>
        page.AddToggle(
            card,
            id,
            label,
            () => HasSave() && read(),
            v =>
            {
                if (HasSave())
                    write(v);
            }
        );

    private static bool HasSave() =>
        !Save_Manager.instance.IsNullOrDestroyed() && Save_Manager.instance.initialized;
}
