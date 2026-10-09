using System;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Teleport;

internal static class DungeonWaypointTravel
{
    static UIWaypoint Find(string scene)
    {
        if (
            KeyTeleportTargetRules.SavedDungeonPreset(scene) < 0
            || !Scenes.IsGameScene()
            || TravelAnywhere.Busy
            || Refs_Manager.player_actor.IsNullOrDestroyed()
            || Refs_Manager.player_data.IsNullOrDestroyed()
            || Refs_Manager.player_data.UnlockedWaypointScenes.IsNullOrDestroyed()
            || !Refs_Manager.player_data.UnlockedWaypointScenes.Contains(scene)
        )
            return null;
        UIWaypoint chosen = null;
        foreach (var pin in Resources.FindObjectsOfTypeAll<UIWaypoint>())
        {
            if (
                pin.IsNullOrDestroyed()
                || pin.sceneName != scene
                || !TravelMapWaypoints.IsNativeWaypoint(pin)
            )
                continue;
            // Duplicate era widgets must agree on the destination gate.
            if (!chosen.IsNullOrDestroyed() && chosen.gate != pin.gate)
                return null;
            chosen = pin;
        }
        return chosen;
    }

    public static bool CanTravel(string scene)
    {
        try
        {
            return !Find(scene).IsNullOrDestroyed();
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Dungeon waypoint validation");
            return false;
        }
    }

    public static void Travel(string scene)
    {
        try
        {
            var pin = Find(scene);
            if (pin.IsNullOrDestroyed())
            {
                FavouriteTeleports.SetStatus(
                    "Dungeon waypoint unavailable or locked. Open the world map once if needed."
                );
                return;
            }
            if (scene == Scenes.SceneName)
            {
                FavouriteTeleports.SetStatus("You are already at this dungeon waypoint.");
                return;
            }
            Hud_Manager.Hud_Base.Resume_Click();
            Main.logger_instance?.Msg(
                "[KeyTeleports] Native dungeon waypoint -> " + scene + "; gate=" + pin.gate
            );
            // Delegate scene setup, gate selection and map closing to the game.
            // Travel Anywhere excludes these scenes and lets this handler through.
            pin.LoadWaypointScene();
            FavouriteTeleports.SetStatus("Dungeon waypoint travel requested.");
        }
        catch (Exception ex)
        {
            FavouriteTeleports.SetStatus("Dungeon waypoint travel failed. See the mod log.");
            ErrorLog.Report(ex, "Dungeon waypoint travel");
        }
    }
}
