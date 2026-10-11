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
    public bool data_initialized = false;

    private string asset_path = Application.dataPath + "/../Mods/" + Main.mod_name + "/Assets";
    private static Canvas game_canvas = null;
    public static GameObject game_pause_menu = null;
    private static Canvas hud_canvas = null;
    private readonly string asset_bundle_name = "lastepochmods"; //Name of asset file
    private bool hud_initializing = false;
    private bool data_initializing = false;

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
                if ((!data_initialized) && (!data_initializing))
                {
                    Init_UserData();
                } //set once
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

    void Init_UserData()
    {
        data_initializing = true;
        try
        {
            if (!Save_Manager.instance.IsNullOrDestroyed())
            {
                if (Main.debug)
                {
                    Main.logger_instance.Msg("Hud Manager : Initialize user config");
                }
                if (!Content.Character.Init_UserData())
                {
                    return;
                }

                if (Main.debug)
                {
                    Main.logger_instance.Msg("Hud Manager : Initialized");
                }
                data_initialized = true;
            }
        }
        catch (System.Exception ex)
        {
            ErrorLog.Report(ex, "Hud_Manager.Init_UserData");
        }
        finally
        {
            data_initializing = false;
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
                        GameObject character_cheats_content = Functions.GetViewportContent(
                            content_obj,
                            "Character_Cheats",
                            "Character_Cheats_Content"
                        );
                        if (!character_cheats_content.IsNullOrDestroyed())
                        {
                            Cheats.godmode_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "GodMode",
                                "Toggle_Character_Cheats_GodMode"
                            );
                            Cheats.lowlife_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "ForceLowLife",
                                "Toggle_Character_Cheats_LowLife"
                            );
                            Cheats.unlock_all_idols = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "UnlockAllIdolsSlots",
                                "Toggle_Character_Cheats_UnlockAllIdols"
                            );

                            Cheats.autoPot_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "AutoPotions",
                                "Toggle_Character_Cheats_AutoPotions"
                            );
                            Cheats.autopot_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "AutoPotions",
                                "Slider_Character_Cheats_AutoPotions"
                            );

                            Cheats.density_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "DensityMultiplier",
                                "Toggle_Character_Cheats_DensityMultiplier"
                            );
                            Cheats.density_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "DensityMultiplier",
                                "Slider_Character_Cheats_DensityMultiplier"
                            );

                            Cheats.experience_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "ExperienceMultiplier",
                                "Toggle_Character_Cheats_ExperienceMultiplier"
                            );
                            Cheats.experience_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "ExperienceMultiplier",
                                "Slider_Character_Cheats_ExperienceMultiplier"
                            );

                            Cheats.ability_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "AbilityMultiplier",
                                "Toggle_Character_Cheats_AbilityMultiplier"
                            );
                            Cheats.ability_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "AbilityMultiplier",
                                "Slider_Character_Cheats_AbilityMultiplier"
                            );

                            Cheats.favor_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "FavorMultiplier",
                                "Toggle_Character_Cheats_FavorMultiplier"
                            );
                            Cheats.favor_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "FavorMultiplier",
                                "Slider_Character_Cheats_FavorMultiplier"
                            );

                            Cheats.memoryamber_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "MemoryAmberMultiplier",
                                "Toggle_Character_Cheats_MemoryAmberMultiplier"
                            );
                            Cheats.memoryamber_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "MemoryAmberMultiplier",
                                "Slider_Character_Cheats_MemoryAmberMultiplier"
                            );
                            Cheats.PrepareMemoryAmberMultiplier();

                            Cheats.itemdropmultiplier_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "ItemDropMultiplier",
                                "Toggle_Character_Cheats_ItemDropMultiplier"
                            );
                            Cheats.itemdropmultiplier_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "ItemDropMultiplier",
                                "Slider_Character_Cheats_ItemDropMultiplier"
                            );

                            Cheats.itemdropchance_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "ItemDropChance",
                                "Toggle_Character_Cheats_ItemDropChance"
                            );
                            Cheats.itemdropchance_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "ItemDropChance",
                                "Slider_Character_Cheats_ItemDropChance"
                            );

                            Cheats.golddropmultiplier_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "GoldDropMultiplier",
                                "Toggle_Character_Cheats_GoldDropMultiplier"
                            );
                            Cheats.golddropmultiplier_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "GoldDropMultiplier",
                                "Slider_Character_Cheats_GoldDropMultiplier"
                            );

                            Cheats.golddropchance_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "GoldDropChance",
                                "Toggle_Character_Cheats_GoldDropChance"
                            );
                            Cheats.golddropchance_slider = Functions.Get_SliderInPanel(
                                character_cheats_content,
                                "GoldDropChance",
                                "Slider_Character_Cheats_GoldDropChance"
                            );

                            Cheats.waypoints_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "WaypointsUnlock",
                                "Toggle_Character_Cheats_UnlockAllWaypoints"
                            );
                            Cheats.twohanded_shield_toggle = Functions.Get_ToggleInPanel(
                                character_cheats_content,
                                "TwoHandeWithShield",
                                "Toggle_Character_Cheats_TwoHandeWithShield"
                            );

                            Cheats.level_once_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_LevelOnce"
                                )
                                .GetComponent<Button>();
                            Cheats.level_max_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_LevelToMax"
                                )
                                .GetComponent<Button>();
                            Cheats.complete_quest_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_CompleteQuest"
                                )
                                .GetComponent<Button>();
                            Cheats.masterie_buttons = Functions
                                .GetChild(character_cheats_content, "Btn_Character_Cheats_Masterie")
                                .GetComponent<Button>();
                            Cheats.masterie_text = Functions.Get_TextInButton(
                                character_cheats_content,
                                "Btn_Character_Cheats_Masterie",
                                "Label"
                            );
                            Cheats.add_runes_button = Functions
                                .GetChild(character_cheats_content, "Btn_Character_Cheats_AddRunes")
                                .GetComponent<Button>();
                            Cheats.add_glyphs_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_AddGlyphs"
                                )
                                .GetComponent<Button>();
                            Cheats.add_shards_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_AddAffixs"
                                )
                                .GetComponent<Button>();
                            GameObject ancient_bones_button = Functions.FindDescendant(
                                character_cheats_content,
                                "Btn_Character_Cheats_AddAncienBone"
                            );
                            if (!ancient_bones_button.IsNullOrDestroyed())
                            {
                                Cheats.add_ancient_bones_button =
                                    ancient_bones_button.GetComponent<Button>();
                                Mods.Character.Character_MemoryAmber.BuildButton(
                                    Cheats.add_ancient_bones_button
                                );
                            }
                            Cheats.discover_blessings_button = Functions
                                .GetChild(
                                    character_cheats_content,
                                    "Btn_Character_Cheats_DicoverAllBlessings"
                                )
                                .GetComponent<Button>();
                            Cheats.BuildChooseBlessingsButton(character_cheats_content);
                            Cheats.BuildUnlockBlessingSlotsButton();
                        }
                        else
                        {
                            Main.logger_instance.Error(
                                "Hud Manager : character_cheats_content is null"
                            );
                        }

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

                            Data.died_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "Died",
                                "Toggle_Character_Data_Died"
                            );
                            Data.hardcore_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "Hardcore",
                                "Toggle_Character_Data_Hardcore"
                            );
                            Data.masochist_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "Masochist",
                                "Toggle_Character_Data_Masochist"
                            );
                            Data.portal_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "Portal Unlocked",
                                "Toggle_Character_Data_Portal"
                            );
                            Data.solo_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "SoloChallenge",
                                "Toggle_Character_Data_SoloChallenge"
                            );

                            Data.deaths_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Deaths",
                                "Slider_Character_Data_Deaths"
                            );

                            Data.lantern_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "LanternLuminance",
                                "Slider_Character_Data_LanternLuminance"
                            );

                            Data.soul_row = Functions.GetChild(
                                character_data_content,
                                "Soul Embers"
                            );
                            Data.soul_text = Functions.Get_TextInButton(
                                character_data_content,
                                "Soul Embers",
                                "Value"
                            );
                            Data.soul_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "Soul Embers",
                                "Slider_Character_Data_SoulEmbers"
                            );

                            Data.weaver_points_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "TreePoints",
                                "Toggle_Weaver_TreePoints"
                            );
                            Data.weaver_points_text = Functions.Get_TextInToggle(
                                character_data_content,
                                "TreePoints",
                                "Toggle_Weaver_TreePoints",
                                "Value"
                            );
                            Data.weaver_points_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "TreePoints",
                                "Slider_Weaver_TreePoints"
                            );
                            if (!Data.weaver_points_slider.IsNullOrDestroyed())
                            {
                                Data.weaver_points_slider.interactable = true;
                                Data.EnsureWeaverRange();
                            }
                            if (!Save_Manager.instance.IsNullOrDestroyed())
                            {
                                var woven = Save_Manager.instance.data.Factions.TheWoven;
                                Data.suppressWeaverSlider = true;
                                try
                                {
                                    if (!Data.weaver_points_toggle.IsNullOrDestroyed())
                                    {
                                        Data.weaver_points_toggle.isOn = woven.Enable_TreePoints;
                                    }
                                    if (!Data.weaver_points_slider.IsNullOrDestroyed())
                                    {
                                        Data.weaver_points_slider.value = woven.TreePoints;
                                    }
                                    if (!Data.weaver_points_text.IsNullOrDestroyed())
                                    {
                                        Data.weaver_points_text.text = woven.TreePoints.ToString();
                                    }
                                }
                                finally
                                {
                                    Data.suppressWeaverSlider = false;
                                }
                            }
                            SettingRow.AddMultiplier(
                                character_data_content,
                                "TreePoints",
                                "TreePointMultiplier",
                                "Toggle_Weaver_TreePoints",
                                "Toggle_Weaver_PointMultiplier",
                                "Slider_Weaver_TreePoints",
                                "Slider_Weaver_PointMultiplier",
                                "Weaver Point Multiplier"
                            );
                            Data.weaver_multiplier_toggle = Functions.Get_ToggleInPanel(
                                character_data_content,
                                "TreePointMultiplier",
                                "Toggle_Weaver_PointMultiplier"
                            );
                            Data.weaver_multiplier_slider = Functions.Get_SliderInPanel(
                                character_data_content,
                                "TreePointMultiplier",
                                "Slider_Weaver_PointMultiplier"
                            );
                            if (!Save_Manager.instance.IsNullOrDestroyed())
                            {
                                int multiplier = SettingRow.Clamp(
                                    Save_Manager.instance.data.Factions.TheWoven.PointMultiplier
                                );
                                SettingRow.PrepareSlider(Data.weaver_multiplier_slider, multiplier);
                                if (!Data.weaver_multiplier_toggle.IsNullOrDestroyed())
                                {
                                    Data.weaver_multiplier_toggle.isOn = Save_Manager
                                        .instance
                                        .data
                                        .Factions
                                        .TheWoven
                                        .Enable_PointMultiplier;
                                }
                            }

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
                            Data.SetupSoulEmberControls();
                            Data.SetupCorruptionAllButton();
                            ModUI.Settings.CharacterActionControls.Bind(
                                character_cheats_content,
                                character_data_content
                            );
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
                                new System.Action<int>(
                                    (_) =>
                                    {
                                        Update_Faction_Data();
                                    }
                                )
                            );
                            Faction_Tracker.factions_go = Functions.GetChild(
                                character_faction_content,
                                "Factions_Values"
                            );
                            if (!Faction_Tracker.factions_go.IsNullOrDestroyed())
                            {
                                Faction_Tracker.factions_go.active = false;
                            }

                            Faction_Tracker.factions_favor_text = Functions.Get_TextInButton(
                                character_faction_content,
                                "Factions_Values",
                                "FavorValue"
                            );
                            Faction_Tracker.factions_favor_slider = Functions.Get_SliderInPanel(
                                character_faction_content,
                                "Factions_Values",
                                "Slider_Character_Factions_Favor"
                            );
                            Faction_Tracker.factions_gain_favor_button = Functions
                                .GetChild(
                                    Faction_Tracker.factions_go,
                                    "Btn_Character_Factions_Gain_Favor"
                                )
                                .GetComponent<Button>();

                            Faction_Tracker.factions_rank_text = Functions.Get_TextInButton(
                                character_faction_content,
                                "Factions_Values",
                                "RankValue"
                            );
                            Faction_Tracker.factions_rank_slider = Functions.Get_SliderInPanel(
                                character_faction_content,
                                "Factions_Values",
                                "Slider_Character_Factions_Rank"
                            );
                            Faction_Tracker.factions_set_rank_button = Functions
                                .GetChild(
                                    Faction_Tracker.factions_go,
                                    "Btn_Character_Factions_Set_Rang"
                                )
                                .GetComponent<Button>();

                            Faction_Tracker.factions_reputation_text = Functions.Get_TextInButton(
                                character_faction_content,
                                "Factions_Values",
                                "ReputationValue"
                            );
                            Faction_Tracker.factions_reputation_slider =
                                Functions.Get_SliderInPanel(
                                    character_faction_content,
                                    "Factions_Values",
                                    "Slider_Character_Factions_Reputation"
                                );
                            Faction_Tracker.factions_set_reputation_button = Functions
                                .GetChild(
                                    Faction_Tracker.factions_go,
                                    "Btn_Character_Factions_Set_Reputation"
                                )
                                .GetComponent<Button>();
                        }

                        //Buffs
                        Buffs.enable_mod = Functions.Get_ToggleInLabel(
                            content_obj,
                            "Character_Buffs",
                            "Toggle_Character_Buffs_Enable"
                        );
                        GameObject character_buffs_content = Functions.GetViewportContent(
                            content_obj,
                            "Character_Buffs",
                            "Character_Buffs_Content"
                        );
                        if (!character_buffs_content.IsNullOrDestroyed())
                        {
                            //Movespeed
                            Buffs.movespeed_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "MoveSpeed",
                                "Toggle_Character_Buffs_MoveSpeed"
                            );
                            Buffs.movespeed_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "MoveSpeed",
                                "Slider_Character_Buffs_MoveSpeed"
                            );

                            //Damage
                            Buffs.damage_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Damage",
                                "Toggle_Character_Buffs_Damage"
                            );
                            Buffs.damage_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Damage",
                                "Slider_Character_Buffs_Damage"
                            );

                            Buffs.EnsureAreaOfEffectBuffRow(character_buffs_content);

                            //AreaOfEffect
                            Buffs.aoe_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "AreaOfEffect",
                                "Toggle_Character_Buffs_AreaOfEffect"
                            );
                            Buffs.aoe_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "AreaOfEffect",
                                "Slider_Character_Buffs_AreaOfEffect"
                            );

                            //AttackSpeed
                            Buffs.attackspeed_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "AttackSpeed",
                                "Toggle_Character_Buffs_AttackSpeed"
                            );
                            Buffs.attackspeed_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "AttackSpeed",
                                "Slider_Character_Buffs_AttackSpeed"
                            );

                            //CastingSpeed
                            Buffs.castingspeed_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "CastingSpeed",
                                "Toggle_Character_Buffs_CastingSpeed"
                            );
                            Buffs.castingspeed_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "CastingSpeed",
                                "Slider_Character_Buffs_CastingSpeed"
                            );

                            //CriticalChance
                            Buffs.criticalchance_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "CriticalChance",
                                "Toggle_Character_Buffs_CriticalChance"
                            );
                            Buffs.criticalchance_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "CriticalChance",
                                "Slider_Character_Buffs_CriticalChance"
                            );

                            //CriticalMultiplier
                            Buffs.criticalmultiplier_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "CriticalMultiplier",
                                "Toggle_Character_Buffs_CriticalMultiplier"
                            );
                            Buffs.criticalmultiplier_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "CriticalMultiplier",
                                "Slider_Character_Buffs_CriticalMultiplier"
                            );

                            //HealthRegen
                            Buffs.healthregen_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "HealthRegen",
                                "Toggle_Character_Buffs_HealthRegen"
                            );
                            Buffs.healthregen_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "HealthRegen",
                                "Slider_Character_Buffs_HealthRegen"
                            );

                            //ManaRegen
                            Buffs.manaregen_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "ManaRegen",
                                "Toggle_Character_Buffs_ManaRegen"
                            );
                            Buffs.manaregen_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "ManaRegen",
                                "Slider_Character_Buffs_ManaRegen"
                            );

                            //Strenght
                            Buffs.str_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Strenght",
                                "Toggle_Character_Buffs_Strenght"
                            );
                            Buffs.str_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Strenght",
                                "Slider_Character_Buffs_Strenght"
                            );

                            //Intelligence
                            Buffs.int_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Intelligence",
                                "Toggle_Character_Buffs_Intelligence"
                            );
                            Buffs.int_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Intelligence",
                                "Slider_Character_Buffs_Intelligence"
                            );

                            //Dexterity
                            Buffs.dex_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Dexterity",
                                "Toggle_Character_Buffs_Dexterity"
                            );
                            Buffs.dex_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Dexterity",
                                "Slider_Character_Buffs_Dexterity"
                            );

                            //Vitality
                            Buffs.vit_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Vitality",
                                "Toggle_Character_Buffs_Vitality"
                            );
                            Buffs.vit_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Vitality",
                                "Slider_Character_Buffs_Vitality"
                            );

                            //Attunement
                            Buffs.att_toggle = Functions.Get_ToggleInPanel(
                                character_buffs_content,
                                "Attunement",
                                "Toggle_Character_Buffs_Attunement"
                            );
                            Buffs.att_slider = Functions.Get_SliderInPanel(
                                character_buffs_content,
                                "Attunement",
                                "Slider_Character_Buffs_Attunement"
                            );
                        }
                        else
                        {
                            Main.logger_instance.Error(
                                "Hud Manager : character_buffs_content is null"
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
                if (!Cheats.godmode_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.godmode_toggle, Cheats.Godmode_Toggle_Action);
                }
                if (!Cheats.twohanded_shield_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.twohanded_shield_toggle,
                        Cheats.TwoHandedShield_Toggle_Action
                    );
                }
                if (!Cheats.lowlife_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.lowlife_toggle, Cheats.Lowlife_Toggle_Action);
                }
                if (!Cheats.max_blessings_button.IsNullOrDestroyed())
                    Events.Set_Button_Event(
                        Cheats.max_blessings_button,
                        Cheats.MaxOutBlessings_OnClick_Action
                    );
                if (!Cheats.unlock_blessing_slots_button.IsNullOrDestroyed())
                    Events.Set_Button_Event(
                        Cheats.unlock_blessing_slots_button,
                        Cheats.UnlockBlessingSlots_OnClick_Action
                    );
                if (!Cheats.choose_blessings_button.IsNullOrDestroyed())
                    Events.Set_Button_Event(
                        Cheats.choose_blessings_button,
                        Cheats.ChooseBlessings_OnClick_Action
                    );
                if (!Cheats.unlock_all_idols.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.unlock_all_idols,
                        Cheats.UnlockAllIdols_Toggle_Action
                    );
                }
                if (!Cheats.autoPot_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.autoPot_toggle, Cheats.AutoPot_Toggle_Action);
                }
                if (!Cheats.density_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.density_toggle, Cheats.Density_Toggle_Action);
                }
                if (!Cheats.experience_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.experience_toggle,
                        Cheats.Experience_Toggle_Action
                    );
                }
                if (!Cheats.ability_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.ability_toggle, Cheats.Ability_Toggle_Action);
                }
                if (!Cheats.favor_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(Cheats.favor_toggle, Cheats.Favor_Toggle_Action);
                }
                if (!Cheats.memoryamber_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.memoryamber_toggle,
                        Cheats.memoryamber_Toggle_Action
                    );
                }
                if (!Cheats.itemdropmultiplier_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.itemdropmultiplier_toggle,
                        Cheats.ItemDropMulti_Toggle_Action
                    );
                }
                if (!Cheats.itemdropchance_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.itemdropchance_toggle,
                        Cheats.ItemDropChance_Toggle_Action
                    );
                }
                if (!Cheats.golddropmultiplier_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.golddropmultiplier_toggle,
                        Cheats.GoldMulti_Toggle_Action
                    );
                }
                if (!Cheats.golddropchance_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.golddropchance_toggle,
                        Cheats.GoldChance_Toggle_Action
                    );
                }
                if (!Cheats.waypoints_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Cheats.waypoints_toggle,
                        Cheats.Waypoints_Toggle_Action
                    );
                }
                if (!Cheats.level_once_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.level_once_button,
                        Cheats.LevelUpOnce_OnClick_Action
                    );
                }
                if (!Cheats.level_max_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.level_max_button,
                        Cheats.LevelUpMax_OnClick_Action
                    );
                }
                if (!Cheats.complete_quest_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.complete_quest_button,
                        Cheats.CompleteQuest_OnClick_Action
                    );
                }
                if (!Cheats.masterie_buttons.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.masterie_buttons,
                        Cheats.Masteries_OnClick_Action
                    );
                }
                if (!Cheats.add_runes_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.add_runes_button,
                        Cheats.AddRunes_OnClick_Action
                    );
                }
                if (!Cheats.add_glyphs_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.add_glyphs_button,
                        Cheats.AddGlyphs_OnClick_Action
                    );
                }
                if (!Cheats.add_shards_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.add_shards_button,
                        Cheats.AddAffixs_OnClick_Action
                    );
                }
                if (!Cheats.add_ancient_bones_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.add_ancient_bones_button,
                        Cheats.AddAncientBones_OnClick_Action
                    );
                }
                if (!Cheats.discover_blessings_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Cheats.discover_blessings_button,
                        Cheats.DiscoverAllBlessings_OnClick_Action
                    );
                }
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
                if (!Data.weaver_points_toggle.IsNullOrDestroyed())
                {
                    Events.Set_Toggle_Event(
                        Data.weaver_points_toggle,
                        Data.weaver_points_toggle_Action
                    );
                }

                if (!Data.soul_add_button.IsNullOrDestroyed())
                {
                    // This runtime button owns its listeners. Rebinding must not
                    // stack a second currency grant onto the existing click.
                    Data.soul_add_button.onClick.RemoveAllListeners();
                    Events.Set_Button_Event(Data.soul_add_button, Data.SoulEmbers_Add_Action);
                }
                if (!Data.monolith_corruption_all_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Data.monolith_corruption_all_button,
                        Data.MonolithCorruptionAll_Action
                    );
                }
                if (!Data.save_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(Data.save_button, Data.Save_OnClick_Action);
                }

                if (!Faction_Tracker.factions_favor_slider.IsNullOrDestroyed())
                {
                    Events.Set_Slider_Event(
                        Faction_Tracker.factions_favor_slider,
                        Faction_Tracker.factions_favor_slider_Action
                    );
                }
                if (!Faction_Tracker.factions_gain_favor_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Faction_Tracker.factions_gain_favor_button,
                        Faction_Tracker.factions_gain_favor_OnClick_Action
                    );
                }
                if (!Faction_Tracker.factions_set_favor_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Faction_Tracker.factions_set_favor_button,
                        Faction_Tracker.factions_set_favor_OnClick_Action
                    );
                }
                if (!Faction_Tracker.factions_rank_slider.IsNullOrDestroyed())
                {
                    Events.Set_Slider_Event(
                        Faction_Tracker.factions_rank_slider,
                        Faction_Tracker.factions_rank_slider_Action
                    );
                }
                if (!Faction_Tracker.factions_set_rank_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Faction_Tracker.factions_set_rank_button,
                        Faction_Tracker.factions_set_rank_OnClick_Action
                    );
                }
                if (!Faction_Tracker.factions_reputation_slider.IsNullOrDestroyed())
                {
                    Events.Set_Slider_Event(
                        Faction_Tracker.factions_reputation_slider,
                        Faction_Tracker.factions_reputation_slider_Action
                    );
                }
                if (!Faction_Tracker.factions_set_reputation_button.IsNullOrDestroyed())
                {
                    Events.Set_Button_Event(
                        Faction_Tracker.factions_set_reputation_button,
                        Faction_Tracker.factions_set_reputation_OnClick_Action
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

            public static bool Init_UserData()
            {
                bool result = false;
                if (!Save_Manager.instance.IsNullOrDestroyed())
                {
                    if (Save_Manager.instance.initialized)
                    {
                        //Content.Character.Cheats
                        if (!Cheats.godmode_toggle.IsNullOrDestroyed())
                        {
                            Cheats.godmode_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_GodMode;
                        }
                        if (!Cheats.twohanded_shield_toggle.IsNullOrDestroyed())
                        {
                            Cheats.twohanded_shield_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_TwoHandedWithShield;
                        }
                        if (!Cheats.lowlife_toggle.IsNullOrDestroyed())
                        {
                            Cheats.lowlife_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_LowLife;
                        }
                        if (!Cheats.unlock_all_idols.IsNullOrDestroyed())
                        {
                            Cheats.unlock_all_idols.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_UnlockAllIdolsSlots;
                        }
                        if (!Cheats.autoPot_toggle.IsNullOrDestroyed())
                        {
                            Cheats.autoPot_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_AutoPot;
                        }
                        if (!Cheats.autopot_slider.IsNullOrDestroyed())
                        {
                            Cheats.autopot_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .autoPot;
                        }
                        if (!Cheats.density_toggle.IsNullOrDestroyed())
                        {
                            Cheats.density_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_DensityMultiplier;
                        }
                        if (!Cheats.density_slider.IsNullOrDestroyed())
                        {
                            Cheats.density_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .DensityMultiplier;
                        }
                        if (!Cheats.experience_toggle.IsNullOrDestroyed())
                        {
                            Cheats.experience_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_ExperienceMultiplier;
                        }
                        if (!Cheats.experience_slider.IsNullOrDestroyed())
                        {
                            Cheats.experience_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .ExperienceMultiplier;
                        }
                        if (!Cheats.ability_toggle.IsNullOrDestroyed())
                        {
                            Cheats.ability_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_AbilityMultiplier;
                        }
                        if (!Cheats.ability_slider.IsNullOrDestroyed())
                        {
                            Cheats.ability_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .AbilityMultiplier;
                        }
                        if (!Cheats.favor_toggle.IsNullOrDestroyed())
                        {
                            Cheats.favor_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_FavorMultiplier;
                        }
                        if (!Cheats.favor_slider.IsNullOrDestroyed())
                        {
                            Cheats.favor_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .FavorMultiplier;
                        }
                        if (!Cheats.memoryamber_toggle.IsNullOrDestroyed())
                        {
                            Cheats.memoryamber_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_MemoryAmberMultiplier;
                        }
                        if (!Cheats.memoryamber_slider.IsNullOrDestroyed())
                        {
                            Cheats.memoryamber_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .MemoryAmberMultiplier;
                        }
                        if (!Cheats.itemdropmultiplier_toggle.IsNullOrDestroyed())
                        {
                            Cheats.itemdropmultiplier_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_ItemDropMultiplier;
                        }
                        if (!Cheats.itemdropmultiplier_slider.IsNullOrDestroyed())
                        {
                            Cheats.itemdropmultiplier_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .ItemDropMultiplier;
                        }
                        if (!Cheats.itemdropchance_toggle.IsNullOrDestroyed())
                        {
                            Cheats.itemdropchance_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_ItemDropChance;
                        }
                        if (!Cheats.itemdropchance_slider.IsNullOrDestroyed())
                        {
                            Cheats.itemdropchance_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .ItemDropChance;
                        }
                        if (!Cheats.golddropmultiplier_toggle.IsNullOrDestroyed())
                        {
                            Cheats.golddropmultiplier_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_GoldDropMultiplier;
                        }
                        if (!Cheats.golddropmultiplier_slider.IsNullOrDestroyed())
                        {
                            Cheats.golddropmultiplier_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .GoldDropMultiplier;
                        }
                        if (!Cheats.golddropchance_toggle.IsNullOrDestroyed())
                        {
                            Cheats.golddropchance_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_GoldDropChance;
                        }
                        if (!Cheats.golddropchance_slider.IsNullOrDestroyed())
                        {
                            Cheats.golddropchance_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .GoldDropChance;
                        }
                        if (!Cheats.waypoints_toggle.IsNullOrDestroyed())
                        {
                            Cheats.waypoints_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .Cheats
                                .Enable_WaypointsUnlock;
                        }
                        //Content.Character.Buffs
                        if (!Buffs.enable_mod.IsNullOrDestroyed())
                        {
                            Buffs.enable_mod.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Mod;
                        }
                        if (!Buffs.movespeed_toggle.IsNullOrDestroyed())
                        {
                            Buffs.movespeed_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_MoveSpeed_Buff;
                        }
                        if (!Buffs.movespeed_slider.IsNullOrDestroyed())
                        {
                            Buffs.movespeed_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .MoveSpeed_Buff_Value;
                        }
                        if (!Buffs.damage_toggle.IsNullOrDestroyed())
                        {
                            Buffs.damage_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Damage_Buff;
                        }
                        if (!Buffs.damage_slider.IsNullOrDestroyed())
                        {
                            Buffs.damage_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Damage_Buff_Value;
                        }
                        if (!Buffs.aoe_toggle.IsNullOrDestroyed())
                        {
                            Buffs.aoe_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_AoE_Buff;
                        }
                        if (!Buffs.aoe_slider.IsNullOrDestroyed())
                        {
                            Buffs.aoe_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .AoE_Buff_Value;
                        }
                        if (!Buffs.attackspeed_toggle.IsNullOrDestroyed())
                        {
                            Buffs.attackspeed_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_AttackSpeed_Buff;
                        }
                        if (!Buffs.attackspeed_slider.IsNullOrDestroyed())
                        {
                            Buffs.attackspeed_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .AttackSpeed_Buff_Value;
                        }
                        if (!Buffs.castingspeed_toggle.IsNullOrDestroyed())
                        {
                            Buffs.castingspeed_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_CastSpeed_Buff;
                        }
                        if (!Buffs.castingspeed_slider.IsNullOrDestroyed())
                        {
                            Buffs.castingspeed_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .CastSpeed_Buff_Value;
                        }
                        if (!Buffs.criticalchance_toggle.IsNullOrDestroyed())
                        {
                            Buffs.criticalchance_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_CriticalChance_Buff;
                        }
                        if (!Buffs.criticalchance_slider.IsNullOrDestroyed())
                        {
                            Buffs.criticalchance_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .CriticalChance_Buff_Value;
                        }
                        if (!Buffs.criticalmultiplier_toggle.IsNullOrDestroyed())
                        {
                            Buffs.criticalmultiplier_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_CriticalMultiplier_Buff;
                        }
                        if (!Buffs.criticalmultiplier_slider.IsNullOrDestroyed())
                        {
                            Buffs.criticalmultiplier_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .CriticalMultiplier_Buff_Value;
                        }
                        if (!Buffs.healthregen_toggle.IsNullOrDestroyed())
                        {
                            Buffs.healthregen_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_HealthRegen_Buff;
                        }
                        if (!Buffs.healthregen_slider.IsNullOrDestroyed())
                        {
                            Buffs.healthregen_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .HealthRegen_Buff_Value;
                        }
                        if (!Buffs.manaregen_toggle.IsNullOrDestroyed())
                        {
                            Buffs.manaregen_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_ManaRegen_Buff;
                        }
                        if (!Buffs.manaregen_slider.IsNullOrDestroyed())
                        {
                            Buffs.manaregen_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .ManaRegen_Buff_Value;
                        }
                        if (!Buffs.str_toggle.IsNullOrDestroyed())
                        {
                            Buffs.str_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Str_Buff;
                        }
                        if (!Buffs.str_slider.IsNullOrDestroyed())
                        {
                            Buffs.str_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Str_Buff_Value;
                        }
                        if (!Buffs.int_toggle.IsNullOrDestroyed())
                        {
                            Buffs.int_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Int_Buff;
                        }
                        if (!Buffs.int_slider.IsNullOrDestroyed())
                        {
                            Buffs.int_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Int_Buff_Value;
                        }
                        if (!Buffs.dex_toggle.IsNullOrDestroyed())
                        {
                            Buffs.dex_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Dex_Buff;
                        }
                        if (!Buffs.dex_slider.IsNullOrDestroyed())
                        {
                            Buffs.dex_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Dex_Buff_Value;
                        }
                        if (!Buffs.vit_toggle.IsNullOrDestroyed())
                        {
                            Buffs.vit_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Vit_Buff;
                        }
                        if (!Buffs.vit_slider.IsNullOrDestroyed())
                        {
                            Buffs.vit_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Vit_Buff_Value;
                        }
                        if (!Buffs.att_toggle.IsNullOrDestroyed())
                        {
                            Buffs.att_toggle.isOn = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Enable_Att_Buff;
                        }
                        if (!Buffs.att_slider.IsNullOrDestroyed())
                        {
                            Buffs.att_slider.value = Save_Manager
                                .instance
                                .data
                                .Character
                                .PermanentBuffs
                                .Att_Buff_Value;
                        }
                        result = true;
                    }
                }

                return result;
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

            public static void Update_Faction_Data()
            {
                Faction_Tracker.factions_favor_text.text =
                    Faction_Tracker.factions_favor_slider.value.ToString();
                Faction_Tracker.factions_rank_text.text =
                    Faction_Tracker.factions_rank_slider.value.ToString();
                Faction_Tracker.factions_reputation_text.text =
                    Faction_Tracker.factions_reputation_slider.value.ToString();
                if (Faction_Tracker.factions_dropdown.value > 0)
                {
                    Faction_Tracker.factions_go.active = true;
                }
                else
                {
                    Faction_Tracker.factions_go.active = false;
                }
                Faction_Tracker.selected_faction = Faction_Tracker.GetFaction();
            }

            public class Cheats
            {
                // BUG: For some reason the game always return true in action delegates
                public static Toggle godmode_toggle = null;
                public static readonly System.Action<bool> Godmode_Toggle_Action =
                    new System.Action<bool>(Set_Godmode_Enable);

                private static void Set_Godmode_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!godmode_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GodMode =
                            godmode_toggle.isOn;
                    }
                }

                public static Toggle twohanded_shield_toggle = null;
                public static readonly System.Action<bool> TwoHandedShield_Toggle_Action =
                    new System.Action<bool>(Set_TwoHandedShield_Enable);

                private static void Set_TwoHandedShield_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!twohanded_shield_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_TwoHandedWithShield =
                            twohanded_shield_toggle.isOn;
                    }
                }

                public static Toggle lowlife_toggle = null;
                public static readonly System.Action<bool> Lowlife_Toggle_Action =
                    new System.Action<bool>(Set_Lowlife_Enable);

                private static void Set_Lowlife_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!lowlife_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_LowLife =
                            lowlife_toggle.isOn;
                    }
                }

                public static Button max_blessings_button = null;
                public static readonly System.Action MaxOutBlessings_OnClick_Action =
                    new System.Action(Mods.Character.Character_Blessings.MaxOutBlessings);

                public static Button unlock_blessing_slots_button = null;
                public static readonly System.Action UnlockBlessingSlots_OnClick_Action =
                    new System.Action(Mods.Character.Character_Blessings.UnlockBlessingSlots);

                public static void BuildUnlockBlessingSlotsButton()
                {
                    if (
                        !unlock_blessing_slots_button.IsNullOrDestroyed()
                        || discover_blessings_button.IsNullOrDestroyed()
                    )
                        return;
                    var original = discover_blessings_button.gameObject;
                    var rect = original.GetComponent<RectTransform>();
                    var row = new GameObject("Blessing discovery actions");
                    var rowRect = row.AddComponent<RectTransform>();
                    row.transform.SetParent(original.transform.parent, false);
                    row.transform.SetSiblingIndex(original.transform.GetSiblingIndex());
                    rowRect.anchorMin = rect.anchorMin;
                    rowRect.anchorMax = rect.anchorMax;
                    rowRect.pivot = rect.pivot;
                    rowRect.anchoredPosition = rect.anchoredPosition;
                    rowRect.sizeDelta = rect.sizeDelta;
                    var layout = original.GetComponent<LayoutElement>();
                    if (!layout.IsNullOrDestroyed())
                    {
                        var copy = row.AddComponent<LayoutElement>();
                        copy.minWidth = layout.minWidth;
                        copy.minHeight = layout.minHeight;
                        copy.preferredWidth = layout.preferredWidth;
                        copy.preferredHeight = layout.preferredHeight;
                        copy.flexibleWidth = layout.flexibleWidth;
                        copy.flexibleHeight = layout.flexibleHeight;
                    }
                    var button = UnityEngine.Object.Instantiate(original, row.transform, false);
                    button.name = "Btn_Character_Cheats_UnlockBlessingSlots";
                    unlock_blessing_slots_button = button.GetComponent<Button>();
                    unlock_blessing_slots_button.onClick = new Button.ButtonClickedEvent();
                    foreach (var label in button.GetComponentsInChildren<Text>(true))
                        ModUI.Settings.LocaleRegistry.Apply(label, "Unlock Blessing Slots");
                    foreach (
                        var label in button.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(true)
                    )
                        label.text = "Unlock Blessing Slots";
                    foreach (var label in original.GetComponentsInChildren<Text>(true))
                        ModUI.Settings.LocaleRegistry.Apply(label, "Discover All Blessings");
                    foreach (
                        var label in original.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(true)
                    )
                        ModUI.Settings.LocaleRegistry.Apply(label, "Discover All Blessings");
                    original.transform.SetParent(row.transform, false);
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = new Vector2(.32f, 1f);
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var buttonRect = button.GetComponent<RectTransform>();
                    buttonRect.anchorMin = new Vector2(.68f, 0f);
                    buttonRect.anchorMax = Vector2.one;
                    buttonRect.offsetMin = buttonRect.offsetMax = Vector2.zero;
                    var maxButtonObject = UnityEngine.Object.Instantiate(
                        original,
                        row.transform,
                        false
                    );
                    maxButtonObject.name = "Btn_Character_Cheats_MaxOutBlessings";
                    max_blessings_button = maxButtonObject.GetComponent<Button>();
                    max_blessings_button.onClick = new Button.ButtonClickedEvent();
                    foreach (var label in maxButtonObject.GetComponentsInChildren<Text>(true))
                        ModUI.Settings.LocaleRegistry.Apply(label, "Max Out Blessings");
                    foreach (
                        var label in maxButtonObject.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(
                            true
                        )
                    )
                        ModUI.Settings.LocaleRegistry.Apply(label, "Max Out Blessings");
                    var maxRect = maxButtonObject.GetComponent<RectTransform>();
                    maxRect.anchorMin = new Vector2(.34f, 0f);
                    maxRect.anchorMax = new Vector2(.66f, 1f);
                    maxRect.offsetMin = maxRect.offsetMax = Vector2.zero;
                    foreach (var label in row.GetComponentsInChildren<Text>(true))
                    {
                        label.resizeTextForBestFit = true;
                        label.resizeTextMinSize = 9;
                        label.resizeTextMaxSize = 14;
                    }
                    maxButtonObject.SetActive(true);
                    button.SetActive(true);
                }

                public static Button choose_blessings_button = null;
                public static readonly System.Action ChooseBlessings_OnClick_Action =
                    new System.Action(Mods.Character.Character_Blessings.ChooseBlessings);

                public static void BuildChooseBlessingsButton(GameObject content)
                {
                    var oldRow = Functions.GetChild(content, "AllowChoosingBlessings");
                    if (oldRow.IsNullOrDestroyed() || discover_blessings_button.IsNullOrDestroyed())
                        return;
                    if (choose_blessings_button.IsNullOrDestroyed())
                    {
                        var replacement = UnityEngine.Object.Instantiate(
                            discover_blessings_button.gameObject,
                            oldRow.transform.parent,
                            false
                        );
                        replacement.name = "Btn_Character_Cheats_ChooseBlessings";
                        replacement.transform.SetSiblingIndex(oldRow.transform.GetSiblingIndex());
                        var sourceRect = oldRow.GetComponent<RectTransform>();
                        var targetRect = replacement.GetComponent<RectTransform>();
                        targetRect.anchorMin = sourceRect.anchorMin;
                        targetRect.anchorMax = sourceRect.anchorMax;
                        targetRect.pivot = sourceRect.pivot;
                        targetRect.anchoredPosition = sourceRect.anchoredPosition;
                        targetRect.sizeDelta = sourceRect.sizeDelta;
                        targetRect.localScale = Vector3.one;
                        var oldLayout = oldRow.GetComponent<LayoutElement>();
                        var newLayout = replacement.GetComponent<LayoutElement>();
                        if (!oldLayout.IsNullOrDestroyed())
                        {
                            if (newLayout.IsNullOrDestroyed())
                                newLayout = replacement.AddComponent<LayoutElement>();
                            newLayout.minWidth = oldLayout.minWidth;
                            newLayout.minHeight = oldLayout.minHeight;
                            newLayout.preferredWidth = oldLayout.preferredWidth;
                            newLayout.preferredHeight = oldLayout.preferredHeight;
                            newLayout.flexibleWidth = oldLayout.flexibleWidth;
                            newLayout.flexibleHeight = oldLayout.flexibleHeight;
                        }
                        choose_blessings_button = replacement.GetComponent<Button>();
                        choose_blessings_button.onClick = new Button.ButtonClickedEvent();
                        foreach (var label in replacement.GetComponentsInChildren<Text>(true))
                            ModUI.Settings.LocaleRegistry.Apply(label, "Choose Blessings");
                        foreach (
                            var label in replacement.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(
                                true
                            )
                        )
                            label.text = "Choose Blessings";
                        replacement.SetActive(true);
                    }
                    oldRow.SetActive(false);
                }

                public static Toggle unlock_all_idols = null;
                public static readonly System.Action<bool> UnlockAllIdols_Toggle_Action =
                    new System.Action<bool>(Set_UnlockAllIdols_Enable);

                private static void Set_UnlockAllIdols_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!unlock_all_idols.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_UnlockAllIdolsSlots =
                            unlock_all_idols.isOn;
                        Mods.Character.Character_UnlockAllIdols.Update();
                    }
                }

                public static Toggle autoPot_toggle = null;
                public static readonly System.Action<bool> AutoPot_Toggle_Action =
                    new System.Action<bool>(Set_AutoPot_Enable);

                private static void Set_AutoPot_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!autoPot_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_AutoPot =
                            autoPot_toggle.isOn;
                    }
                }

                public static Slider autopot_slider = null;

                public static Toggle density_toggle = null;
                public static readonly System.Action<bool> Density_Toggle_Action =
                    new System.Action<bool>(Set_Density_Enable);

                private static void Set_Density_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!density_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_DensityMultiplier =
                            density_toggle.isOn;
                    }
                }

                public static Slider density_slider = null;

                public static Toggle experience_toggle = null;
                public static readonly System.Action<bool> Experience_Toggle_Action =
                    new System.Action<bool>(Set_Experience_Enable);

                private static void Set_Experience_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!experience_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ExperienceMultiplier =
                            experience_toggle.isOn;
                    }
                }

                public static Slider experience_slider = null;

                public static Toggle ability_toggle = null;
                public static readonly System.Action<bool> Ability_Toggle_Action =
                    new System.Action<bool>(Set_Ability_Enable);

                private static void Set_Ability_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!ability_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_AbilityMultiplier =
                            ability_toggle.isOn;
                    }
                }

                public static Slider ability_slider = null;

                public static Toggle favor_toggle = null;
                public static readonly System.Action<bool> Favor_Toggle_Action =
                    new System.Action<bool>(Set_Favor_Enable);

                private static void Set_Favor_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!favor_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_FavorMultiplier =
                            favor_toggle.isOn;
                    }
                }

                public static Slider favor_slider = null;

                public static Toggle memoryamber_toggle = null;
                public static readonly System.Action<bool> memoryamber_Toggle_Action =
                    new System.Action<bool>(Set_memoryamber_Enable);

                private static void Set_memoryamber_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!memoryamber_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_MemoryAmberMultiplier =
                            memoryamber_toggle.isOn;
                    }
                }

                public static void PrepareMemoryAmberMultiplier()
                {
                    if (memoryamber_slider.IsNullOrDestroyed())
                    {
                        return;
                    }
                    memoryamber_slider.wholeNumbers = true;
                    memoryamber_slider.minValue = 1f;
                    memoryamber_slider.maxValue = 255f;

                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        uint value = Save_Manager
                            .instance
                            .data
                            .Character
                            .Cheats
                            .MemoryAmberMultiplier;
                        if (value < 1)
                        {
                            value = 1;
                        }
                        if (value > 255)
                        {
                            value = 255;
                        }
                        Save_Manager.instance.data.Character.Cheats.MemoryAmberMultiplier = value;
                        memoryamber_slider.value = value;
                    }
                }

                public static Slider memoryamber_slider = null;

                public static Toggle itemdropmultiplier_toggle = null;
                public static readonly System.Action<bool> ItemDropMulti_Toggle_Action =
                    new System.Action<bool>(Set_ItemDropMulti_Enable);

                private static void Set_ItemDropMulti_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!itemdropmultiplier_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ItemDropMultiplier =
                            itemdropmultiplier_toggle.isOn;
                    }
                }

                public static Slider itemdropmultiplier_slider = null;

                public static Toggle itemdropchance_toggle = null;
                public static readonly System.Action<bool> ItemDropChance_Toggle_Action =
                    new System.Action<bool>(Set_ItemDropChance_Enable);

                private static void Set_ItemDropChance_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!itemdropchance_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ItemDropChance =
                            itemdropchance_toggle.isOn;
                    }
                }

                public static Slider itemdropchance_slider = null;

                public static Toggle golddropmultiplier_toggle = null;
                public static readonly System.Action<bool> GoldMulti_Toggle_Action =
                    new System.Action<bool>(Set_GoldMulti_Enable);

                private static void Set_GoldMulti_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!golddropmultiplier_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GoldDropMultiplier =
                            golddropmultiplier_toggle.isOn;
                    }
                }

                public static Slider golddropmultiplier_slider = null;

                public static Toggle golddropchance_toggle = null;
                public static readonly System.Action<bool> GoldChance_Toggle_Action =
                    new System.Action<bool>(Set_GoldChance_Enable);

                private static void Set_GoldChance_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!golddropchance_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GoldDropChance =
                            golddropchance_toggle.isOn;
                    }
                }

                public static Slider golddropchance_slider = null;

                public static Toggle waypoints_toggle = null;
                public static readonly System.Action<bool> Waypoints_Toggle_Action =
                    new System.Action<bool>(Set_Waypoints_Enable);

                private static void Set_Waypoints_Enable(bool enable)
                {
                    if (
                        (!Save_Manager.instance.IsNullOrDestroyed())
                        && (!waypoints_toggle.IsNullOrDestroyed())
                    )
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_WaypointsUnlock =
                            waypoints_toggle.isOn;
                    }
                }

                public static Button level_once_button = null;
                public static readonly System.Action LevelUpOnce_OnClick_Action = new System.Action(
                    LevelUpOnce_Click
                );

                public static void LevelUpOnce_Click()
                {
                    Mods.Character.Character_Level.LevelUpOnce();
                }

                public static Button level_max_button = null;
                public static readonly System.Action LevelUpMax_OnClick_Action = new System.Action(
                    LevelUpMax_Click
                );

                public static void LevelUpMax_Click()
                {
                    Mods.Character.Character_Level.LevelUptoMax();
                }

                public static Button complete_quest_button = null;
                public static readonly System.Action CompleteQuest_OnClick_Action =
                    new System.Action(CompleteQuest_Click);

                public static void CompleteQuest_Click()
                {
                    Mods.Character.Character_MainQuest.Complete();
                }

                public static Button masterie_buttons = null;
                public static readonly System.Action Masteries_OnClick_Action = new System.Action(
                    Masteries_Click
                );

                public static void Masteries_Click()
                {
                    Mods.Character.Character_Masteries.ResetChooseMasterie();
                }

                public static Text masterie_text = null;

                public static Button add_runes_button = null;
                public static readonly System.Action AddRunes_OnClick_Action = new System.Action(
                    AddRunes_Click
                );

                public static void AddRunes_Click()
                {
                    Mods.Character.Character_Materials.GetAllRunesX99();
                }

                public static Button add_glyphs_button = null;
                public static readonly System.Action AddGlyphs_OnClick_Action = new System.Action(
                    AddGlyphs_Click
                );

                public static void AddGlyphs_Click()
                {
                    Mods.Character.Character_Materials.GetAllGlyphsX99();
                }

                public static Button add_shards_button = null;
                public static readonly System.Action AddAffixs_OnClick_Action = new System.Action(
                    AddAffixs_Click
                );

                public static void AddAffixs_Click()
                {
                    Mods.Character.Character_Materials.GetAllShardsX10();
                }

                public static Button add_ancient_bones_button = null;
                public static readonly System.Action AddAncientBones_OnClick_Action =
                    new System.Action(AddAncientBones_Click);

                public static void AddAncientBones_Click()
                {
                    Mods.Character.Character_Materials.AddAncientBones(10000);
                }

                public static Button discover_blessings_button = null;
                public static readonly System.Action DiscoverAllBlessings_OnClick_Action =
                    new System.Action(DiscoverAllBlessings_Click);

                public static void DiscoverAllBlessings_Click()
                {
                    Mods.Character.Character_Blessings.DiscoverAllBlessings();
                }
            }

            public class Data
            {
                public static Dropdown class_dropdown = null;
                public static Toggle died_toggle = null;
                public static Toggle hardcore_toggle = null;
                public static Toggle masochist_toggle = null;
                public static Toggle portal_toggle = null;
                public static Toggle solo_toggle = null;
                public static Slider deaths_slider = null;
                public static Slider lantern_slider = null;
                public static GameObject soul_row = null;
                public static Text soul_text = null;
                public static Slider soul_slider = null;
                public static Button soul_add_button = null;
                public static readonly System.Action SoulEmbers_Add_Action = new System.Action(
                    AddSoulEmbers
                );
                public static Toggle weaver_points_toggle = null;
                public static Text weaver_points_text = null;
                public static Slider weaver_points_slider = null;
                public static Toggle weaver_multiplier_toggle = null;
                public static Slider weaver_multiplier_slider = null;
                public static bool suppressWeaverSlider = false;
                public static readonly System.Action<bool> weaver_points_toggle_Action =
                    new System.Action<bool>(SetWeaverTreePointsEnabled);

                public static void SetupSoulEmberControls()
                {
                    if (!soul_row.IsNullOrDestroyed())
                        soul_row.SetActive(false);
                    if (
                        !soul_add_button.IsNullOrDestroyed()
                        || Cheats.add_ancient_bones_button.IsNullOrDestroyed()
                    )
                        return;
                    var clone = Object.Instantiate(
                        Cheats.add_ancient_bones_button.gameObject,
                        Cheats.add_ancient_bones_button.transform.parent,
                        false
                    );
                    clone.name = "Btn_Character_Cheats_AddSoulEmbers";
                    soul_add_button = clone.GetComponent<Button>();
                    soul_add_button.onClick = new Button.ButtonClickedEvent();
                    PrepareFullWidthActionButton(clone, soul_add_button, "Add 1,000 Soul Embers");
                    clone.SetActive(true);
                }

                public static void AddSoulEmbers()
                {
                    if (Mods.Character.Character_SoulEmbers.Add(1000))
                        UpdateSoulEmberBalance();
                }

                public static void UpdateSoulEmberBalance()
                {
                    int balance = Mods.Character.Character_SoulEmbers.GetBalance();
                    if (balance >= 0 && !soul_text.IsNullOrDestroyed())
                    {
                        soul_text.gameObject.SetActive(true);
                        soul_text.text = balance.ToString();
                    }
                }

                public static void EnsureWeaverRange()
                {
                    if (suppressWeaverSlider || weaver_points_slider.IsNullOrDestroyed())
                    {
                        return;
                    }
                    int max = Mods.Factions.TheWoven.Faction_Woven_TreePoints.SliderMax;
                    if (
                        weaver_points_slider.wholeNumbers
                        && weaver_points_slider.minValue == 0f
                        && (int)weaver_points_slider.maxValue == max
                    )
                    {
                        return;
                    }
                    suppressWeaverSlider = true;
                    try
                    {
                        weaver_points_slider.wholeNumbers = true;
                        weaver_points_slider.minValue = 0f;
                        weaver_points_slider.maxValue = max;
                    }
                    finally
                    {
                        suppressWeaverSlider = false;
                    }
                }

                public static void SetWeaverTreePointsEnabled(bool on)
                {
                    if (suppressWeaverSlider || Save_Manager.instance.IsNullOrDestroyed())
                    {
                        return;
                    }
                    if (!weaver_points_toggle.IsNullOrDestroyed())
                    {
                        on = weaver_points_toggle.isOn;
                    }
                    Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints = on;
                    EnsureWeaverRange();
                    if (on && !weaver_points_slider.IsNullOrDestroyed())
                    {
                        Save_Manager.instance.data.Factions.TheWoven.TreePoints = (int)
                            weaver_points_slider.value;
                    }
                    if (
                        !weaver_points_text.IsNullOrDestroyed()
                        && !weaver_points_slider.IsNullOrDestroyed()
                    )
                    {
                        weaver_points_text.text = ((int)weaver_points_slider.value).ToString();
                    }
                    if (on)
                    {
                        Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                    }
                    else
                    {
                        Mods.Factions.TheWoven.Faction_Woven_TreePoints.ReleaseToRealPoints();
                    }
                }

                public static void SetWeaverTreePoints(float value)
                {
                    if (suppressWeaverSlider || Save_Manager.instance.IsNullOrDestroyed())
                    {
                        return;
                    }
                    if (!weaver_points_toggle.IsNullOrDestroyed() && !weaver_points_toggle.isOn)
                    {
                        return;
                    }
                    EnsureWeaverRange();
                    int points = (int)value;
                    int max = Mods.Factions.TheWoven.Faction_Woven_TreePoints.SliderMax;
                    if (points > max)
                    {
                        points = max;
                    }
                    Save_Manager.instance.data.Factions.TheWoven.TreePoints = points;
                    Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints = true;
                    if (!weaver_points_text.IsNullOrDestroyed())
                    {
                        weaver_points_text.text = points.ToString();
                    }
                    Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                }

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
                public static readonly System.Action Save_OnClick_Action = new System.Action(
                    Save_Click
                );

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
                public static Il2CppLE.Factions.Faction selected_faction = null;

                public static Il2CppLE.Factions.Faction GetFaction()
                {
                    Il2CppLE.Factions.Faction faction = null;
                    if (!Refs_Manager.faction_tracker.IsNullOrDestroyed())
                    {
                        foreach (
                            Il2CppSystem.Collections.Generic.KeyValuePair<
                                Il2CppLE.Factions.FactionID,
                                Il2CppLE.Factions.Faction
                            > values in Refs_Manager.faction_tracker.factions
                        )
                        {
                            if (
                                (
                                    (values.Key == Il2CppLE.Factions.FactionID.CircleOfFortune)
                                    && (factions_dropdown.value == 1)
                                )
                                || (
                                    (values.Key == Il2CppLE.Factions.FactionID.MerchantsGuild)
                                    && (factions_dropdown.value == 2)
                                )
                                || (
                                    (values.Key == Il2CppLE.Factions.FactionID.ForgottenKnights)
                                    && (factions_dropdown.value == 3)
                                )
                                || (
                                    (values.Key == Il2CppLE.Factions.FactionID.TheWeaver)
                                    && (factions_dropdown.value == 4)
                                )
                            )
                            {
                                faction = values.Value;
                            }
                        }
                    }

                    return faction;
                }

                public static Dropdown factions_dropdown = null;
                public static GameObject factions_go = null;
                public static Text factions_favor_text = null;
                public static Slider factions_favor_slider = null;
                public static readonly System.Action<float> factions_favor_slider_Action =
                    new System.Action<float>(factions_favor);

                public static void factions_favor(float f)
                {
                    int result = System.Convert.ToInt32(factions_favor_slider.value);
                    if (!factions_favor_text.IsNullOrDestroyed())
                    {
                        factions_favor_text.text = result.ToString();
                    }
                }

                public static Button factions_gain_favor_button = null;
                public static readonly System.Action factions_gain_favor_OnClick_Action =
                    new System.Action(factions_gain_favor_Click);

                public static void factions_gain_favor_Click()
                {
                    if (
                        (!factions_favor_slider.IsNullOrDestroyed())
                        && (!selected_faction.IsNullOrDestroyed())
                    )
                    {
                        if (!selected_faction.hasEverJoined)
                        {
                            selected_faction.Join(true);
                        }
                        if (!selected_faction.IsMember)
                        {
                            selected_faction.IsMember = true;
                        }

                        int added_favor = Il2CppSystem.Convert.ToInt32(factions_favor_slider.value);
                        int max_favor = 999999;
                        int max_added_favor = max_favor - selected_faction.Favor;
                        if (added_favor > max_added_favor)
                        {
                            added_favor = max_added_favor;
                        }

                        using (Mods.UI.SessionGainCounters.SuppressManualGrants())
                            selected_faction.GainFavor(added_favor, false);
                        selected_faction.SaveAndSync(false);
                    }
                    else
                    {
                        Main.logger_instance.Error("Faction not found");
                    }
                }

                public static Button factions_set_favor_button = null;
                public static readonly System.Action factions_set_favor_OnClick_Action =
                    new System.Action(factions_set_favor_Click);

                public static void factions_set_favor_Click()
                {
                    if (
                        (!factions_favor_slider.IsNullOrDestroyed())
                        && (!selected_faction.IsNullOrDestroyed())
                    )
                    {
                        if (!selected_faction.hasEverJoined)
                        {
                            selected_faction.Join(true);
                        }
                        if (!selected_faction.IsMember)
                        {
                            selected_faction.IsMember = true;
                        }

                        int added_favor = Il2CppSystem.Convert.ToInt32(factions_favor_slider.value);
                        int max_favor = 999999;
                        int max_added_favor = max_favor - selected_faction.Favor;
                        if (added_favor > max_added_favor)
                        {
                            added_favor = max_added_favor;
                        }

                        using (Mods.UI.SessionGainCounters.SuppressManualGrants())
                            selected_faction.Favor = added_favor;
                        selected_faction.SaveAndSync(false);
                    }
                    else
                    {
                        Main.logger_instance.Error("Faction not found");
                    }
                }

                public static Text factions_rank_text = null;
                public static Slider factions_rank_slider = null;
                public static readonly System.Action<float> factions_rank_slider_Action =
                    new System.Action<float>(factions_rank);

                public static void factions_rank(float f)
                {
                    int result = System.Convert.ToInt32(factions_rank_slider.value);
                    if (!factions_rank_text.IsNullOrDestroyed())
                    {
                        factions_rank_text.text = result.ToString();
                    }
                }

                public static Button factions_set_rank_button = null;
                public static readonly System.Action factions_set_rank_OnClick_Action =
                    new System.Action(factions_set_rank_Click);

                public static void factions_set_rank_Click()
                {
                    if (
                        (!factions_rank_slider.IsNullOrDestroyed())
                        && (!selected_faction.IsNullOrDestroyed())
                    )
                    {
                        if (!selected_faction.hasEverJoined)
                        {
                            selected_faction.Join(true);
                        }
                        if (!selected_faction.IsMember)
                        {
                            selected_faction.IsMember = true;
                        }

                        int max_rank = 12;
                        if ((factions_dropdown.value == 3) || (factions_dropdown.value == 4))
                        {
                            max_rank = 10;
                        }
                        int rank = Il2CppSystem.Convert.ToInt32(factions_rank_slider.value);
                        if (rank > max_rank)
                        {
                            rank = max_rank;
                        }

                        selected_faction.Rank = rank;
                        selected_faction.SaveAndSync(false);
                    }
                    else
                    {
                        Main.logger_instance.Error("Faction not found");
                    }
                }

                public static Text factions_reputation_text = null;
                public static Slider factions_reputation_slider = null;
                public static readonly System.Action<float> factions_reputation_slider_Action =
                    new System.Action<float>(factions_reputation);

                public static void factions_reputation(float f)
                {
                    int result = System.Convert.ToInt32(factions_reputation_slider.value);
                    if (!factions_reputation_text.IsNullOrDestroyed())
                    {
                        factions_reputation_text.text = result.ToString();
                    }
                }

                public static Button factions_set_reputation_button = null;
                public static readonly System.Action factions_set_reputation_OnClick_Action =
                    new System.Action(factions_set_reputation_Click);

                public static void factions_set_reputation_Click()
                {
                    if (
                        (!factions_reputation_slider.IsNullOrDestroyed())
                        && (!selected_faction.IsNullOrDestroyed())
                    )
                    {
                        if (!selected_faction.hasEverJoined)
                        {
                            selected_faction.Join(true);
                        }
                        if (!selected_faction.IsMember)
                        {
                            selected_faction.IsMember = true;
                        }

                        int favor = Il2CppSystem.Convert.ToInt32(factions_reputation_slider.value);
                        int total_reputation = selected_faction.TotalReputation;
                        if (favor > total_reputation)
                        {
                            favor = total_reputation;
                        }

                        selected_faction.Reputation = favor;
                        selected_faction.SaveAndSync(false);
                    }
                    else
                    {
                        Main.logger_instance.Error("Faction not found");
                    }
                }
            }

            public class Buffs
            {
                public static Toggle enable_mod = null;

                public static Toggle movespeed_toggle = null;
                public static Slider movespeed_slider = null;

                public static Toggle damage_toggle = null;
                public static Slider damage_slider = null;

                public static Toggle aoe_toggle = null;
                public static Slider aoe_slider = null;

                public static Toggle attackspeed_toggle = null;
                public static Slider attackspeed_slider = null;

                public static Toggle castingspeed_toggle = null;
                public static Slider castingspeed_slider = null;

                public static Toggle criticalchance_toggle = null;
                public static Slider criticalchance_slider = null;

                public static Toggle criticalmultiplier_toggle = null;
                public static Slider criticalmultiplier_slider = null;

                public static Toggle healthregen_toggle = null;
                public static Slider healthregen_slider = null;

                public static Toggle manaregen_toggle = null;
                public static Slider manaregen_slider = null;

                public static Toggle str_toggle = null;
                public static Slider str_slider = null;

                public static Toggle int_toggle = null;
                public static Slider int_slider = null;

                public static Toggle dex_toggle = null;
                public static Slider dex_slider = null;

                public static Toggle vit_toggle = null;
                public static Slider vit_slider = null;

                public static Toggle att_toggle = null;
                public static Slider att_slider = null;

                static readonly string[] BuffPanelOrder =
                {
                    "MoveSpeed",
                    "Damage",
                    "AreaOfEffect",
                    "AttackSpeed",
                    "CastingSpeed",
                    "CriticalChance",
                    "CriticalMultiplier",
                    "HealthRegen",
                    "ManaRegen",
                    "Strenght",
                    "Intelligence",
                    "Dexterity",
                    "Vitality",
                    "Attunement",
                };

                public static void EnsureAreaOfEffectBuffRow(GameObject character_buffs_content)
                {
                    if (character_buffs_content.IsNullOrDestroyed())
                    {
                        return;
                    }

                    GameObject source = Functions.GetChild(
                        character_buffs_content,
                        "Damage",
                        false
                    );
                    if (source.IsNullOrDestroyed())
                    {
                        return;
                    }

                    SettingRow.AddMultiplier(
                        character_buffs_content,
                        "Damage",
                        "AreaOfEffect",
                        "Toggle_Character_Buffs_Damage",
                        "Toggle_Character_Buffs_AreaOfEffect",
                        "Slider_Character_Buffs_Damage",
                        "Slider_Character_Buffs_AreaOfEffect",
                        "Area of Effect"
                    );

                    GameObject row = Functions.GetChild(
                        character_buffs_content,
                        "AreaOfEffect",
                        false
                    );
                    if (row.IsNullOrDestroyed())
                    {
                        return;
                    }

                    foreach (Transform child in row.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name != "Slider_Character_Buffs_AreaOfEffect")
                        {
                            continue;
                        }
                        Slider slider = child.GetComponent<Slider>();
                        if (!slider.IsNullOrDestroyed())
                        {
                            slider.minValue = 0f;
                            slider.maxValue = 100f;
                        }
                    }

                    foreach (Text label in row.GetComponentsInChildren<Text>(true))
                    {
                        if (label.gameObject.name == "Value")
                        {
                            continue;
                        }
                        ModUI.Settings.LocaleRegistry.Apply(label, "Area of Effect");
                    }
                    foreach (
                        Il2CppTMPro.TMP_Text label in row.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(
                            true
                        )
                    )
                    {
                        if (label.gameObject.name == "Value")
                        {
                            continue;
                        }
                        ModUI.Settings.LocaleRegistry.Apply(label, "Area of Effect");
                    }

                    LayoutBuffPanels(character_buffs_content);
                }

                static void LayoutBuffPanels(GameObject content)
                {
                    var rows = new System.Collections.Generic.List<RectTransform>();
                    foreach (string panelName in BuffPanelOrder)
                    {
                        GameObject panel = Functions.GetChild(content, panelName, false);
                        if (panel.IsNullOrDestroyed())
                        {
                            continue;
                        }
                        RectTransform rect = panel.GetComponent<RectTransform>();
                        if (!rect.IsNullOrDestroyed())
                        {
                            rows.Add(rect);
                        }
                    }
                    if (rows.Count < 2)
                    {
                        return;
                    }

                    float step = rows[1].anchoredPosition.y - rows[0].anchoredPosition.y;
                    if (Mathf.Abs(step) < 0.01f)
                    {
                        float rowHeight = Mathf.Abs(rows[0].rect.height);
                        if (rowHeight < 1f)
                        {
                            rowHeight = Mathf.Abs(rows[0].sizeDelta.y);
                        }
                        if (rowHeight < 1f)
                        {
                            rowHeight = 32f;
                        }
                        step = -rowHeight;
                    }

                    float baseY = rows[0].anchoredPosition.y;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        RectTransform rect = rows[i];
                        rect.SetSiblingIndex(i);
                        Vector2 pos = rect.anchoredPosition;
                        rect.anchoredPosition = new Vector2(pos.x, baseY + (i * step));
                    }

                    RectTransform parent = content.GetComponent<RectTransform>();
                    if (!parent.IsNullOrDestroyed())
                    {
                        float neededHeight = Mathf.Abs(step) * rows.Count;
                        if (parent.sizeDelta.y < neededHeight)
                        {
                            parent.sizeDelta = new Vector2(parent.sizeDelta.x, neededHeight);
                        }
                    }
                }
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
                if (content_obj.IsNullOrDestroyed())
                {
                    return;
                }

                GameObject items_craft_content = Functions.GetViewportContent(
                    content_obj,
                    "Items_Craft",
                    "Items_Craft_Content"
                );
                ModUI.Settings.InfiniteForgingPotentialControls.Bind(
                    content_obj,
                    items_craft_content
                );
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
