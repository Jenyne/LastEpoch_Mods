using System;
using System.Collections.Generic;
using System.Text;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Teleport;

internal static class KeyTeleportDestinations
{
    public static readonly string[] Labels =
    {
        "End of Time",
        "Temporal Sanctum",
        "Lightless Arbor",
        "Soulfire Bastion",
        "The Bazaar",
        "The Observatory",
    };
    static readonly string[][] aliases =
    {
        new[] { "eot", "endoftime" },
        new[] { "temporalsanctum" },
        new[] { "lightlessarbor" },
        new[] { "soulfirebastion" },
        new[] { "bazaar" },
        new[] { "observatory" },
    };
    static readonly string[] scenes = new string[Labels.Length];
    static string lastMapping;

    static string Normalize(string name)
    {
        var result = new StringBuilder();
        foreach (char c in name ?? "")
            if (char.IsLetterOrDigit(c))
                result.Append(char.ToLowerInvariant(c));
        return result.ToString();
    }

    public static void Refresh()
    {
        Array.Clear(scenes, 0, scenes.Length);
        try
        {
            // Resolve actual game identifiers from real map waypoints rather than invented
            // scene IDs or a direct jump into generated dungeon rooms.
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            var list = Refs_Manager.scene_list;
            if (list.IsNullOrDestroyed())
                list = SceneList.instance;
            var nativeScenes = list.IsNullOrDestroyed() ? null : list.scenes;
            var collection =
                nativeScenes?.TryCast<Il2CppSystem.Collections.Generic.ICollection<SceneDetails>>();
            if (collection != null)
                for (int i = 0; i < collection.Count; i++)
                {
                    var details = nativeScenes[i];
                    if (!details.IsNullOrDestroyed() && !string.IsNullOrEmpty(details.Name))
                        names[details.Name] = details.LocalizedName;
                }
            var pins = Resources.FindObjectsOfTypeAll<UIWaypoint>();
            if (pins == null)
                return;
            var candidates = new HashSet<string>[Labels.Length];
            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pin in pins)
            {
                if (
                    pin.IsNullOrDestroyed()
                    || pin.noWaypointInScene
                    || string.IsNullOrWhiteSpace(pin.sceneName)
                )
                    continue;
                string scene = pin.sceneName;
                string normalized = Normalize(scene);
                if (normalized.Contains("pcg") || normalized.Contains("arena"))
                    continue;
                names.TryGetValue(scene, out var localized);
                string label = Normalize(localized);
                for (int i = 0; i < aliases.Length; i++)
                    foreach (string alias in aliases[i])
                        if (
                            normalized == alias
                            || (alias != "eot" && normalized.Contains(alias))
                            || label == alias
                            || label == "the" + alias
                        )
                        {
                            candidates[i].Add(scene);
                            break;
                        }
            }
            for (int i = 0; i < candidates.Length; i++)
                if (candidates[i].Count == 1)
                    foreach (string scene in candidates[i])
                        scenes[i] = scene;
            string mapping = string.Join(" | ", scenes);
            if (mapping != lastMapping)
            {
                lastMapping = mapping;
                Main.logger_instance?.Msg("[KeyTeleports] " + mapping);
            }
        }
        catch (Exception ex)
        {
            Array.Clear(scenes, 0, scenes.Length);
            ErrorLog.Report(ex, "Key teleport destinations");
        }
    }

    public static void Travel(int index)
    {
        Refresh();
        string scene = index >= 0 && index < scenes.Length ? scenes[index] : null;
        if (scene == null)
        {
            FavouriteTeleports.SetStatus(
                "Destination unavailable. Open the world map, then refresh key teleports."
            );
            return;
        }
        // Always use the real unlocked waypoint route for presets, including in build 11.
        FavouriteTeleports.TravelWaypoint(scene);
    }
}
