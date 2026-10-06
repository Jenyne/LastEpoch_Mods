using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Character;

[RegisterTypeInIl2Cpp]
public class Character_TpSafe : MonoBehaviour
{
    //You have to unlock Portal first to be able to use this

    bool mod_enable = false; //Disabled: Ctrl+Q conflicts with skill/AutoCast bindings.
    KeyCode key_0 = KeyCode.LeftControl; //Left Ctrl
    KeyCode key_1 = KeyCode.Q; //Q
    string tp_waypoint = "EoT"; //Monolith Waypoint

    public static Character_TpSafe instance { get; private set; }

    public Character_TpSafe(System.IntPtr ptr)
        : base(ptr) { }

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (CanRun())
        {
            if (Input.GetKey(key_0) && Input.GetKeyDown(key_1))
            {
                TpSafe();
            }
        }
    }

    bool CanRun()
    {
        if (
            (Scenes.IsGameScene())
            && (!Save_Manager.instance.IsNullOrDestroyed())
            && (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            && (mod_enable)
        )
        {
            if (!Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    void TpSafe()
    {
        bool backup_godmode = Save_Manager.instance.data.Character.Cheats.Enable_GodMode;
        try
        {
            Save_Manager.instance.data.Character.Cheats.Enable_GodMode = true;
            // Use the current transition service; opening the map with omitted
            // nullable arguments fails in the generated IL2CPP wrapper.
            LastEpoch_Hud.Scripts.Mods.Teleport.Teleport_ToScene.StartTpToScene(tp_waypoint);
        }
        catch (System.Exception ex)
        {
            Main.logger_instance?.Error("Safe teleport failed: " + ex.Message);
        }
        finally
        {
            if (!Save_Manager.instance.IsNullOrDestroyed())
            {
                Save_Manager.instance.data.Character.Cheats.Enable_GodMode = backup_godmode;
            }
        }
    }
}
