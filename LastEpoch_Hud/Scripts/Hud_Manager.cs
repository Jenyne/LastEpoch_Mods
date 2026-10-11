using System.IO;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppSystem.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.ModUI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

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
        Content.Set_Active();
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
                    Content.content_obj = Functions.GetChild(hud_object, "Content");
                    SafeInit(
                        "Content.Character",
                        () =>
                        {
                            Content.Character.Get_Refs();
                            Content.Character.Set_Events();
                            Content.Character.Set_Active(false);
                        }
                    );
                    SafeInit(
                        "Content.Items",
                        () =>
                        {
                            Content.Items.Get_Refs();
                            Content.Items.Set_Active(false);
                        }
                    );
                    SafeInit(
                        "Content.Scenes",
                        () =>
                        {
                            Content.Scenes.Get_Refs();
                            Content.Scenes.Set_Active(false);
                        }
                    );
                    SafeInit(
                        "Content.Skills",
                        () =>
                        {
                            Content.Skills.Get_Refs();
                            Content.Skills.Set_Active(false);
                        }
                    );
                    SafeInit(
                        "ForceDropModel",
                        () =>
                        {
                            ModUI.ForceDrop.ForceDropModel.GetRefs();
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

    public class Content
    {
        public static GameObject content_obj = null;

        public static void Set_Active()
        {
            if (!content_obj.IsNullOrDestroyed())
            {
                bool show = ModUI.ForceDrop.ForceDropModel.Enabled;
                if (content_obj.active != show)
                {
                    content_obj.active = show;
                }
            }
        }

        public class Character
        {
            public static GameObject content_obj = null;

            public static void Get_Refs()
            {
                if (!Content.content_obj.IsNullOrDestroyed())
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Character_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        //Data
                        GameObject character_data_content = Functions.GetViewportContent(
                            content_obj,
                            "Character_Data",
                            "Character_Data_Content"
                        );
                        if (!character_data_content.IsNullOrDestroyed())
                        {
                            Data.class_dropdown = Functions.Get_DopboxInPanel(
                                character_data_content,
                                "Classe",
                                "Dropdown_Character_Data_Classes",
                                new System.Action<int>(
                                    (_) =>
                                    {
                                        if (
                                            (!Refs_Manager.player_data.IsNullOrDestroyed())
                                            && (!Data.class_dropdown.IsNullOrDestroyed())
                                        )
                                        {
                                            Refs_Manager.player_data.CharacterClass =
                                                Data.class_dropdown.value;
                                        }
                                    }
                                )
                            );

                            Data.monolith_stability_basic_go = Functions.GetChild(
                                character_data_content,
                                "Monolith_Stability_Basic"
                            );
                            Data.monolith_stability_basic_go.active = false;
                            Data.monolith_stability_basic_text = Functions.Get_TextInButton(
                                character_data_content,
                                "Monolith_Stability_Basic",
                                "Value"
                            );
                            Data.monolith_stability_basic_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Monolith_Stability_Basic",
                                "Slider_Basic_Stability"
                            );

                            Data.monolith_stability_empower_go = Functions.GetChild(
                                character_data_content,
                                "Monolith_Stability_Empower"
                            );
                            Data.monolith_stability_empower_go.active = false;
                            Data.monolith_stability_empower_text = Functions.Get_TextInButton(
                                character_data_content,
                                "Monolith_Stability_Empower",
                                "Value"
                            );
                            Data.monolith_stability_empower_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Monolith_Stability_Empower",
                                "Slider_Empower_Stability"
                            );

                            Data.monolith_corruption_go = Functions.GetChild(
                                character_data_content,
                                "Monolith_Corruption"
                            );
                            Data.monolith_corruption_go.active = false;
                            Data.monolith_corruption_text = Functions.Get_TextInButton(
                                character_data_content,
                                "Monolith_Corruption",
                                "Value"
                            );
                            Data.monolith_corruption_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Monolith_Corruption",
                                "Slider_Empower_Corruption"
                            );

                            Data.monolith_gaze_go = Functions.GetChild(
                                character_data_content,
                                "Monolith_Gaze"
                            );
                            Data.monolith_gaze_go.active = false;
                            Data.monolith_gaze_text = Functions.Get_TextInButton(
                                character_data_content,
                                "Monolith_Gaze",
                                "Value"
                            );
                            Data.monolith_gaze_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Monolith_Gaze",
                                "Slider"
                            );
                            Data.SetupMonolithInputs();

                            Data.monolith_selector_go = Functions.GetChild(
                                character_data_content,
                                "Monoliths"
                            );
                            Data.monolith_dropdown = Functions.Get_DopboxInPanel(
                                character_data_content,
                                "Monoliths",
                                "Dropdown",
                                new System.Action<int>(
                                    (_) =>
                                    {
                                        Update_Monoliths_Data();
                                    }
                                )
                            );
                            Data.monolith_dropdown.options = new List<Dropdown.OptionData>();
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Select" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Fall_Of_The_Outcast" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "The_Stolen_Lance" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "The_Black_Sun" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Blood_Frost_And_Death" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Ending_The_Storm" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Fall_Of_The_Empire" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Reign_Of_Dragon" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "The_Last_Ruins" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "The_Age_Of_Winter" }
                            );
                            Data.monolith_dropdown.options.Add(
                                new Dropdown.OptionData { text = "Spirits_Of_Fire" }
                            );
                            Data.monolith_dropdown.m_CurrentIndex = 0;
                        }
                        else
                        {
                            Main.logger_instance.Error(
                                "Hud Manager : character_data_content is null"
                            );
                        }

                        GameObject char_data = Functions.GetChild(content_obj, "Character_Data");
                        if (!char_data.IsNullOrDestroyed())
                        {
                            GameObject panel_save = Functions.GetChild(char_data, "Panel");
                            if (!panel_save.IsNullOrDestroyed())
                            {
                                Data.save_button = Functions
                                    .GetChild(panel_save, "Btn_Character_Data_Save")
                                    .GetComponent<Button>();
                            }
                            Data.SetupCorruptionAllButton();
                        }

                        //Faction Tracker
                        GameObject character_faction_content = Functions.GetViewportContent(
                            content_obj,
                            "Character_Factions",
                            "Character_Factions_Content"
                        );
                        if (!character_faction_content.IsNullOrDestroyed())
                        {
                            Faction_Tracker.factions_dropdown = Functions.Get_DopboxInPanel(
                                character_faction_content,
                                "Factions_Dropdown",
                                "Dropdown",
                                // Hidden control: the page clones it; removed with Faction_Tracker.
                                new System.Action<int>(_ => { })
                            );
                            Faction_Tracker.factions_favor_slider = Functions.Get_SliderInPanel(
                                character_faction_content,
                                "Factions_Values",
                                "Slider_Character_Factions_Favor"
                            );
                            Faction_Tracker.factions_rank_slider = Functions.Get_SliderInPanel(
                                character_faction_content,
                                "Factions_Values",
                                "Slider_Character_Factions_Rank"
                            );
                            Faction_Tracker.factions_reputation_slider =
                                Functions.Get_SliderInPanel(
                                    character_faction_content,
                                    "Factions_Values",
                                    "Slider_Character_Factions_Reputation"
                                );
                        }
                    }
                    else
                    {
                        Main.logger_instance.Error("Hud Manager : Character_Content is null");
                    }
                }
            }

            public static void Set_Events()
            {
                if (Data.monolith_inputs_ready)
                {
                    Events.Set_Input_Event(
                        Data.monolith_stability_basic_input,
                        Data.monolith_stability_basic_input_Action
                    );
                    Events.Set_Input_Event(
                        Data.monolith_stability_empower_input,
                        Data.monolith_stability_empower_input_Action
                    );
                    Events.Set_Input_Event(
                        Data.monolith_corruption_input,
                        Data.monolith_corruption_input_Action
                    );
                    Events.Set_Input_Event(
                        Data.monolith_gaze_input,
                        Data.monolith_gaze_input_Action
                    );
                }
                else
                {
                    if (!Data.monolith_stability_basic_slider.IsNullOrDestroyed())
                    {
                        Events.Set_Slider_Event(
                            Data.monolith_stability_basic_slider,
                            Data.monolith_stability_basic_slider_Action
                        );
                    }
                    if (!Data.monolith_stability_empower_slider.IsNullOrDestroyed())
                    {
                        Events.Set_Slider_Event(
                            Data.monolith_stability_empower_slider,
                            Data.monolith_stability_empower_slider_Action
                        );
                    }
                    if (!Data.monolith_corruption_slider.IsNullOrDestroyed())
                    {
                        Events.Set_Slider_Event(
                            Data.monolith_corruption_slider,
                            Data.monolith_corruption_slider_Action
                        );
                    }
                    if (!Data.monolith_gaze_slider.IsNullOrDestroyed())
                    {
                        Events.Set_Slider_Event(
                            Data.monolith_gaze_slider,
                            Data.monolith_gaze_slider_Action
                        );
                    }
                }

                if (!Data.monolith_corruption_all_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Data.monolith_corruption_all_button,
                        Data.MonolithCorruptionAll_Action
                    );
                }
            }

            public static void Set_Active(bool show)
            {
                if (!content_obj.IsNullOrDestroyed())
                {
                    content_obj.active = show;
                }
            }

            public static void Update_Monoliths_Data()
            {
                if (Refs_Manager.player_data.IsNullOrDestroyed())
                {
                    Data.ShowMonolithValue(
                        Data.monolith_stability_basic_go,
                        Data.monolith_stability_basic_input,
                        Data.monolith_stability_basic_slider,
                        Data.monolith_stability_basic_text,
                        -1,
                        false
                    );
                    Data.ShowMonolithValue(
                        Data.monolith_stability_empower_go,
                        Data.monolith_stability_empower_input,
                        Data.monolith_stability_empower_slider,
                        Data.monolith_stability_empower_text,
                        -1,
                        false
                    );
                    Data.ShowMonolithValue(
                        Data.monolith_corruption_go,
                        Data.monolith_corruption_input,
                        Data.monolith_corruption_slider,
                        Data.monolith_corruption_text,
                        -1,
                        false
                    );
                    Data.ShowMonolithValue(
                        Data.monolith_gaze_go,
                        Data.monolith_gaze_input,
                        Data.monolith_gaze_slider,
                        Data.monolith_gaze_text,
                        -1,
                        false
                    );
                    ModUI.Pages.MonolithTimelineEditor.RefreshSelection();
                    return;
                }
                if (
                    (!Refs_Manager.player_data.IsNullOrDestroyed())
                    && (!Data.monolith_dropdown.IsNullOrDestroyed())
                    && (!Data.monolith_stability_basic_go.IsNullOrDestroyed())
                    && (!Data.monolith_stability_basic_slider.IsNullOrDestroyed())
                    && (!Data.monolith_stability_basic_text.IsNullOrDestroyed())
                    && (!Data.monolith_stability_empower_go.IsNullOrDestroyed())
                    && (!Data.monolith_stability_empower_slider.IsNullOrDestroyed())
                    && (!Data.monolith_stability_empower_text.IsNullOrDestroyed())
                    && (!Data.monolith_corruption_go.IsNullOrDestroyed())
                    && (!Data.monolith_corruption_slider.IsNullOrDestroyed())
                    && (!Data.monolith_corruption_text.IsNullOrDestroyed())
                    && (!Data.monolith_gaze_go.IsNullOrDestroyed())
                    && (!Data.monolith_gaze_slider.IsNullOrDestroyed())
                    && (!Data.monolith_gaze_text.IsNullOrDestroyed())
                )
                {
                    int index = Data.monolith_dropdown.value;
                    if (index < 1)
                    {
                        Data.ShowMonolithValue(
                            Data.monolith_stability_basic_go,
                            Data.monolith_stability_basic_input,
                            Data.monolith_stability_basic_slider,
                            Data.monolith_stability_basic_text,
                            -1,
                            false
                        );
                        Data.ShowMonolithValue(
                            Data.monolith_stability_empower_go,
                            Data.monolith_stability_empower_input,
                            Data.monolith_stability_empower_slider,
                            Data.monolith_stability_empower_text,
                            -1,
                            false
                        );
                        Data.ShowMonolithValue(
                            Data.monolith_corruption_go,
                            Data.monolith_corruption_input,
                            Data.monolith_corruption_slider,
                            Data.monolith_corruption_text,
                            -1,
                            false
                        );
                        Data.ShowMonolithValue(
                            Data.monolith_gaze_go,
                            Data.monolith_gaze_input,
                            Data.monolith_gaze_slider,
                            Data.monolith_gaze_text,
                            -1,
                            false
                        );
                    }
                    else
                    {
                        SavedMonolithRun basic = null;
                        SavedMonolithRun empower = null;
                        foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                        {
                            if (run.TimelineID == index)
                            {
                                if (run.DifficultyIndex == 0)
                                {
                                    basic = run;
                                }
                                else
                                {
                                    empower = run;
                                }
                            }
                        }

                        if (!basic.IsNullOrDestroyed())
                        {
                            Data.ShowMonolithValue(
                                Data.monolith_stability_basic_go,
                                Data.monolith_stability_basic_input,
                                Data.monolith_stability_basic_slider,
                                Data.monolith_stability_basic_text,
                                basic.Stability,
                                true
                            );
                        }
                        else
                        {
                            Data.ShowMonolithValue(
                                Data.monolith_stability_basic_go,
                                Data.monolith_stability_basic_input,
                                Data.monolith_stability_basic_slider,
                                Data.monolith_stability_basic_text,
                                -1,
                                false
                            );
                        }

                        if (!empower.IsNullOrDestroyed())
                        {
                            Data.ShowMonolithValue(
                                Data.monolith_stability_empower_go,
                                Data.monolith_stability_empower_input,
                                Data.monolith_stability_empower_slider,
                                Data.monolith_stability_empower_text,
                                empower.Stability,
                                true
                            );
                            if (!empower.SavedEchoWeb.IsNullOrDestroyed())
                            {
                                Data.ShowMonolithValue(
                                    Data.monolith_corruption_go,
                                    Data.monolith_corruption_input,
                                    Data.monolith_corruption_slider,
                                    Data.monolith_corruption_text,
                                    empower.SavedEchoWeb.Corruption,
                                    true
                                );
                                Data.ShowMonolithValue(
                                    Data.monolith_gaze_go,
                                    Data.monolith_gaze_input,
                                    Data.monolith_gaze_slider,
                                    Data.monolith_gaze_text,
                                    empower.SavedEchoWeb.GazeOfOrobyss,
                                    true
                                );
                            }
                            else
                            {
                                Data.ShowMonolithValue(
                                    Data.monolith_corruption_go,
                                    Data.monolith_corruption_input,
                                    Data.monolith_corruption_slider,
                                    Data.monolith_corruption_text,
                                    -1,
                                    false
                                );
                                Data.ShowMonolithValue(
                                    Data.monolith_gaze_go,
                                    Data.monolith_gaze_input,
                                    Data.monolith_gaze_slider,
                                    Data.monolith_gaze_text,
                                    -1,
                                    false
                                );
                            }
                        }
                        else
                        {
                            Data.ShowMonolithValue(
                                Data.monolith_stability_empower_go,
                                Data.monolith_stability_empower_input,
                                Data.monolith_stability_empower_slider,
                                Data.monolith_stability_empower_text,
                                -1,
                                false
                            );
                            Data.ShowMonolithValue(
                                Data.monolith_corruption_go,
                                Data.monolith_corruption_input,
                                Data.monolith_corruption_slider,
                                Data.monolith_corruption_text,
                                -1,
                                false
                            );
                            Data.ShowMonolithValue(
                                Data.monolith_gaze_go,
                                Data.monolith_gaze_input,
                                Data.monolith_gaze_slider,
                                Data.monolith_gaze_text,
                                -1,
                                false
                            );
                        }
                    }
                }
                ModUI.Pages.MonolithTimelineEditor.RefreshSelection();
            }

            public class Data
            {
                public static Dropdown class_dropdown = null;
                public static Dropdown monolith_dropdown = null;
                public static GameObject monolith_selector_go = null;
                public static bool monolith_inputs_ready = false;
                static bool suppress_monolith_input = false;

                public static GameObject monolith_stability_basic_go = null;
                public static Text monolith_stability_basic_text = null;
                public static Slider monolith_stability_basic_slider = null;
                public static Il2CppTMPro.TMP_InputField monolith_stability_basic_input = null;
                public static readonly System.Action<float> monolith_stability_basic_slider_Action =
                    new System.Action<float>(Set_monolith_stability_basic_slider);
                public static readonly System.Action<string> monolith_stability_basic_input_Action =
                    new System.Action<string>(Set_monolith_stability_basic);

                public static void Set_monolith_stability_basic_slider(float value)
                {
                    Set_monolith_stability_basic(((int)value).ToString());
                }

                public static void Set_monolith_stability_basic(string text)
                {
                    if (
                        suppress_monolith_input
                        || monolith_dropdown.IsNullOrDestroyed()
                        || !TryReadMonolithValue(text, out int result)
                    )
                    {
                        return;
                    }
                    int index = monolith_dropdown.value;
                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    {
                        foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                        {
                            if ((run.TimelineID == index) && (run.DifficultyIndex == 0))
                            {
                                run.Stability = result;
                                break;
                            }
                        }
                    }
                    ApplyLiveStability(index, 0, result);
                    if (!monolith_stability_basic_text.IsNullOrDestroyed())
                    {
                        monolith_stability_basic_text.text = result.ToString();
                    }
                }

                public static GameObject monolith_stability_empower_go = null;
                public static Text monolith_stability_empower_text = null;
                public static Slider monolith_stability_empower_slider = null;
                public static Il2CppTMPro.TMP_InputField monolith_stability_empower_input = null;
                public static readonly System.Action<float> monolith_stability_empower_slider_Action =
                    new System.Action<float>(Set_monolith_stability_empower_slider);
                public static readonly System.Action<string> monolith_stability_empower_input_Action =
                    new System.Action<string>(Set_monolith_stability_empower);

                public static void Set_monolith_stability_empower_slider(float value)
                {
                    Set_monolith_stability_empower(((int)value).ToString());
                }

                public static void Set_monolith_stability_empower(string text)
                {
                    if (
                        suppress_monolith_input
                        || monolith_dropdown.IsNullOrDestroyed()
                        || !TryReadMonolithValue(text, out int result)
                    )
                    {
                        return;
                    }
                    int index = monolith_dropdown.value;
                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    {
                        foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                        {
                            if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                            {
                                run.Stability = result;
                                break;
                            }
                        }
                    }
                    ApplyLiveStability(index, 1, result);
                    if (!monolith_stability_empower_text.IsNullOrDestroyed())
                    {
                        monolith_stability_empower_text.text = result.ToString();
                    }
                }

                public static GameObject monolith_corruption_go = null;
                public static Text monolith_corruption_text = null;
                public static Slider monolith_corruption_slider = null;
                public static Il2CppTMPro.TMP_InputField monolith_corruption_input = null;
                public static Button monolith_corruption_all_button = null;
                public static readonly System.Action MonolithCorruptionAll_Action =
                    new System.Action(CopySelectedTimelineToAll);
                public static readonly System.Action<float> monolith_corruption_slider_Action =
                    new System.Action<float>(Set_monolith_corruption_slider);
                public static readonly System.Action<string> monolith_corruption_input_Action =
                    new System.Action<string>(Set_monolith_corruption_empower);

                public static void Set_monolith_corruption_slider(float value)
                {
                    Set_monolith_corruption_empower(((int)value).ToString());
                }

                public static void Set_monolith_corruption_empower(string text)
                {
                    if (
                        suppress_monolith_input
                        || monolith_dropdown.IsNullOrDestroyed()
                        || !TryReadMonolithValue(text, out int result)
                    )
                    {
                        return;
                    }
                    int index = monolith_dropdown.value;
                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    {
                        foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                        {
                            if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                            {
                                if (!run.SavedEchoWeb.IsNullOrDestroyed())
                                {
                                    run.SavedEchoWeb.Corruption = result;
                                }
                                break;
                            }
                        }
                        if (Refs_Manager.player_data.MaxCorruption < result)
                        {
                            Refs_Manager.player_data.MaxCorruption = result;
                        }
                    }
                    ApplyLiveCorruption(index, result);
                    if (!monolith_corruption_text.IsNullOrDestroyed())
                    {
                        monolith_corruption_text.text = result.ToString();
                    }
                }

                public static GameObject monolith_gaze_go = null;
                public static Text monolith_gaze_text = null;
                public static Slider monolith_gaze_slider = null;
                public static Il2CppTMPro.TMP_InputField monolith_gaze_input = null;
                public static readonly System.Action<float> monolith_gaze_slider_Action =
                    new System.Action<float>(Set_monolith_gaze_slider);
                public static readonly System.Action<string> monolith_gaze_input_Action =
                    new System.Action<string>(Set_monolith_gaze_empower);

                public static void Set_monolith_gaze_slider(float value)
                {
                    Set_monolith_gaze_empower(((int)value).ToString());
                }

                public static void Set_monolith_gaze_empower(string text)
                {
                    if (
                        suppress_monolith_input
                        || monolith_dropdown.IsNullOrDestroyed()
                        || !TryReadMonolithValue(text, out int result)
                    )
                    {
                        return;
                    }
                    int index = monolith_dropdown.value;
                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    {
                        foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                        {
                            if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                            {
                                if (!run.SavedEchoWeb.IsNullOrDestroyed())
                                {
                                    run.SavedEchoWeb.GazeOfOrobyss = result;
                                }
                                break;
                            }
                        }
                    }
                    ApplyLiveGaze(index, result);
                    if (!monolith_gaze_text.IsNullOrDestroyed())
                    {
                        monolith_gaze_text.text = result.ToString();
                    }
                }

                public static void SetupCorruptionAllButton()
                {
                    if (
                        monolith_corruption_go.IsNullOrDestroyed()
                        || monolith_corruption_input.IsNullOrDestroyed()
                        || save_button.IsNullOrDestroyed()
                        || !monolith_corruption_all_button.IsNullOrDestroyed()
                    )
                    {
                        return;
                    }

                    GameObject parent =
                        monolith_corruption_go.transform.parent != null
                            ? monolith_corruption_go.transform.parent.gameObject
                            : null;
                    if (parent.IsNullOrDestroyed())
                    {
                        return;
                    }

                    GameObject buttonClone = Object.Instantiate(
                        save_button.gameObject,
                        parent.transform
                    );
                    buttonClone.name = "Btn_Monolith_Corruption_ApplyAll";
                    monolith_corruption_all_button = buttonClone.GetComponent<Button>();
                    PrepareFullWidthActionButton(
                        buttonClone,
                        monolith_corruption_all_button,
                        "Apply to All Timelines"
                    );
                }

                public static void CopySelectedTimelineToAll()
                {
                    if (
                        Refs_Manager.player_data.IsNullOrDestroyed()
                        || monolith_dropdown.IsNullOrDestroyed()
                        || monolith_dropdown.value <= 0
                    )
                    {
                        return;
                    }

                    bool copyBasic =
                        !monolith_stability_basic_go.IsNullOrDestroyed()
                        && monolith_stability_basic_go.activeSelf;
                    bool copyEmpowered =
                        !monolith_stability_empower_go.IsNullOrDestroyed()
                        && monolith_stability_empower_go.activeSelf;
                    bool copyCorruption =
                        !monolith_corruption_go.IsNullOrDestroyed()
                        && monolith_corruption_go.activeSelf;
                    bool copyGaze =
                        !monolith_gaze_go.IsNullOrDestroyed() && monolith_gaze_go.activeSelf;
                    int basic = 0,
                        empowered = 0,
                        corruption = 0,
                        gaze = 0;
                    if (
                        copyBasic
                        && !ReadMonolithControl(
                            monolith_stability_basic_input,
                            monolith_stability_basic_slider,
                            out basic
                        )
                    )
                    {
                        return;
                    }
                    if (
                        copyEmpowered
                        && !ReadMonolithControl(
                            monolith_stability_empower_input,
                            monolith_stability_empower_slider,
                            out empowered
                        )
                    )
                    {
                        return;
                    }
                    if (
                        copyCorruption
                        && !ReadMonolithControl(
                            monolith_corruption_input,
                            monolith_corruption_slider,
                            out corruption
                        )
                    )
                    {
                        return;
                    }
                    if (
                        copyGaze
                        && !ReadMonolithControl(monolith_gaze_input, monolith_gaze_slider, out gaze)
                    )
                    {
                        return;
                    }

                    var runs = new System.Collections.Generic.List<(
                        int TimelineId,
                        int Difficulty
                    )>();
                    foreach (SavedMonolithRun saved in Refs_Manager.player_data.MonolithRuns)
                    {
                        if (saved.DifficultyIndex == 0 && copyBasic)
                        {
                            saved.Stability = basic;
                            runs.Add((saved.TimelineID, 0));
                        }
                        else if (
                            saved.DifficultyIndex == 1
                            && (copyEmpowered || copyCorruption || copyGaze)
                        )
                        {
                            if (copyEmpowered)
                            {
                                saved.Stability = empowered;
                            }
                            if (!saved.SavedEchoWeb.IsNullOrDestroyed())
                            {
                                if (copyCorruption)
                                {
                                    saved.SavedEchoWeb.Corruption = corruption;
                                }
                                if (copyGaze)
                                {
                                    saved.SavedEchoWeb.GazeOfOrobyss = gaze;
                                }
                            }
                            runs.Add((saved.TimelineID, 1));
                        }
                    }
                    foreach (var run in runs)
                    {
                        if (run.Difficulty == 0)
                        {
                            ApplyLiveStability(run.TimelineId, 0, basic);
                        }
                        else
                        {
                            if (copyEmpowered)
                            {
                                ApplyLiveStability(run.TimelineId, 1, empowered);
                            }
                            if (copyCorruption)
                            {
                                ApplyLiveCorruption(run.TimelineId, corruption);
                            }
                            if (copyGaze)
                            {
                                ApplyLiveGaze(run.TimelineId, gaze);
                            }
                        }
                    }
                    if (
                        copyCorruption
                        && runs.Count > 0
                        && Refs_Manager.player_data.MaxCorruption < corruption
                    )
                    {
                        Refs_Manager.player_data.MaxCorruption = corruption;
                    }
                    Content.Character.Update_Monoliths_Data();
                    Main.logger_instance?.Msg(
                        "Copied selected values to " + runs.Count + " existing timeline runs"
                    );
                }

                static bool ReadMonolithControl(
                    Il2CppTMPro.TMP_InputField input,
                    Slider slider,
                    out int value
                )
                {
                    value = 0;
                    if (!input.IsNullOrDestroyed())
                    {
                        return TryReadMonolithValue(input.text, out value);
                    }
                    if (slider.IsNullOrDestroyed())
                    {
                        return false;
                    }
                    return TryReadMonolithValue(
                        Mathf.RoundToInt(slider.value).ToString(),
                        out value
                    );
                }

                static void PrepareNumericInput(
                    Il2CppTMPro.TMP_InputField input,
                    string defaultText
                )
                {
                    if (input.IsNullOrDestroyed())
                    {
                        return;
                    }
                    input.enabled = true;
                    input.interactable = true;
                    input.readOnly = false;
                    input.contentType = Il2CppTMPro.TMP_InputField.ContentType.IntegerNumber;
                    input.characterLimit = 7;
                    input.text = defaultText ?? "";

                    if (!input.targetGraphic.IsNullOrDestroyed())
                    {
                        input.targetGraphic.raycastTarget = true;
                    }
                    if (!input.textComponent.IsNullOrDestroyed())
                    {
                        input.textComponent.raycastTarget = false;
                        input.textComponent.horizontalAlignment = Il2CppTMPro
                            .HorizontalAlignmentOptions
                            .Left;
                        input.textComponent.verticalAlignment = Il2CppTMPro
                            .VerticalAlignmentOptions
                            .Middle;
                    }

                    CanvasGroup group = input.GetComponent<CanvasGroup>();
                    if (!group.IsNullOrDestroyed())
                    {
                        group.interactable = true;
                        group.blocksRaycasts = true;
                    }
                }

                static void PrepareFullWidthActionButton(
                    GameObject buttonObject,
                    Button button,
                    string labelText
                )
                {
                    if (buttonObject.IsNullOrDestroyed() || button.IsNullOrDestroyed())
                    {
                        return;
                    }
                    button.interactable = true;

                    LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
                    if (layout.IsNullOrDestroyed())
                    {
                        layout = buttonObject.AddComponent<LayoutElement>();
                    }
                    layout.ignoreLayout = false;
                    layout.minHeight = 34f;
                    layout.preferredHeight = 38f;
                    layout.flexibleWidth = 1f;

                    RectTransform rect = buttonObject.GetComponent<RectTransform>();
                    if (!rect.IsNullOrDestroyed())
                    {
                        rect.localScale = Vector3.one;
                    }

                    Text buttonText = null;
                    GameObject label = Functions.FindDescendant(buttonObject, "Label");
                    if (!label.IsNullOrDestroyed())
                    {
                        buttonText = label.GetComponent<Text>();
                    }
                    if (buttonText.IsNullOrDestroyed())
                    {
                        buttonText = buttonObject.GetComponentInChildren<Text>(true);
                    }
                    if (!buttonText.IsNullOrDestroyed())
                    {
                        RectTransform labelRect = buttonText.GetComponent<RectTransform>();
                        if (!labelRect.IsNullOrDestroyed())
                        {
                            labelRect.anchorMin = Vector2.zero;
                            labelRect.anchorMax = Vector2.one;
                            labelRect.offsetMin = Vector2.zero;
                            labelRect.offsetMax = Vector2.zero;
                        }
                        ModUI.Settings.Prefab.ApplyLabel(buttonText, labelText);
                    }
                }

                public static void MoveEndgameControls(
                    GameObject monolithTarget,
                    GameObject dungeonTarget
                )
                {
                    if (!monolithTarget.IsNullOrDestroyed())
                    {
                        ModUI.Pages.MonolithTimelineEditor.Build(monolithTarget);
                    }
                }

                public static void SetupMonolithInputs()
                {
                    monolith_stability_basic_input = CreateMonolithInput(
                        monolith_stability_basic_go,
                        monolith_stability_basic_slider,
                        monolith_stability_basic_text
                    );
                    monolith_stability_empower_input = CreateMonolithInput(
                        monolith_stability_empower_go,
                        monolith_stability_empower_slider,
                        monolith_stability_empower_text
                    );
                    monolith_corruption_input = CreateMonolithInput(
                        monolith_corruption_go,
                        monolith_corruption_slider,
                        monolith_corruption_text
                    );
                    monolith_gaze_input = CreateMonolithInput(
                        monolith_gaze_go,
                        monolith_gaze_slider,
                        monolith_gaze_text
                    );
                    monolith_inputs_ready =
                        !monolith_stability_basic_input.IsNullOrDestroyed()
                        && !monolith_stability_empower_input.IsNullOrDestroyed()
                        && !monolith_corruption_input.IsNullOrDestroyed()
                        && !monolith_gaze_input.IsNullOrDestroyed();
                }

                static Il2CppTMPro.TMP_InputField CreateMonolithInput(
                    GameObject panel,
                    Slider slider,
                    Text valueText
                )
                {
                    GameObject template = FindShardNameInput(hud_object);
                    if (
                        template.IsNullOrDestroyed()
                        || panel.IsNullOrDestroyed()
                        || slider.IsNullOrDestroyed()
                    )
                    {
                        return null;
                    }

                    slider.gameObject.SetActive(false);
                    if (!valueText.IsNullOrDestroyed())
                    {
                        valueText.gameObject.SetActive(false);
                    }

                    GameObject clone = Object.Instantiate(template, panel.transform);
                    clone.name = "MonolithValueInput";
                    RectTransform rect = clone.GetComponent<RectTransform>();
                    RectTransform source = slider.GetComponent<RectTransform>();
                    if (!rect.IsNullOrDestroyed() && !source.IsNullOrDestroyed())
                    {
                        rect.anchorMin = source.anchorMin;
                        rect.anchorMax = source.anchorMax;
                        rect.pivot = source.pivot;
                        rect.anchoredPosition = source.anchoredPosition;
                        rect.sizeDelta = source.sizeDelta;
                        rect.localScale = Vector3.one;
                    }

                    Il2CppTMPro.TMP_InputField input =
                        clone.GetComponent<Il2CppTMPro.TMP_InputField>();
                    if (!input.IsNullOrDestroyed())
                    {
                        PrepareNumericInput(input, "");
                        if (!input.textViewport.IsNullOrDestroyed())
                        {
                            RectTransform view = input.textViewport;
                            view.anchoredPosition = new Vector2(view.anchoredPosition.x, 0f);
                            view.sizeDelta = new Vector2(view.sizeDelta.x, 0f);
                        }
                    }
                    return input;
                }

                static GameObject FindShardNameInput(GameObject obj)
                {
                    if (obj.IsNullOrDestroyed())
                    {
                        return null;
                    }
                    for (int i = 0; i < obj.transform.childCount; i++)
                    {
                        GameObject child = obj.transform.GetChild(i).gameObject;
                        if (
                            (child.name == "InputField")
                            && (child.transform.parent != null)
                            && (child.transform.parent.name == "Name")
                            && (
                                !child
                                    .GetComponent<Il2CppTMPro.TMP_InputField>()
                                    .IsNullOrDestroyed()
                            )
                        )
                        {
                            return child;
                        }
                        GameObject nested = FindShardNameInput(child);
                        if (!nested.IsNullOrDestroyed())
                        {
                            return nested;
                        }
                    }
                    return null;
                }

                public static void ShowMonolithValue(
                    GameObject row,
                    Il2CppTMPro.TMP_InputField input,
                    Slider slider,
                    Text label,
                    int value,
                    bool show
                )
                {
                    if (!row.IsNullOrDestroyed())
                    {
                        row.active = show;
                    }
                    suppress_monolith_input = true;
                    try
                    {
                        SetMonolithInputOrSlider(input, slider, value);

                        if (!label.IsNullOrDestroyed())
                        {
                            label.text = value.ToString();
                        }
                    }
                    finally
                    {
                        suppress_monolith_input = false;
                    }
                }

                private static void SetMonolithInputOrSlider(
                    Il2CppTMPro.TMP_InputField input,
                    Slider slider,
                    int value
                )
                {
                    if (!input.IsNullOrDestroyed())
                    {
                        input.text = value.ToString();
                        return;
                    }
                    if (slider.IsNullOrDestroyed())
                    {
                        return;
                    }
                    slider.value = value;
                }

                static bool TryReadMonolithValue(string text, out int value)
                {
                    value = 0;
                    if (string.IsNullOrEmpty(text) || !int.TryParse(text, out value))
                    {
                        return false;
                    }
                    if (value < 0)
                    {
                        value = 0;
                    }
                    if (value > 9999999)
                    {
                        value = 9999999;
                    }
                    return true;
                }

                static MonolithRun FindLiveRun(int timelineId, int difficulty)
                {
                    if (Refs_Manager.player_actor.IsNullOrDestroyed())
                    {
                        return null;
                    }
                    MonolithRunsManager manager = Refs_Manager.player_actor.monolithRunsManager;
                    if (manager.IsNullOrDestroyed())
                    {
                        return null;
                    }
                    MonolithRun run = null;
                    if (
                        manager.TryGetRun((TimelineID)timelineId, difficulty, out run)
                        && !run.IsNullOrDestroyed()
                    )
                    {
                        return run;
                    }
                    try
                    {
                        manager.loadRuns();
                    }
                    catch (System.Exception ex)
                    {
                        ErrorLog.Report(ex, "Monoliths.loadRuns");
                        return null;
                    }
                    if (
                        manager.TryGetRun((TimelineID)timelineId, difficulty, out run)
                        && !run.IsNullOrDestroyed()
                    )
                    {
                        return run;
                    }
                    return null;
                }

                static void PersistLiveRuns()
                {
                    if (Refs_Manager.player_actor.IsNullOrDestroyed())
                    {
                        return;
                    }
                    MonolithRunsManager manager = Refs_Manager.player_actor.monolithRunsManager;
                    if (manager.IsNullOrDestroyed())
                    {
                        return;
                    }
                    try
                    {
                        manager.saveRuns();
                    }
                    catch (System.Exception exception)
                    {
                        Main.logger_instance?.Error("Monolith save failed: " + exception.Message);
                    }
                }

                static void RefreshOpenTimeline(MonolithRun run)
                {
                    if (run.IsNullOrDestroyed())
                    {
                        return;
                    }
                    try
                    {
                        MonolithTimelinePanelManager panel =
                            Object.FindObjectOfType<MonolithTimelinePanelManager>();
                        if (panel.IsNullOrDestroyed() || panel.run.IsNullOrDestroyed())
                        {
                            return;
                        }
                        if (
                            (panel.run.timelineID != run.timelineID)
                            || (panel.run.difficultyIndex != run.difficultyIndex)
                        )
                        {
                            return;
                        }
                        panel.SetRunInfo(run, true, false, -1);
                    }
                    catch (System.Exception exception)
                    {
                        Main.logger_instance?.Error(
                            "Monolith refresh failed: " + exception.Message
                        );
                    }
                }

                static void ApplyLiveStability(int timelineId, int difficulty, int value)
                {
                    MonolithRun run = FindLiveRun(timelineId, difficulty);
                    if (run.IsNullOrDestroyed())
                    {
                        return;
                    }
                    run.stability = value;
                    PersistLiveRuns();
                    RefreshOpenTimeline(run);
                }

                static void ApplyLiveCorruption(int timelineId, int value)
                {
                    MonolithRun run = FindLiveRun(timelineId, 1);
                    if (run.IsNullOrDestroyed() || run.web.IsNullOrDestroyed())
                    {
                        return;
                    }
                    run.web.corruption = value;
                    run.updateCorruptionMod();
                    if (!Refs_Manager.player_actor.IsNullOrDestroyed())
                    {
                        MonolithRunsManager manager = Refs_Manager.player_actor.monolithRunsManager;
                        if (!manager.IsNullOrDestroyed())
                        {
                            try
                            {
                                manager.updateHighestCorruptionOfAnyRun();
                            }
                            catch (System.Exception ex)
                            {
                                ErrorLog.Report(ex, "Monoliths.updateHighestCorruptionOfAnyRun");
                            }
                        }
                        LocalPlayer localPlayer =
                            Refs_Manager.player_actor.GetComponent<LocalPlayer>();
                        if (
                            !localPlayer.IsNullOrDestroyed()
                            && !localPlayer.monolithProgressManager.IsNullOrDestroyed()
                        )
                        {
                            localPlayer.monolithProgressManager.UpdateCorruption(
                                (TimelineID)timelineId,
                                value
                            );
                        }
                    }
                    PersistLiveRuns();
                    RefreshOpenTimeline(run);
                }

                static void ApplyLiveGaze(int timelineId, int value)
                {
                    MonolithRun run = FindLiveRun(timelineId, 1);
                    if (run.IsNullOrDestroyed() || run.web.IsNullOrDestroyed())
                    {
                        return;
                    }
                    run.web.gazeOfOrobyss = value;
                    PersistLiveRuns();
                    RefreshOpenTimeline(run);
                }

                public static Button save_button = null;

                public static void Save_Click()
                {
                    Main.logger_instance.Msg("Save Character Data");
                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    {
                        Refs_Manager.player_data.SaveData();
                    }
                }
            }

            public class Faction_Tracker
            {
                public static Dropdown factions_dropdown = null;
                public static Slider factions_favor_slider = null;
                public static Slider factions_rank_slider = null;
                public static Slider factions_reputation_slider = null;
            }
        }

        public class Items
        {
            public static GameObject content_obj = null;

            public static void Get_Refs()
            {
                if (Content.content_obj.IsNullOrDestroyed())
                {
                    return;
                }

                content_obj = Functions.GetChild(Content.content_obj, "Items_Content");
            }

            public static void Set_Active(bool show)
            {
                if (!content_obj.IsNullOrDestroyed())
                {
                    content_obj.active = show;
                }
            }
        }

        public class Scenes
        {
            public static GameObject content_obj = null;

            public static void Get_Refs()
            {
                if (Content.content_obj.IsNullOrDestroyed())
                {
                    return;
                }

                content_obj = Functions.GetChild(Content.content_obj, "Scenes_Content");
                if (content_obj.IsNullOrDestroyed())
                {
                    return;
                }

                GameObject scene_dungeons_content = Functions.GetViewportContent(
                    content_obj,
                    "Center",
                    "Scenes_Dungeons_Content"
                );
                if (!scene_dungeons_content.IsNullOrDestroyed())
                {
                    Dungeons.enter_without_key_toggle = Functions.Get_ToggleInPanel(
                        scene_dungeons_content,
                        "EnterWithoutKey",
                        "Toggle_Scenes_Dungeons_EnterWithoutKey"
                    );
                    HideSceneTeleport(scene_dungeons_content, "Teleport_Dropdown");
                    HideSceneTeleport(scene_dungeons_content, "Border (1)");
                    HideSceneTeleport(scene_dungeons_content, "Teleport_Btn");
                    HideSceneTeleport(scene_dungeons_content, "Border (2)");
                }

                GameObject scene_monoliths_content = Functions.GetViewportContent(
                    content_obj,
                    "Monoliths",
                    "Scenes_Monoliths_Content"
                );
                if (scene_monoliths_content.IsNullOrDestroyed())
                {
                    return;
                }

                Content.Character.Data.MoveEndgameControls(
                    scene_monoliths_content,
                    scene_dungeons_content
                );
            }

            public static void Set_Active(bool show)
            {
                if (!content_obj.IsNullOrDestroyed())
                {
                    content_obj.active = show;
                }
            }

            public class Camera
            {
                public static void Reset()
                {
                    Mods.Camera.Camera_Override.ResetToDefault();
                }

                public static void Set()
                {
                    Mods.Camera.Camera_Override.Set();
                }
            }

            public class Dungeons
            {
                public static Toggle enter_without_key_toggle = null;
            }

            static void HideSceneTeleport(GameObject content, string child_name)
            {
                GameObject child = Functions.GetChild(content, child_name);
                if (!child.IsNullOrDestroyed())
                {
                    child.SetActive(false);
                }
            }
        }

        public class Skills
        {
            public static GameObject content_obj = null;

            public static void Get_Refs()
            {
                if (Content.content_obj.IsNullOrDestroyed())
                {
                    return;
                }

                content_obj = Functions.GetChild(Content.content_obj, "Skill_Tree_Content");
            }

            public static void Set_Active(bool show)
            {
                if (!content_obj.IsNullOrDestroyed())
                {
                    content_obj.active = show;
                }
            }
        }
    }
}
