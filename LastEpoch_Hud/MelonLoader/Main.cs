using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Il2Cpp;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LastEpoch_Hud
{
    public class Main : MelonLoader.MelonMod
    {
        public static MelonLoader.MelonLogger.Instance logger_instance = null;
        public const string company_name = "Eleventh Hour Games";
        public const string game_name = "Last Epoch";
        public const string mod_name = "LastEpoch_Hud";
        #if COMPAT15_MINIMAL
        public const string mod_version = "4.4.7-LE1.5-exp1";
#else
        public const string mod_version = "4.4.7"; //LastEpoch 1.3
#endif
        public static bool debug = false;

        public override void OnInitializeMelon()
        {
            logger_instance = LoggerInstance;
#if COMPAT15_MINIMAL
            Main.logger_instance?.Msg("[Compat15:LOAD] Experimental Last Epoch 1.5 minimal mode");
            PatchCompat15<Scripts.Mods.Character.Character_Experience_Multiplier.ExperienceTracker_GainExp>("Character XP");
            PatchCompat15<Scripts.Mods.Character.Character_Ability_Experience_Multiplier.ExperienceTracker_GainExp>("Skill XP");
            PatchCompat15<Scripts.Mods.Character.Character_Favor_Experience_Multiplier.ExperienceTracker_GainExp>("Favor XP");
            PatchCompat15<Scripts.Mods.Items.Items_AutoPickup_Items.GroundItemManager_dropItemForPlayer>("Basic Auto Pickup");
#else
            Scripts.Mods.Localization.LocalizationOverride.RegisterAll();
#endif
        }
#if COMPAT15_MINIMAL
        private void PatchCompat15<T>(string name)
        {
            try
            {
                HarmonyInstance.CreateClassProcessor(typeof(T)).Patch();
                Main.logger_instance?.Msg("[Compat15:PATCH] OK " + name);
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("[Compat15:PATCH] FAIL " + name + ": " + ex);
            }
        }
#endif
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Scenes.SceneName = SceneManager.GetActiveScene().name;
        }
        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            Scenes.SceneName = SceneManager.GetActiveScene().name;
        }
        static bool diagnosticsAttachAttempted = false;
        public override void OnLateUpdate()
        {
            if ((!Base.Initializing) && (!Base.Initialized)) { Base.Init(); }
            if (!diagnosticsAttachAttempted
                && Scripts.ModUI.SaveManager.instance != null
                && Scripts.ModUI.SaveManager.instance.initialized)
            {
                diagnosticsAttachAttempted = true;
                Scripts.Mods.Diagnostics.DiagnosticsDumper.AttachIfEnabled();
            }
        }
        public override void OnApplicationQuit()
        {
            Caching.ClearCache();
        }
    }
    public class Locales
    {
        public enum Selected
        {
            Unknow,
            English,
            French,
            Korean,
            German,
            Russian,
            Polish,
            Portuguese,
            Chinese,
            Spanish
        }

        public static Selected current = Selected.Unknow;
        private static readonly string dictionary_path = Application.dataPath + "/../Mods/" + Main.mod_name + "/Locales";
        public static string dictionnary_filename = "";
        public static Dictionary<string, string> current_dictionary = null;
        public static bool update = false;
        //public static bool debug_text = false; //used to generate default json from prefab
        //public static List<string>? debug_json;
        public static char[] igrone_str = { '+', '%', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        [HarmonyPatch(typeof(Localization), "get_Locale")]
        public class Localization_get_Locale
        {
            [HarmonyPostfix]
            static void Postfix(string __result)
            {
                Selected backup = current;
                current = Selected.Unknow;
                switch (__result)
                {
                    case "English (en)": { current = Selected.English; dictionnary_filename = "en"; break; }
                    case "French (fr)": { current = Selected.French; dictionnary_filename = "fr"; break; }
                    case "Korean (ko)": { current = Selected.Korean; dictionnary_filename = "ko"; break; }
                    case "German (Germany) (de-DE)": { current = Selected.German; dictionnary_filename = "de"; break; }
                    case "Russian (ru)": { current = Selected.Russian; dictionnary_filename = "ru"; break; }
                    case "Polish (pl)": { current = Selected.Polish; dictionnary_filename = "pl"; break; }
                    case "Portuguese (pt)": { current = Selected.Portuguese; dictionnary_filename = "pt"; break; }
                    case "Chinese (Simplified) (zh)": { current = Selected.Chinese; dictionnary_filename = "zh"; break; }
                    case "Spanish (Spain) (es-ES)": { current = Selected.Spanish; dictionnary_filename = "es"; break; }
                }
                if (current != backup)
                {
                    if (backup == Selected.Unknow) { Main.logger_instance?.Msg("Locale initialized to " + current.ToString()); }
                    else { Main.logger_instance?.Msg("Locale change to " + current.ToString()); }
                    update = LoadDictionary();
                }
            }
        }

        private static bool LoadDictionary()
        {
            string full_path = dictionary_path + "/" + dictionnary_filename + Extensions.json;
            if ((Directory.Exists(dictionary_path)) && (File.Exists(full_path)))
            {
                current_dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(full_path));
                return true;
            }
            else
            {
                Main.logger_instance?.Error("Dictionnary not found for " + current.ToString() + " (" + full_path + ")");
                Main.logger_instance?.Error("If you want to use locale for hud, copy " + dictionary_path + "/base" + Extensions.json + " to " + dictionnary_filename + Extensions.json + ", then edit this file");
                return false;
            }
        }
    }
    public class Base
    {
        public static readonly string base_object_name = "BaseHud";
        public static bool Initialized = false;
        public static bool Initializing = false;

        public static void Init()
        {
            if (Initializing || Initialized) { return; }
            Initializing = true;
            try
            {
                Main.logger_instance?.Msg("[Compat15:BOOT] Creating BaseHud");
                GameObject base_object = Object.Instantiate(new GameObject(name: base_object_name), Vector3.zero, Quaternion.identity);
                Object.DontDestroyOnLoad(base_object);

                TryAdd<Scripts.Refs_Manager>(base_object, "Refs_Manager");
                TryAdd<Scripts.Save_Manager>(base_object, "Save_Manager");
                TryAdd<Scripts.Hud_Manager>(base_object, "Hud_Manager");
                TryAdd<Scripts.ModUI.SaveManager>(base_object, "ModUI.SaveManager");
                TryAdd<Scripts.Mods_Manager>(base_object, "Mods_Manager");
#if !COMPAT15_MINIMAL
                TryAdd<Scripts.VirtualKeyboard>(base_object, "VirtualKeyboard");
#endif
                Initialized = true;
                Main.logger_instance?.Msg("[Compat15:BOOT] BaseHud initialized");
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("[Compat15:BOOT] Base initialization failed: " + ex);
            }
            finally
            {
                Initializing = false;
            }
        }

        private static void TryAdd<T>(GameObject target, string name) where T : Component
        {
            try
            {
                target.AddComponent<T>();
                Main.logger_instance?.Msg("[Compat15:BOOT] OK " + name);
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("[Compat15:BOOT] FAIL " + name + ": " + ex);
            }
        }
    }
    public class Scenes
    {
        public static string SceneName = "";
        private static readonly string[] SceneMenuNames = { "ClientSplash", "PersistentUI", "Login", "CharacterSelectScene" };

        public static bool IsGameScene()
        {
            if (!string.IsNullOrWhiteSpace(SceneName) && (!SceneMenuNames.Contains(SceneName))) { return true; }
            else { return false; }
        }
        public static bool IsCharacterSelection()
        {
            if (!string.IsNullOrWhiteSpace(SceneName) && (SceneName.Contains(SceneMenuNames[3]))) { return true; }
            else { return false; }
        }
    }
    public class Extensions
    {
        public static readonly string jpg = ".jpg";
        public static readonly string png = ".png";
        public static readonly string json = ".json";
        public static readonly string prefab = ".prefab";
    }
    public static class Functions
    {
        public static bool IsNullOrDestroyed(this object obj)
        {
            try
            {
                if (obj == null) { return true; }
                else if (obj is Object unityObj && !unityObj) { return true; }
                return false;
            }
            catch { return true; }
        }
        public static GameObject GetChild(GameObject obj, string name)
        {
            GameObject result = null;
            if (!obj.IsNullOrDestroyed())
            {
                bool found = false;
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    string obj_name = obj.transform.GetChild(i).gameObject.name;
                    if (obj_name == name)
                    {
                        result = obj.transform.GetChild(i).gameObject;
                        found = true;
                        break;
                    }
                }
                string[] no_bug = { "skin", "Modifier Button", "legendary_icon", "quad_stash_row", "Hud_VirtualKeyboard" };
                if ((!found) && (!no_bug.Contains(name))) { Main.logger_instance?.Error("Functions.GetChild, Child : " + name + " not Found"); }
            }
            else { Main.logger_instance.Error("GetChild(" + name + ") : Obj is null"); }

            return result;
        }
        public static List<GameObject> GetAllChild(GameObject obj)
        {
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                //string obj_name = obj.transform.GetChild(i).gameObject.name;
                result.Add(obj.transform.GetChild(i).gameObject);
            }

            return result;
        }
        public static GameObject GetViewportContent(GameObject obj, string panel_name, string panel_content_name)
        {
            GameObject result = null;
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed())
            {
                GameObject content = GetChild(panel, panel_content_name);
                if (!content.IsNullOrDestroyed())
                {
                    GameObject viewport = GetChild(content, "Viewport");
                    if (!viewport.IsNullOrDestroyed()) { result = GetChild(viewport, "Content"); }
                }
            }

            return result;
        }
        public static Toggle Get_ToggleInPanel(GameObject obj, string panel_name, string obj_name)
        {
            Toggle result = null; // new Toggle();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed()) { result = Functions.GetChild(panel, obj_name).GetComponent<Toggle>(); }

            return result;
        }
        public static Text Get_TextInPanel(GameObject obj, string panel_name, string obj_name)
        {
            Text result = null;// new Text();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed()) { result = Functions.GetChild(panel, obj_name).GetComponent<Text>(); }

            return result;
        }
        public static Slider Get_SliderInPanel(GameObject obj, string panel_name, string obj_name)
        {
            Slider result = null; // new Slider();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed()) { result = Functions.GetChild(panel, obj_name).GetComponent<Slider>(); }

            return result;
        }
        public static Button Get_ButtonInPanel(GameObject obj, string obj_name)
        {
            Button result = null; // new Button();
            GameObject panel = GetChild(obj, obj_name);
            if (!panel.IsNullOrDestroyed()) { result = panel.GetComponent<Button>(); }

            return result;
        }
        public static Text Get_TextInToggle(GameObject obj, string panel_name, string toggle_name, string obj_name)
        {
            Text result = null; // new Text();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed())
            {
                GameObject toogle = GetChild(panel, toggle_name);
                if (!toogle.IsNullOrDestroyed())
                {
                    result = GetChild(toogle, obj_name).GetComponent<Text>();
                }
            }

            return result;
        }
        public static Text Get_TextInButton(GameObject obj, string button_name, string text_name)
        {
            Text result = null; // new Text();
            GameObject button = GetChild(obj, button_name);
            if (!button.IsNullOrDestroyed())
            {
                result = GetChild(button, text_name).GetComponent<Text>();
            }

            return result;
        }
        public static Dropdown Get_DopboxInPanel(GameObject obj, string panel_name, string dropdown_name, UnityAction<int> action)
        {
            Dropdown result = null; // new Dropdown();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed())
            {
                GameObject dropdown = GetChild(panel, dropdown_name);
                if (!dropdown.IsNullOrDestroyed())
                {
                    result = dropdown.GetComponent<Dropdown>();
                    result.onValueChanged = new Dropdown.DropdownEvent();
                    result.onValueChanged.AddListener(action);
                }
                else { Main.logger_instance?.Error("Dropdown : " + dropdown_name + " not found in " + panel_name); }
            }
            else { Main.logger_instance?.Error("Panel : "+ panel_name + " not found"); }
            
            return result;
        }
        public static Toggle Get_ToggleInLabel(GameObject obj, string panel_name, string obj_name, bool makeSureItsActive = false)
        {
            Toggle result = null; // new Toggle();
            GameObject panel = GetChild(obj, panel_name);
            if (!panel.IsNullOrDestroyed())
            {
                GameObject label = GetChild(panel, "Title");
                if (!label.IsNullOrDestroyed())
                {
                    var temp = Functions.GetChild(label, obj_name);
                    if (makeSureItsActive) temp.SetActive(true);
                    result = temp.GetComponent<Toggle>();
                }
            }

            return result;
        }
        public static bool Check_Texture(string name)
        {
            if ((name.Substring(name.Length - Extensions.jpg.Length, Extensions.jpg.Length).ToLower() == Extensions.jpg) ||
                        (name.Substring(name.Length - Extensions.png.Length, Extensions.png.Length).ToLower() == Extensions.png))
            {
                return true;
            }
            else { return false; }
        }
        public static bool Check_Json(string name)
        {
            if (name.Substring(name.Length - Extensions.json.Length, Extensions.json.Length).ToLower() == Extensions.json)
            {
                return true;
            }
            else { return false; }
        }
        public static bool Check_Prefab(string name)
        {
            if (name.Substring(name.Length - Extensions.prefab.Length, Extensions.prefab.Length).ToLower() == Extensions.prefab)
            {
                return true;
            }
            else { return false; }
        }
        public static Sprite GetItemIcon(ItemDataUnpacked item)
        {
            Sprite result = null; // new Sprite();
            try { result = UITooltipItem.GetItemSprite(item, ItemUIContext.Default); }
            catch { Main.logger_instance?.Error("Error GetItemIcon"); }

            return result;
        }
        public static bool CheckClass(int classe, ItemList.ClassRequirement req)
        {
            if ((req == ItemList.ClassRequirement.Any) ||
                (req == ItemList.ClassRequirement.None) ||
                ((req == ItemList.ClassRequirement.Primalist) && (classe == 0)) ||
                ((req == ItemList.ClassRequirement.Mage) && (classe == 1)) ||
                ((req == ItemList.ClassRequirement.Sentinel) && (classe == 2)) ||
                ((req == ItemList.ClassRequirement.Acolyte) && (classe == 3)) ||
                ((req == ItemList.ClassRequirement.Rogue) && (classe == 4)))
            { return true; }
            else { return false; }
        }
    }
}
