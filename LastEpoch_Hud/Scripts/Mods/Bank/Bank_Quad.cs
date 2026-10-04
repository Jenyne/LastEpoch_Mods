using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Bank
{
    [RegisterTypeInIl2Cpp]
    public class Bank_Quad : MonoBehaviour
    {
        public Bank_Quad(System.IntPtr ptr) : base(ptr) { }
        public static Bank_Quad instance { get; private set; }

        public static int character_index = -1;
        public static int backup_active_tab = -1;
        public static Vector2Int default_size = new Vector2Int(12, 17);
        public static Sprite default_grid = null;
        public static Vector2Int quad_size = new Vector2Int(24, 34);
        public static Sprite quad_grid = null;
        public static UIPanel stash_panel = null;
        public static StashItemContainer stash_item_container = null;
        public static StashItemContainerUI stash_item_container_ui = null;
        public static Image stash_grid_image = null;
        private static readonly System.Collections.Generic.Dictionary<System.IntPtr, bool[]> occupied_slots = new System.Collections.Generic.Dictionary<System.IntPtr, bool[]>();
        public static ItemContainersManager item_contenairs_manager = null;

        //configure
        public static ConfigureTabUI configure_tab_ui = null;
        public static GameObject QuadStash_obj = null;
        public static string toggle_str = "Quad Stash";
        public static string toggle_explain_str = "Empty the tab before changing its size. Reopen to refresh.";
        public static Toggle configure_stash_toggle = null;
        public static TextMeshProUGUI configure_stash_toggle_title = null;
        public static TextMeshProUGUI configure_stash_toggle_explanation = null;
        public static string configure_stash_name_backup = "";
        public static bool open_configure = false;

        void Awake()
        {
            instance = this;

        }
        void Update()
        {
            if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            {
                if (!Scenes.IsGameScene())
                {
                    if (!stash_panel.IsNullOrDestroyed()) { stash_panel = null; } //Reset

                    if ((Refs_Manager.game_uibase.characterSelectOpen) && (!Refs_Manager.game_uibase.characterSelectPanel.IsNullOrDestroyed()))
                    {
                        GameObject char_selection_game_object = Refs_Manager.game_uibase.characterSelectPanel.instance;
                        if (!char_selection_game_object.IsNullOrDestroyed())
                        {
                            CharacterSelect char_select = char_selection_game_object.GetComponent<CharacterSelect>();
                            LocalCharacterSlots local_slots = char_selection_game_object.GetComponent<LocalCharacterSlots>();

                            if ((!char_select.IsNullOrDestroyed()) && (!local_slots.IsNullOrDestroyed()))
                            {
                                if (char_select.currentState == CharacterSelect.CharacterSelectState.LoadCharacter)
                                {
                                    int index = char_select.SelectedCharacterIndex;
                                    if ((index > -1) && (index != character_index) && (index < local_slots.characterSlots.Count))
                                    {
                                        character_index = index;
                                        Cycle cycle = local_slots.characterSlots[index].Cycle;
                                        string solo_char_name = "";
                                        StashType stashType = StashType.Softcore;

                                        if (local_slots.characterSlots[index].SoloChallenge)
                                        {
                                            solo_char_name = local_slots.characterSlots[index].CharacterName;
                                            Save.Data.path = Save.Data.base_path + cycle.ToString() + @"\" + solo_char_name + @"\";
                                        }
                                        else
                                        {
                                            if (local_slots.characterSlots[index].Hardcore) { stashType = StashType.Hardcore; }
                                            else { stashType = StashType.Softcore; }
                                            Save.Data.path = Save.Data.base_path + cycle.ToString() + @"\" + stashType.ToString() + @"\";
                                        }
                                        Save.Data.Load();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Get Refs
                    if (stash_panel.IsNullOrDestroyed() && !StashPanelUI.Instance.IsNullOrDestroyed())
                    {
                        stash_panel = StashPanelUI.Instance.GetComponent<UIPanel>();
                        if (stash_panel.IsNullOrDestroyed())
                        {
                            stash_panel = StashPanelUI.Instance.GetComponentInParent<UIPanel>();
                        }
                    }
                    if ((!stash_panel.IsNullOrDestroyed()) && (/*(stash_item_container_ui.IsNullOrDestroyed()) ||*/ (stash_grid_image.IsNullOrDestroyed()) || (default_grid.IsNullOrDestroyed())))
                    {
                        if (!stash_panel.instance.IsNullOrDestroyed())
                        {
                            GameObject left_obj = Functions.GetChild(stash_panel.instance, "left-container");
                            if (!left_obj.IsNullOrDestroyed())
                            {
                                /*if (stash_item_container_ui.IsNullOrDestroyed())
                                {
                                    GameObject stash_obj = Functions.GetChild(left_obj, "Stash");
                                    if (!stash_obj.IsNullOrDestroyed()) {  stash_item_container_ui = stash_obj.GetComponent<StashItemContainerUI>(); }
                                }*/
                                if (stash_grid_image.IsNullOrDestroyed())
                                {
                                    GameObject grid_obj = Functions.GetChild(left_obj, "grid-img");
                                    if (!grid_obj.IsNullOrDestroyed()) { stash_grid_image = grid_obj.GetComponent<Image>(); }
                                }
                                if ((!stash_grid_image.IsNullOrDestroyed()) && (default_grid.IsNullOrDestroyed()))
                                {
                                    default_grid = stash_grid_image.activeSprite;
                                    Object.DontDestroyOnLoad(default_grid);
                                }
                            }
                        }
                    }
                    if ((!Hud_Manager.asset_bundle.IsNullOrDestroyed()) && (quad_grid.IsNullOrDestroyed()))
                    {
                        foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                        {
                            if (name.Contains("/quadstash/"))
                            {
                                if ((Functions.Check_Texture(name)) && (name.Contains("quad_grid")))
                                {
                                    Texture2D texture = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<Texture2D>();
                                    quad_grid = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                                    Object.DontDestroyOnLoad(quad_grid);
                                }
                            }
                        }
                    }

                    //Update UI
                    if ((!stash_panel.IsNullOrDestroyed()) && (!stash_item_container.IsNullOrDestroyed()))
                    {
                        if (stash_panel.isOpen)
                        {
                            if (backup_active_tab != stash_item_container.CurrentlyActiveTab) //Tab Changed
                            {
                                backup_active_tab = stash_item_container.CurrentlyActiveTab;
                                if (!stash_grid_image.IsNullOrDestroyed())
                                {
                                    if (Get.IsQuadStash() && !quad_grid.IsNullOrDestroyed()) { stash_grid_image.sprite = quad_grid; }
                                    else { stash_grid_image.sprite = default_grid; }
                                }
                            }
                        }
                        else { backup_active_tab = -1; }
                    }
                    if (!configure_tab_ui.IsNullOrDestroyed())
                    {
                        if ((open_configure) && (configure_tab_ui.gameObject.active)) //DoOnce
                        {
                            open_configure = false;
                            configure_stash_name_backup = configure_tab_ui.nameInputTMP.text; //set backup name
                            if (!configure_stash_toggle_title.IsNullOrDestroyed())
                            {
                                if (configure_stash_toggle_title.text != toggle_str)
                                {
                                    configure_stash_toggle_title.text = toggle_str;
                                }                                
                            }
                            if (!configure_stash_toggle_explanation.IsNullOrDestroyed())
                            {
                                if (configure_stash_toggle_explanation.text != toggle_explain_str)
                                {
                                    configure_stash_toggle_explanation.text = toggle_explain_str;
                                }                                
                            }
                            if ((!Save.Data.UserTabs.IsNullOrDestroyed()) && (!configure_stash_toggle.IsNullOrDestroyed()))
                            {
                                configure_stash_toggle.isOn = Save.Data.UserTabs.names.Contains(configure_tab_ui.nameInputTMP.text);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsQuadContainer(ItemContainer container)
        {
            return !container.IsNullOrDestroyed() && container.id == ContainerID.STASH && container.size == quad_size;
        }

        private static bool Fits(Vector2Int position, Vector2Int size, Vector2Int bounds)
        {
            return position.x >= 0 && position.y >= 0 && size.x > 0 && size.y > 0 &&
                size.x <= bounds.x && size.y <= bounds.y &&
                position.x <= bounds.x - size.x && position.y <= bounds.y - size.y;
        }

        private static bool[] GetOccupiedSlots(ItemContainer container)
        {
            if (occupied_slots.TryGetValue(container.Pointer, out var slots)) { return slots; }
            slots = new bool[quad_size.x * quad_size.y];
            // A tab may already contain items when it first becomes quad. Reconstruct
            // occupancy instead of assuming a missing cache means an empty tab.
            foreach (var entry in container.content)
            {
                if (!Fits(entry.Position, entry.size, quad_size))
                {
                    // Preserve invalid/out-of-range contents; never permit overlapping writes.
                    System.Array.Fill(slots, true);
                    break;
                }
                foreach (int index in Get.SlotsPosition(entry.Position, entry.size)) { slots[index] = true; }
            }
            occupied_slots[container.Pointer] = slots;
            return slots;
        }
        public class Get
        {
            public static string ActiveTabName()
            {
                string r = "";
                try
                {
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        int index = stash_item_container.CurrentlyActiveTab;
                        for (int i = 0; i < stash_item_container.LinkedStash.Tabs.Count; i++)
                        {
                            if (stash_item_container.LinkedStash.Tabs[i].TabId == index)
                            {
                                r = stash_item_container.LinkedStash.Tabs[i].DisplayName;
                                break;
                            }
                        }
                    }
                }
                catch { }

                return r;
            }
            public static string ActiveTabName(int index)
            {
                string r = "";
                try
                {
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        for (int i = 0; i < stash_item_container.LinkedStash.Tabs.Count; i++)
                        {
                            if (stash_item_container.LinkedStash.Tabs[i].TabId == index)
                            {
                                r = stash_item_container.LinkedStash.Tabs[i].DisplayName;
                                break;
                            }
                        }
                    }
                }
                catch { }

                return r;
            }
            public static bool IsQuadStash()
            {
                bool r = false;
                try
                {
                    string tab_name = ActiveTabName();
                    if (!Save.Data.UserTabs.IsNullOrDestroyed())
                    {
                        if ((tab_name != "") && (Save.Data.UserTabs.names.Contains(tab_name))) { r = true; }
                    }
                }
                catch { }

                return r;
            }
            public static bool IsQuadStash(int index)
            {
                bool r = false;
                try
                {
                    string tab_name = ActiveTabName(index);
                    if (!Save.Data.UserTabs.IsNullOrDestroyed())
                    {
                        if ((tab_name != "") && (Save.Data.UserTabs.names.Contains(tab_name))) { r = true; }
                    }
                }
                catch { }

                return r;
            }
            public static System.Collections.Generic.List<int> SlotsPosition(Vector2Int slot_position, Vector2Int item_size)
            {
                System.Collections.Generic.List<int> positions = new System.Collections.Generic.List<int>();
                int base_position = slot_position.x + (slot_position.y * quad_size.x);
                for (int y = 0; y < item_size.y; y++)
                {
                    for (int x = 0; x < item_size.x; x++)
                    {
                        positions.Add(base_position + x + (y * quad_size.x));
                    }
                }

                return positions;
            }            
        }
        public class Save
        {
            public class Data
            {
                public static readonly string base_path = Directory.GetCurrentDirectory() + @"\Mods\LastEpoch_Hud\QuadStashs\";
                public static string filename = "QuadStashs.json";
                public static string path = "";
                public static Structures.tabs UserTabs = new Structures.tabs { names = new System.Collections.Generic.List<string>() };

                public class Structures
                {
                    public struct tabs
                    {
                        public System.Collections.Generic.List<string> names;
                    }
                }

                public static void DefaultConfig()
                {
                    Main.logger_instance.Msg("QuadStashs : Make DefaultConfig");

                    UserTabs = new Structures.tabs
                    {
                        names = new System.Collections.Generic.List<string>()
                    };
                }
                public static void Load()
                {
                    if (string.IsNullOrEmpty(Data.path)) { return; }
                    Main.logger_instance.Msg("QuadStashs : Try to Load : " + Data.path + Data.filename);

                    if (!File.Exists(Data.path + filename))
                    {
                        DefaultConfig();
                        Save();
                    }
                    if (File.Exists(Data.path + filename))
                    {
                        try
                        {
                            Data.UserTabs = JsonConvert.DeserializeObject<Structures.tabs>(File.ReadAllText(Data.path + filename));
                            if (Data.UserTabs.names == null) { DefaultConfig(); }
                            Main.logger_instance.Msg("QuadStashs : Loaded");
                        }
                        catch { Main.logger_instance.Error("QuadStashs : Error loading file : " + Data.path + filename); }
                    }
                }
                public static void Save()
                {
                    if (string.IsNullOrEmpty(Data.path)) { return; }
                    Main.logger_instance.Msg("QuadStashs : Save : " + Data.path + Data.filename);
                    string jsonString = JsonConvert.SerializeObject(Data.UserTabs, Newtonsoft.Json.Formatting.Indented);
                    if (!Directory.Exists(Data.path)) { Directory.CreateDirectory(Data.path); }
                    File.WriteAllText(Data.path + Data.filename, jsonString);
                }
            }
        }
        public class Hooks
        {
            [HarmonyPatch(typeof(ItemContainer), "Clear", new System.Type[] { })]
            public class ItemContainer_Clear
            {
                [HarmonyPostfix]
                static void Postfix(ItemContainer __instance) { occupied_slots.Remove(__instance.Pointer); }
            }

            [HarmonyPatch(typeof(StashItemContainerUI), "Awake")]
            public class StashItemContainerUI_Awake
            {
                [HarmonyPostfix]
                static void Postfix(ref StashItemContainerUI __instance)
                {
                    stash_item_container_ui = __instance;
                }
            }

            [HarmonyPatch(typeof(ItemContainersManager), "Awake")]
            public class ItemContainersManager_Awake
            {
                [HarmonyPrefix]
                static void Prefix(ref ItemContainersManager __instance)
                {
                    item_contenairs_manager = __instance;
                    occupied_slots.Clear();
                    stash_item_container = null;
                    backup_active_tab = -1;
                }
            }

            [HarmonyPatch(typeof(TabbedItemContainer), "AddNewTab", new System.Type[] { typeof(ItemContainer) })]
            public class TabbedItemContainer_AddNewTab
            {
                [HarmonyPrefix]
                static void Prefix(TabbedItemContainer __instance, ItemContainer __0)
                {
                    var stash = __instance.TryCast<StashItemContainer>();
                    if (stash.IsNullOrDestroyed() || __0.IsNullOrDestroyed() || __0.id != ContainerID.STASH) { return; }
                    stash_item_container = stash;
                    int index = stash.containers.Count;
                    // Do not overwrite native dimensions for ordinary/special-purpose tabs.
                    if (Get.IsQuadStash(index)) { __0.size = quad_size; }
                    occupied_slots.Remove(__0.Pointer);
                }
            }

            [HarmonyPatch(typeof(ItemContainerUIWithSearch), "Tabbed_OnActiveTabChanged")]
            public class ItemContainerUIWithSearch_Tabbed_OnActiveTabChanged
            {
                // Let the game select the new container before calculating its slot size.
                [HarmonyPostfix]
                static void Postfix(ItemContainerUIWithSearch __instance, TabbedItemContainer __0)
                {
                    var stash = __0.TryCast<StashItemContainer>();
                    var ui = __instance.TryCast<StashItemContainerUI>();
                    if (stash.IsNullOrDestroyed() || ui.IsNullOrDestroyed()) { return; }
                    stash_item_container = stash;
                    stash_item_container_ui = ui;
                    ui.slotSize = ui.CalculateSlotSize();
                    ui.ReDrawWholeContainer();
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "CheckSlotsOccupied", new System.Type[] { typeof(Vector2Int), typeof(Vector2Int), typeof(Context) })]
            public class ItemContainer_CheckSlotsOccupied
            {
                [HarmonyPrefix]
                static bool Prefix(ItemContainer __instance, ref bool __result, Vector2Int __0, Vector2Int __1)
                {
                    if (!IsQuadContainer(__instance)) { return true; }
                    // Invalid coordinates must never be treated as free space.
                    __result = true;
                    if (!Fits(__0, __1, __instance.size)) { return false; }
                    var slots = GetOccupiedSlots(__instance);
                    foreach (int index in Get.SlotsPosition(__0, __1))
                    {
                        if (slots[index]) { return false; }
                    }
                    __result = false;
                    return false;
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "SetSlotsOccupied", new System.Type[] { typeof(Vector2Int), typeof(Vector2Int), typeof(bool) })]
            public class ItemContainer_SetSlotsOccupied
            {
                [HarmonyPrefix]
                static bool Prefix(ItemContainer __instance, ref bool __result, Vector2Int __0, Vector2Int __1, bool __2)
                {
                    if (!IsQuadContainer(__instance)) { return true; }
                    __result = false;
                    if (!Fits(__0, __1, __instance.size)) { return false; }
                    var slots = GetOccupiedSlots(__instance);
                    foreach (int index in Get.SlotsPosition(__0, __1)) { slots[index] = __2; }
                    __result = true;
                    return false;
                }
            }
            [HarmonyPatch(typeof(ItemContainer), "GetItemsInArea")]
            public class ItemContainer_GetItemsInArea
            {
                [HarmonyPrefix]
                static bool Prefix(ItemContainer __instance, ref Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> __result, Vector2Int __0, Vector2Int __1)
                {
                    bool r = true;
                    if ((__instance.id == ContainerID.STASH) && (__instance.size == quad_size))
                    {
                        if (!Fits(__0, __1, quad_size))
                        {
                            __result = new Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry>();
                            return false;
                        }
                        Il2CppSystem.Collections.Generic.List<int> area_positions = new Il2CppSystem.Collections.Generic.List<int>();
                        foreach (int slot_index in Get.SlotsPosition(__0, __1)) { area_positions.Add(slot_index); }
                        Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> item_list = new Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry>();
                        foreach (ItemContainerEntry item_container_entry in __instance.content)
                        {
                            System.Collections.Generic.List<int> item_positions = Get.SlotsPosition(item_container_entry.Position, item_container_entry.size);
                            foreach (int slot_position in item_positions)
                            {
                                if (area_positions.Contains(slot_position)) { item_list.Add(item_container_entry); }
                            }
                        }
                        __result = item_list;
                        r = false;
                    }

                    return r;
                }
            }

            [HarmonyPatch(typeof(ConfigureTabUI), "OnModalOpen")]
            public class ConfigureTabUI_OnModalOpen
            {
                [HarmonyPostfix]
                static void Postfix(ConfigureTabUI __instance)
                {
                    configure_tab_ui = __instance;
                    open_configure = false;
                    configure_stash_toggle = null;
                    if (__instance.nameInputTMP.IsNullOrDestroyed()) { return; }
                    configure_stash_name_backup = __instance.nameInputTMP.text;
                    if (__instance.contents.IsNullOrDestroyed() || __instance.stashPriorityUI.IsNullOrDestroyed()) { return; }
                    GameObject content = __instance.contents.gameObject;
                    if (!content.IsNullOrDestroyed())
                    {
                        QuadStash_obj = Functions.GetChild(content, "quad_stash_row");
                        if (QuadStash_obj.IsNullOrDestroyed())
                        {
                            GameObject priority_section = Functions.FindDescendant(content, "Priority Section");
                            GameObject priority = __instance.stashPriorityUI.gameObject;
                            if ((!priority_section.IsNullOrDestroyed()) && (!priority.IsNullOrDestroyed()))
                            {
                                RectTransform priority_section_rect_transf = priority_section.GetComponent<RectTransform>();
                                if (priority_section_rect_transf.IsNullOrDestroyed()) { return; }
                                float row_H = priority_section_rect_transf.rect.height;
                                float margin = 20 * priority_section_rect_transf.lossyScale.x; //Fix Hight resolution scaling

                                GameObject row = Functions.FindDescendant(priority, "Explanation Row");
                                if (row.IsNullOrDestroyed()) { return; }
                                QuadStash_obj = UnityEngine.Object.Instantiate(row, new Vector3(row.transform.position.x, (row.transform.position.y - row_H - margin), row.transform.position.z), Quaternion.identity);
                                QuadStash_obj.name = "quad_stash_row";
                                QuadStash_obj.transform.SetParent(content.transform);

                                GameObject toogle_obj = Functions.GetChild(QuadStash_obj, "Priority Toggle");
                                if (!toogle_obj.IsNullOrDestroyed())
                                {
                                    configure_stash_toggle = toogle_obj.GetComponent<Toggle>();
                                    if (!configure_stash_toggle.IsNullOrDestroyed()) { configure_stash_toggle.onValueChanged = new Toggle.ToggleEvent(); }
                                }
                                GameObject text_obj = Functions.GetChild(QuadStash_obj, "OptionText");
                                if (!text_obj.IsNullOrDestroyed())
                                {
                                    GameObject title_obj = Functions.GetChild(text_obj, "Title");
                                    if (!title_obj.IsNullOrDestroyed())
                                    {
                                        configure_stash_toggle_title = title_obj.GetComponent<TextMeshProUGUI>();
                                    }
                                    GameObject explanation_obj = Functions.GetChild(text_obj, "Explanation");
                                    if (!explanation_obj.IsNullOrDestroyed())
                                    {
                                        configure_stash_toggle_explanation = explanation_obj.GetComponent<TextMeshProUGUI>();
                                    }
                                }
                            }
                        }
                        // Rebind on every modal open, including an existing cloned row.
                        if (!QuadStash_obj.IsNullOrDestroyed())
                        {
                            configure_stash_toggle = QuadStash_obj.GetComponentInChildren<Toggle>(true);
                            if (!configure_stash_toggle.IsNullOrDestroyed())
                            {
                                configure_stash_toggle.onValueChanged = new Toggle.ToggleEvent();
                                configure_stash_toggle.SetIsOnWithoutNotify(Save.Data.UserTabs.names != null &&
                                    Save.Data.UserTabs.names.Contains(configure_stash_name_backup));
                            }
                            if (!configure_stash_toggle_title.IsNullOrDestroyed()) { configure_stash_toggle_title.text = toggle_str; }
                            if (!configure_stash_toggle_explanation.IsNullOrDestroyed()) { configure_stash_toggle_explanation.text = toggle_explain_str; }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(StashTabbedUIControls), "HandleConfigureTabResult",
                new System.Type[] { typeof(Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult) })]
            public class StashTabbedUIControls_HandleConfigureTabResult
            {
                [HarmonyPostfix]
                static void Postfix(StashTabbedUIControls __instance, Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult __0)
                {
                    if (__0.IsNullOrDestroyed() || __0.Action != Il2CppLE.UI.PanelSystem.StashConfigureResult.Save ||
                        configure_tab_ui.IsNullOrDestroyed() || __instance != configure_tab_ui.parentUIController ||
                        __0.SelectedTabId != configure_tab_ui.selectedTabID ||
                        string.IsNullOrEmpty(Save.Data.path) || Save.Data.UserTabs.names == null ||
                        configure_stash_toggle.IsNullOrDestroyed() || stash_item_container.IsNullOrDestroyed()) { return; }
                    int selected = __0.SelectedTabId;
                    if (selected < 0 || selected >= stash_item_container.containers.Count) { return; }
                    var container = stash_item_container.containers[selected];
                    if (container.id != ContainerID.STASH) { return; }
                    bool wasQuad = IsQuadContainer(container);
                    bool wantQuad = configure_stash_toggle.isOn;
                    if (wasQuad != wantQuad && container.content.Count > 0)
                    {
                        wantQuad = wasQuad;
                        Main.logger_instance?.Warning("Empty this stash tab before changing its Quad Stash size.");
                    }
                    // Preserve existing quad status across renames, even when a size change is rejected.
                    var names = Save.Data.UserTabs.names;
                    names.Remove(configure_stash_name_backup);
                    if (wantQuad && !names.Contains(__0.TabName)) { names.Add(__0.TabName); }
                    Save.Data.Save();
                    if (wantQuad != wasQuad)
                    {
                        container.size = wantQuad ? quad_size : default_size;
                        occupied_slots.Remove(container.Pointer);
                    }
                    backup_active_tab = -1;
                }
            }
        }
    }
}
