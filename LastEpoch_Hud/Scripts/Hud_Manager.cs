using System.IO;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppSystem.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.ModUI;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts;

[RegisterTypeInIl2Cpp]
public partial class Hud_Manager : MonoBehaviour
{
    public Hud_Manager(System.IntPtr ptr)
        : base(ptr) { }

    public static AssetBundle asset_bundle;
    public static GameObject hud_object = null;

    private string asset_path = Application.dataPath + "/../Mods/" + Main.mod_name + "/Assets";
    private static Canvas game_canvas = null;
    public static GameObject game_pause_menu = null;
    private static Canvas hud_canvas = null;
    private readonly string asset_bundle_name = "lastepochmods"; //Name of asset file
    private bool hud_initializing = false;

    private readonly MenuInputLock _inputLock = new();
    private float nextPauseMenuProbe = 0f;
    private bool exit = false;
    public static readonly KeyCode MenuKey = KeyCode.F3;
    public static bool mod_menu_open = false;

    void Awake()
    {
        // Do not read AssetBundleCreateRequest.assetBundle before async loading
        // completes. A fresh process can hit this race while Unity is starting.
        string bundlePath = Path.Combine(asset_path, asset_bundle_name);
        asset_bundle = AssetBundle.LoadFromFile(bundlePath);
        if (asset_bundle.IsNullOrDestroyed())
        {
            Main.logger_instance?.Error("[Startup] HUD asset bundle unavailable: " + bundlePath);
            return;
        }
        Object.DontDestroyOnLoad(asset_bundle);
        Main.logger_instance?.Msg("[Startup] HUD asset bundle loaded.");
    }

    void Update()
    {
        if (!asset_bundle.IsNullOrDestroyed())
        {
            if (!Scenes.IsGameScene())
            {
                mod_menu_open = false;
            }
            else if (!hud_object.IsNullOrDestroyed())
            {
                if (Input.GetKeyDown(MenuKey))
                {
                    mod_menu_open = !mod_menu_open;
                }
                if (mod_menu_open && Input.GetKeyDown(KeyCode.Escape))
                {
                    mod_menu_open = false;
                }
            }

            Update_Hud_Scale();
            Update_Refs();
            Update_Locale();

            if (!hud_object.IsNullOrDestroyed())
            {
                Update_Menu(IsPauseOpen() || mod_menu_open);
            }
        }
    }

    private void Update_Menu(bool menuOpen)
    {
        try
        {
            Apply_InputLock(menuOpen);
            if (!menuOpen)
            {
                Hide_Menu();
                return;
            }
            Show_Menu();
        }
        catch (System.Exception ex)
        {
            ErrorLog.Report(ex, "Hud_Manager.Update");
        }
    }

    private void Apply_InputLock(bool menuOpen)
    {
        EpochInputManager manager = Refs_Manager.epoch_input_manager;
        if (manager.IsNullOrDestroyed())
        {
            return;
        }
        switch (_inputLock.Next(menuOpen, manager.isControllerActive, manager.forceDisableInput))
        {
            case InputLockAction.Take:
                manager.forceDisableInput = true;
                break;
            case InputLockAction.Release:
                manager.forceDisableInput = false;
                break;
        }
    }

    private void Refresh_Hud_Content()
    {
        try
        {
            Update_Hud_Content();
        }
        catch (System.Exception ex)
        {
            ErrorLog.Report(ex, "Hud_Manager.Update_Hud_Content");
        }
    }

    private void Show_Menu()
    {
        Refresh_Hud_Content();
        hud_object.active = true;
        ModUI.Settings.NumericSliderInputs.Tick(hud_object);
        if (Refs_Manager.epoch_input_manager.IsNullOrDestroyed())
        {
            return;
        }
        if (Refs_Manager.epoch_input_manager.isControllerActive)
        {
            return;
        }
        Update_Keyboard_Exit();
    }

    private void Update_Keyboard_Exit()
    {
        if (Hud_Base.Btn_Resume.IsNullOrDestroyed())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exit = true;
        }
        if ((Input.GetKeyUp(KeyCode.Escape)) && (exit))
        {
            Hud_Base.Btn_Resume.onClick.Invoke();
            exit = false;
        }
    }

    private void Hide_Menu()
    {
        if (hud_object.active)
        {
            hud_object.active = false;
        }
    }

    void Init_Hud()
    {
        hud_initializing = true;
        Main.logger_instance?.Msg("[Startup] HUD prefab initialization started.");
        if (Main.debug)
        {
            Main.logger_instance.Msg("Hud Manager : Load hud object in assets");
        }
        if (!asset_bundle.IsNullOrDestroyed())
        {
            string asset_name = "";
            foreach (string name in asset_bundle.GetAllAssetNames())
            {
                if (
                    (Functions.Check_Prefab(name))
                    && (name.Contains("/hud/"))
                    && (name.Contains("hud.prefab"))
                )
                {
                    if (Main.debug)
                    {
                        Main.logger_instance.Msg("Hud Manager : Dhud prefab found");
                    }
                    asset_name = name;
                    break;
                }
            }
            if (asset_name != "")
            {
                UnityEngine.Object obj = asset_bundle.LoadAsset(asset_name);
                GameObject prefab_object = obj.TryCast<GameObject>();
                if (!prefab_object.IsNullOrDestroyed())
                {
                    prefab_object.active = false; //Hide
                    prefab_object.AddComponent<UIMouseListener>(); //Block Mouse
                    prefab_object.AddComponent<WindowFocusManager>();

                    //Instantiate
                    if (Main.debug)
                    {
                        Main.logger_instance.Msg("Hud Manager : Instantiate hud prefab");
                    }
                    hud_object = Object.Instantiate(
                        prefab_object,
                        Vector3.zero,
                        Quaternion.identity
                    );
                    Object.DontDestroyOnLoad(hud_object);
                    RemoveUnusedButtons();

                    if (Main.debug)
                    {
                        Main.logger_instance.Msg("Hud Manager : Initialize hud refs");
                    }
                    SafeInit(
                        "ForceDropModel",
                        () =>
                        {
                            ModUI.ForceDrop.ForceDropModel.GetRefs(hud_object);
                            ModUI.ForceDrop.ForceDropModel.HideLegacyControls();
                            ModUI.ForceDrop.ForceDropModel.SetActive(false);
                        }
                    );
                    SafeInit(
                        "ModUI.SaveManager.BindHud",
                        () =>
                        {
                            ModUI.Settings.SaveManager.BindHud(hud_object);
                        }
                    );
                    Main.logger_instance?.Msg("[Startup] HUD binding completed.");
                    SafeInit(
                        "ModUI.HudLayout.Initialize",
                        () =>
                        {
                            ModUI.Shell.HudLayout.Initialize(hud_object);
                        }
                    );
                }
                else
                {
                    Main.logger_instance.Error("Hud Manager : Hud Prefab not found");
                }
            }
        }

        hud_initializing = false;
        Main.logger_instance?.Msg("[Startup] HUD prefab initialization finished.");
    }

    void SafeInit(string name, System.Action action)
    {
        try
        {
            action();
        }
        catch (System.Exception ex)
        {
            ErrorLog.Report(ex, name + " Init");
        }
    }

    static void RemoveUnusedButtons()
    {
        string[] names =
        {
            "Base",
            "Btn_Menu_Maxroll",
            "Btn_Menu_NewItems",
            "Maxroll_Content",
            "NewItems_Content",
        };
        foreach (string name in names)
        {
            GameObject button = Functions.FindDescendant(hud_object, name);
            if (!button.IsNullOrDestroyed())
            {
                UnityEngine.Object.Destroy(button);
            }
        }
    }

    void Update_Refs()
    {
        if ((hud_canvas.IsNullOrDestroyed()) && (!hud_object.IsNullOrDestroyed()))
        {
            hud_canvas = hud_object.GetComponent<Canvas>();
        }
        if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
        {
            if ((game_canvas.IsNullOrDestroyed()) && (Refs_Manager.game_uibase.canvases.Count > 0))
            {
                game_canvas = Refs_Manager.game_uibase.canvases[0];
            }
            if (
                (game_pause_menu.IsNullOrDestroyed())
                || (Hud_Base.Default_PauseMenu_Btns.IsNullOrDestroyed())
            )
            {
                if (UnityEngine.Time.unscaledTime >= nextPauseMenuProbe)
                {
                    nextPauseMenuProbe = UnityEngine.Time.unscaledTime + 1f;
                    Hud_Base.Get_DefaultPauseMenu();
                }
            }
            if (Hud_Base.Get_DefaultPauseMenu_Open())
            {
                Hud_Base.Toogle_DefaultPauseMenu(false);
            }
        }
        // The UI is an in-game overlay. Building Unity UI while the client
        // transitions through splash/login can race native scene initialization.
        // The player reference is populated independently by Refs_Manager.
        if (
            Scenes.IsGameScene()
            && !Refs_Manager.player_actor.IsNullOrDestroyed()
            && !(asset_bundle.IsNullOrDestroyed())
            && hud_object.IsNullOrDestroyed()
            && !hud_initializing
        )
        {
            Init_Hud();
        }
    }

    void Update_Hud_Scale()
    {
        if (
            (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            && (!game_canvas.IsNullOrDestroyed())
            && (!hud_canvas.IsNullOrDestroyed())
        )
        {
            if (hud_canvas.scaleFactor != game_canvas.scaleFactor)
            {
                hud_canvas.scaleFactor = game_canvas.scaleFactor;
            }
        }
    }

    void Update_Locale()
    {
        if (!Locales.update || hud_object.IsNullOrDestroyed())
            return;
        ModUI.Settings.LocaleRegistry.RefreshTree(hud_object);
        Locales.update = false;
    }

    void Update_Hud_Content()
    {
        ModUI.Shell.HudLayout.RefreshActivePage();
        if (!ModUI.ForceDrop.ForceDropModel.Enabled || !ModUI.ForceDrop.ForceDropModel.Initialized)
        {
            return;
        }
        if (!ModUI.ForceDrop.ForceDropModel.TypeInitialized)
        {
            ModUI.ForceDrop.ForceDropModel.InitForceDrop();
            return;
        }
        ModUI.ForceDrop.ForceDropBuilder.Tick();
    }

    public static bool IsPauseOpen()
    {
        if (!game_pause_menu.IsNullOrDestroyed())
        {
            return game_pause_menu.active;
        }
        else
        {
            return false;
        }
    }
}
