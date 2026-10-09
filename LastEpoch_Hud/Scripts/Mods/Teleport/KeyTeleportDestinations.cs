using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;
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
    static readonly string[] scenes = new string[Labels.Length];
    static string lastMapping;

    public static void Refresh()
    {
        Array.Clear(scenes, 0, scenes.Length);
        try
        {
            // Only real map waypoints qualify. Dungeon entrance scenes must use
            // their own UIWaypoint handler, never generic/additive scene travel.
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
            var priorities = new int[Labels.Length];
            Array.Fill(priorities, int.MaxValue);
            foreach (var pin in pins)
            {
                if (
                    pin.IsNullOrDestroyed()
                    || !TravelMapWaypoints.IsNativeWaypoint(pin)
                    || string.IsNullOrWhiteSpace(pin.sceneName)
                )
                    continue;
                string scene = pin.sceneName;
                names.TryGetValue(scene, out var localized);
                for (int i = 0; i < candidates.Length; i++)
                {
                    int priority = KeyTeleportTargetRules.MatchPriority(i, scene, localized);
                    if (priority < 0 || priority > priorities[i])
                        continue;
                    if (priority < priorities[i])
                    {
                        candidates[i].Clear();
                        priorities[i] = priority;
                    }
                    candidates[i].Add(scene);
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
                "Waypoint unavailable. Open the world map, then refresh key teleports."
            );
            return;
        }
        if (KeyTeleportTargetRules.SavedDungeonPreset(scene) >= 0)
        {
            DungeonWaypointTravel.Travel(scene);
            return;
        }
        // Ordinary hubs retain the existing unlocked waypoint route.
        FavouriteTeleports.TravelWaypoint(scene);
    }
}
