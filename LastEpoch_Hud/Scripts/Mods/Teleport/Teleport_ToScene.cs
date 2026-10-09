using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Teleport;

[RegisterTypeInIl2Cpp]
public class Teleport_ToScene : MonoBehaviour
{
    public static Teleport_ToScene instance { get; private set; }

    public Teleport_ToScene(System.IntPtr ptr)
        : base(ptr) { }

    void Awake()
    {
        instance = this;
    }

    public static void StartTpToScene(string scene_name)
    {
        if (instance.IsNullOrDestroyed())
        {
            Main.logger_instance?.Error("Teleport is not running");
            return;
        }
        instance.Begin(scene_name);
    }

    public static bool CanTravelToUnlockedWaypoint(string scene_name)
    {
        try
        {
            return Scenes.IsGameScene()
                && !string.IsNullOrEmpty(scene_name)
                && !TravelSceneRules.IsDungeonScene(scene_name)
                && !Refs_Manager.player_actor.IsNullOrDestroyed()
                && !Refs_Manager.player_data.IsNullOrDestroyed()
                && !Refs_Manager.player_data.UnlockedWaypointScenes.IsNullOrDestroyed()
                && Refs_Manager.player_data.UnlockedWaypointScenes.Contains(scene_name)
                && TryGetGate(scene_name, out _);
        }
        catch (System.Exception ex)
        {
            ErrorLog.Report(ex, "Favourite waypoint validation");
            return false;
        }
    }

    public static bool StartTpToUnlockedWaypoint(string scene_name)
    {
        if (instance.IsNullOrDestroyed() || !CanTravelToUnlockedWaypoint(scene_name))
            return false;
        return instance.Begin(scene_name, true);
    }

    static bool TryGetGate(string scene_name, out byte gate)
    {
        gate = 0;
        bool found = false;
        UIWaypoint[] pins = Resources.FindObjectsOfTypeAll<UIWaypoint>();
        if (pins == null)
        {
            return false;
        }
        foreach (UIWaypoint pin in pins)
        {
            if (
                (pin.IsNullOrDestroyed())
                || (!TravelMapWaypoints.IsNativeWaypoint(pin))
                || (pin.sceneName != scene_name)
            )
            {
                continue;
            }
            gate = pin.gate;
            found = true;
            if (pin.gate == 0)
            {
                break;
            }
        }
        return found;
    }

    bool Begin(string scene_name, bool requireUnlocked = false)
    {
        if (TravelSceneRules.IsDungeonScene(scene_name))
        {
            Main.logger_instance?.Warning(
                "Teleport blocked: dungeon scenes require their entrance flow."
            );
            return false;
        }
        if (TravelAnywhere.Busy)
        {
            Main.logger_instance?.Warning("Teleport blocked while Travel Anywhere finishes.");
            return false;
        }
        if (
            (string.IsNullOrEmpty(scene_name)) || (SceneManager.GetActiveScene().name == scene_name)
        )
        {
            return false;
        }
        byte gate = 0;
        TryGetGate(scene_name, out gate);
        try
        {
            if (
                !requireUnlocked
                && (!Refs_Manager.player_data.IsNullOrDestroyed())
                && (!Refs_Manager.player_data.UnlockedWaypointScenes.IsNullOrDestroyed())
                && (!Refs_Manager.player_data.UnlockedWaypointScenes.Contains(scene_name))
            )
            {
                Refs_Manager.player_data.UnlockedWaypointScenes.Add(scene_name);
            }
            Hud_Manager.Hud_Base.Resume_Click();
            if (
                (!Refs_Manager.game_uibase.IsNullOrDestroyed())
                && (Refs_Manager.game_uibase.IsWorldMapPanelOpen())
            )
            {
                Refs_Manager.game_uibase.closeMap();
            }
            if (!Il2CppLE.Networking.PlayerStore.IsLocalUserIdentityValid())
            {
                Main.logger_instance?.Error("Teleport player is missing");
                return false;
            }
            Il2Cpp.BaseTransitionService travel = Il2CppLE
                .Services
                .ServiceProvider
                .TransitionService;
            if (travel == null)
            {
                Main.logger_instance?.Error("Teleport service is missing");
                return false;
            }
            Main.logger_instance?.Msg(
                "Teleport -> "
                    + scene_name
                    + "; gate="
                    + gate
                    + "; from="
                    + SceneManager.GetActiveScene().name
            );
            travel.Waypoint(
                Il2CppLE.Networking.PlayerStore.LocalUserIdentity,
                Il2CppLE.Services.Models.TransitionFadeType.ToBlack,
                scene_name,
                gate
            );
            return true;
        }
        catch (System.Exception ex)
        {
            Main.logger_instance?.Error("Teleport failed: " + ex.Message);
            return false;
        }
    }
}
