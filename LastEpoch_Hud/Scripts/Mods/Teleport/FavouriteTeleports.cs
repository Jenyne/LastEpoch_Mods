using System;
using System.IO;
using System.Text.RegularExpressions;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Scripts.Mods.Teleport;

internal static class FavouriteTeleports
{
    static readonly string path = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Mods",
        Main.mod_name,
        "FavouriteTeleports.json"
    );
    static FavouriteDestinations destinations = new();
    static bool loaded;
    public static string Status { get; private set; } =
        "Choose a key teleport, or save up to 8 extra favourites.";
    public static System.Collections.Generic.IReadOnlyList<string> Scenes
    {
        get
        {
            Load();
            return destinations.Scenes;
        }
    }

    static void Load()
    {
        if (loaded)
            return;
        loaded = true;
        try
        {
            if (File.Exists(path))
                destinations = FavouriteDestinations.FromJson(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            Status = "Couldn't load favourites. See the mod log.";
            Main.logger_instance?.Warning("Load favourite teleports failed: " + ex.Message);
        }
    }

    public static string Caption(string scene) =>
        scene == "EoT"
            ? "End of Time"
            : Regex.Replace(scene.Replace('_', ' '), "([a-z])([A-Z])", "$1 $2");

    public static void SaveCurrent()
    {
        Load();
        string scene = LastEpoch_Hud.Scenes.SceneName;
        bool anywhere = ModUI.ModSettings.TravelAnywhere.Enabled.Value;
        if (anywhere)
            TravelDestinations.Refresh();
        if (
            anywhere
                ? TravelDestinations.Find(scene) == null
                : !Teleport_ToScene.CanTravelToUnlockedWaypoint(scene)
        )
        {
            Status = anywhere
                ? "This area is not available for direct travel."
                : "Visit an unlocked waypoint area. Open the world map once if needed.";
            return;
        }
        Change(
            candidate => candidate.Add(scene),
            "Saved " + Caption(scene) + ".",
            "Already saved, or all 8 slots are full."
        );
    }

    public static void Remove(string scene) =>
        Change(
            candidate => candidate.Remove(scene),
            "Removed " + Caption(scene) + ".",
            "Favourite no longer exists."
        );

    static void Change(Func<FavouriteDestinations, bool> action, string success, string rejected)
    {
        Load();
        try
        {
            var candidate = FavouriteDestinations.FromJson(destinations.ToJson());
            if (!action(candidate))
            {
                Status = rejected;
                return;
            }
            candidate.Save(path);
            destinations = candidate;
            Status = success;
        }
        catch (Exception ex)
        {
            Status = "Couldn't save favourites. See the mod log.";
            Main.logger_instance?.Error("Save favourite teleports failed: " + ex.Message);
        }
    }

    public static void SetStatus(string text) => Status = text;

    public static void Travel(string scene)
    {
        if (Teleport_ToScene.CanTravelToUnlockedWaypoint(scene))
        {
            TravelWaypoint(scene);
            return;
        }
        if (ModUI.ModSettings.TravelAnywhere.Enabled.Value)
        {
            TravelAnywhere.StartTravel(scene);
            Status = TravelAnywhere.Status;
            return;
        }
        TravelWaypoint(scene);
    }

    public static void TravelWaypoint(string scene)
    {
        if (!Teleport_ToScene.CanTravelToUnlockedWaypoint(scene))
        {
            Status =
                "Waypoint unavailable or locked for this character. Open the map once if needed.";
            return;
        }
        if (scene == LastEpoch_Hud.Scenes.SceneName)
        {
            Status = "You are already in " + Caption(scene) + ".";
            return;
        }
        Status = Teleport_ToScene.StartTpToUnlockedWaypoint(scene)
            ? "Travelling to " + Caption(scene) + "."
            : "Travel unavailable. See the mod log.";
    }
}
