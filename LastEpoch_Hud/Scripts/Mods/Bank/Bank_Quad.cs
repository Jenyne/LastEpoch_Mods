using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Bank;

[RegisterTypeInIl2Cpp]
public class Bank_Quad : MonoBehaviour
{
    public Bank_Quad(System.IntPtr ptr)
        : base(ptr) { }

    public static Bank_Quad instance { get; private set; }

    public static int character_index = -1;
    public static int backup_active_tab = -1;
    static int gridRefreshFrames = 0;
    public static Vector2Int default_size = new Vector2Int(12, 17);
    public static Sprite default_grid = null;
    public static Vector2Int quad_size = new Vector2Int(24, 34);
    public static Sprite quad_grid = null;
    public static UIPanel stash_panel = null;
    public static StashItemContainer stash_item_container = null;
    public static StashItemContainerUI stash_item_container_ui = null;
    public static Image stash_grid_image = null;
    static Image quad_overlay = null;
    static Image.Type default_grid_type = Image.Type.Simple;
    static bool default_grid_preserve_aspect = false;
    static bool saved_grid_image_settings = false;
    private static readonly System.Collections.Generic.Dictionary<
        System.IntPtr,
        bool[]
    > occupied_slots = new System.Collections.Generic.Dictionary<System.IntPtr, bool[]>();
    public static ItemContainersManager item_contenairs_manager = null;

    //configure
    public static ConfigureTabUI configure_tab_ui = null;
    public static GameObject QuadStash_obj = null;
    public static string toggle_str = "Quad Stash";
    public static string toggle_explain_str =
        "Items that no longer fit move to your inventory, or drop at your feet.";
    public static Toggle configure_stash_toggle = null;
    public static TextMeshProUGUI configure_stash_toggle_title = null;
    public static TextMeshProUGUI configure_stash_toggle_explanation = null;
    public static string configure_stash_name_backup = "";
    public static bool open_configure = false;
    static bool ids_migrated = false;
    static bool icons_use_quad_size = false;
    static bool have_native_slot = false;
    static Vector2 native_slot_size = Vector2.zero;

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
                if (!stash_panel.IsNullOrDestroyed())
                {
                    stash_panel = null;
                } //Reset

                if (
                    (Refs_Manager.game_uibase.characterSelectOpen)
                    && (!Refs_Manager.game_uibase.characterSelectPanel.IsNullOrDestroyed())
                )
                {
                    GameObject char_selection_game_object = Refs_Manager
                        .game_uibase
                        .characterSelectPanel
                        .instance;
                    if (!char_selection_game_object.IsNullOrDestroyed())
                    {
                        CharacterSelect char_select =
                            char_selection_game_object.GetComponent<CharacterSelect>();
                        LocalCharacterSlots local_slots =
                            char_selection_game_object.GetComponent<LocalCharacterSlots>();

                        if (
                            (!char_select.IsNullOrDestroyed()) && (!local_slots.IsNullOrDestroyed())
                        )
                        {
                            if (
                                char_select.currentState
                                == CharacterSelect.CharacterSelectState.LoadCharacter
                            )
                            {
                                int index = char_select.SelectedCharacterIndex;
                                if (
                                    (index > -1)
                                    && (index != character_index)
                                    && (index < local_slots.characterSlots.Count)
                                )
                                {
                                    character_index = index;
                                    Cycle cycle = local_slots.characterSlots[index].Cycle;
                                    string solo_char_name = "";
                                    StashType stashType = StashType.Softcore;

                                    if (local_slots.characterSlots[index].SoloChallenge)
                                    {
                                        solo_char_name = local_slots
                                            .characterSlots[index]
                                            .CharacterName;
                                        Save.Data.path =
                                            Save.Data.base_path
                                            + cycle.ToString()
                                            + @"\"
                                            + solo_char_name
                                            + @"\";
                                    }
                                    else
                                    {
                                        if (local_slots.characterSlots[index].Hardcore)
                                        {
                                            stashType = StashType.Hardcore;
                                        }
                                        else
                                        {
                                            stashType = StashType.Softcore;
                                        }
                                        Save.Data.path =
                                            Save.Data.base_path
                                            + cycle.ToString()
                                            + @"\"
                                            + stashType.ToString()
                                            + @"\";
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
                if (
                    (!stash_panel.IsNullOrDestroyed())
                    && ( /*(stash_item_container_ui.IsNullOrDestroyed()) ||*/
                        (stash_grid_image.IsNullOrDestroyed()) || (default_grid.IsNullOrDestroyed())
                    )
                )
                {
                    if (!stash_panel.instance.IsNullOrDestroyed())
                    {
                        GameObject left_obj = Functions.GetChild(
                            stash_panel.instance,
                            "left-container"
                        );
                        if (!left_obj.IsNullOrDestroyed())
                        {
                            /*if (stash_item_container_ui.IsNullOrDestroyed())
                            {
                                GameObject stash_obj = Functions.GetChild(left_obj, "Stash");
                                if (!stash_obj.IsNullOrDestroyed()) {  stash_item_container_ui = stash_obj.GetComponent<StashItemContainerUI>(); }
                            }*/
                            if (stash_grid_image.IsNullOrDestroyed())
                            {
                                GameObject grid_obj = Functions.GetChild(
                                    left_obj,
                                    "grid-img",
                                    false
                                );
                                if (grid_obj.IsNullOrDestroyed())
                                {
                                    grid_obj = Functions.FindDescendant(left_obj, "grid-img");
                                }
                                if (!grid_obj.IsNullOrDestroyed())
                                {
                                    stash_grid_image = grid_obj.GetComponent<Image>();
                                }
                            }
                            if (
                                (!stash_grid_image.IsNullOrDestroyed())
                                && (default_grid.IsNullOrDestroyed())
                            )
                            {
                                default_grid = stash_grid_image.activeSprite;
                                default_grid_type = stash_grid_image.type;
                                default_grid_preserve_aspect = stash_grid_image.preserveAspect;
                                saved_grid_image_settings = true;
                                Object.DontDestroyOnLoad(default_grid);
                            }
                        }
                    }
                }
                if (
                    (!Hud_Manager.asset_bundle.IsNullOrDestroyed())
                    && (quad_grid.IsNullOrDestroyed())
                )
                {
                    foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                    {
                        if (name.Contains("/quadstash/"))
                        {
                            if ((Functions.Check_Texture(name)) && (name.Contains("quad_grid")))
                            {
                                Texture2D texture = Hud_Manager
                                    .asset_bundle.LoadAsset(name)
                                    .TryCast<Texture2D>();
                                quad_grid = Sprite.Create(
                                    texture,
                                    new Rect(0, 0, texture.width, texture.height),
                                    Vector2.zero
                                );
                                Object.DontDestroyOnLoad(quad_grid);
                            }
                        }
                    }
                }

                //Update UI
                if (
                    (!stash_panel.IsNullOrDestroyed())
                    && (!stash_item_container.IsNullOrDestroyed())
                )
                {
                    if (stash_panel.isOpen)
                    {
                        // Rebuild after the game finishes selecting the tab. Doing it inside the
                        // tab-change callback uses the previous container and leaves the icons wrong.
                        bool tabChanged =
                            backup_active_tab != stash_item_container.CurrentlyActiveTab;
                        if ((tabChanged || (gridRefreshFrames > 0)) && RefreshActiveGrid())
                        {
                            backup_active_tab = stash_item_container.CurrentlyActiveTab;
                            if (gridRefreshFrames > 0)
                            {
                                gridRefreshFrames--;
                            }
                        }
                    }
                    else
                    {
                        backup_active_tab = -1;
                    }
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
                        if (
                            (!Save.Data.UserTabs.IsNullOrDestroyed())
                            && (!configure_stash_toggle.IsNullOrDestroyed())
                        )
                        {
                            configure_stash_toggle.isOn = Get.IsQuadStash(
                                configure_tab_ui.selectedTabID
                            );
                        }
                    }
                }
            }
        }
    }

    private static bool IsQuadContainer(ItemContainer container)
    {
        return !container.IsNullOrDestroyed()
            && container.id == ContainerID.STASH
            && container.size == quad_size;
    }

    private static void PrepareToggle(Toggle toggle)
    {
        if (toggle.IsNullOrDestroyed())
        {
            return;
        }
        // The cloned priority row belongs to a toggle group that turns itself back on.
        toggle.group = null;
        toggle.onValueChanged = new Toggle.ToggleEvent();
        toggle.interactable = true;
    }

    // Returns false when an item could not be moved and is still in the tab.
    private static bool EjectOverflow(ItemContainer container, Vector2Int bounds)
    {
        if (container.IsNullOrDestroyed() || container.content == null)
        {
            return true;
        }
        var overflow = new System.Collections.Generic.List<ItemContainerEntry>();
        foreach (ItemContainerEntry entry in container.content)
        {
            if ((entry == null) || (entry.data.IsNullOrDestroyed()))
            {
                continue;
            }
            Vector2Int itemSize = entry.size;
            if (itemSize.x < 1)
            {
                itemSize.x = 1;
            }
            if (itemSize.y < 1)
            {
                itemSize.y = 1;
            }
            Vector2Int pos = entry.Position;
            if (
                (pos.x < 0)
                || (pos.y < 0)
                || (pos.x + itemSize.x > bounds.x)
                || (pos.y + itemSize.y > bounds.y)
            )
            {
                overflow.Add(entry);
            }
        }
        if (overflow.Count == 0)
        {
            return true;
        }

        ItemContainer inventory = null;
        if (!ItemContainersManager.Instance.IsNullOrDestroyed())
        {
            inventory = ItemContainersManager.Instance.inventory;
        }
        bool keptAll = true;
        foreach (ItemContainerEntry entry in overflow)
        {
            if ((entry == null) || (entry.data.IsNullOrDestroyed()))
            {
                continue;
            }
            int qty = entry.Quantity;
            if (qty < 1)
            {
                qty = 1;
            }
            bool moved = false;
            if (!inventory.IsNullOrDestroyed())
            {
                moved = inventory.TryAddItem(entry.data, qty, Context.SILENT);
            }
            if (moved)
            {
                if (!container.TryRemoveItem(entry, qty, Context.SILENT))
                {
                    keptAll = false;
                }
                continue;
            }

            Actor player = Refs_Manager.player_actor;
            if (player.IsNullOrDestroyed())
            {
                player = PlayerFinder.getPlayerActor();
            }
            GroundItemManager ground = Refs_Manager.ground_item_manager;
            if (ground.IsNullOrDestroyed())
            {
                ground = GroundItemManager.instance;
            }
            if (ground.IsNullOrDestroyed())
            {
                ground = UnityEngine.Object.FindObjectOfType<GroundItemManager>();
            }
            if (player.IsNullOrDestroyed() || ground.IsNullOrDestroyed())
            {
                keptAll = false;
                continue;
            }

            var drops = new System.Collections.Generic.List<ItemData>();
            for (int i = 0; i < qty; i++)
            {
                ItemData dropData = entry.data.CreateGameplayDuplicate();
                if (dropData.IsNullOrDestroyed())
                {
                    drops.Clear();
                    break;
                }
                drops.Add(dropData);
            }
            if (drops.Count != qty || !container.TryRemoveItem(entry, qty, Context.SILENT))
            {
                keptAll = false;
                continue;
            }

            Refs_Manager.player_actor = player;
            Refs_Manager.ground_item_manager = ground;
            Vector3 dropPosition = player.position();
            foreach (ItemData dropData in drops)
            {
                ground.dropItemForPlayer(player, dropData, dropPosition, false);
            }
        }
        occupied_slots.Remove(container.Pointer);
        return keptAll;
    }

    // Each stash tab has its own container. Only that tab's size changes.
    // The game's own occupancy array is left alone; quad checks are handled separately.
    static void SyncTabSize(int index)
    {
        if (stash_item_container.IsNullOrDestroyed())
        {
            return;
        }
        if ((index < 0) || (index >= stash_item_container.containers.Count))
        {
            return;
        }
        ItemContainer container = stash_item_container.containers[index];
        if (container.IsNullOrDestroyed() || (container.id != ContainerID.STASH))
        {
            return;
        }
        if (!Get.TryGetTab(index, out _))
        {
            return;
        }
        bool quad = Get.IsQuadStash(index);
        if (!quad && (container.size != quad_size))
        {
            return;
        }
        Vector2Int target = quad ? quad_size : default_size;
        if (container.size == target)
        {
            return;
        }
        if ((container.size.x > target.x) || (container.size.y > target.y))
        {
            if (!EjectOverflow(container, target))
            {
                return;
            }
        }
        container.size = target;
        occupied_slots.Remove(container.Pointer);
    }

    static void SyncEveryTab()
    {
        if (stash_item_container.IsNullOrDestroyed())
        {
            return;
        }
        Get.MigrateIdsFromNames();
        int count = stash_item_container.containers.Count;
        for (int i = 0; i < count; i++)
        {
            SyncTabSize(i);
        }
    }

    static bool BindGridImage()
    {
        if (!stash_grid_image.IsNullOrDestroyed())
        {
            return true;
        }
        GameObject gridObject = null;
        if (!stash_panel.IsNullOrDestroyed() && !stash_panel.instance.IsNullOrDestroyed())
        {
            gridObject = Functions.FindDescendant(stash_panel.instance, "grid-img");
        }
        Transform current = stash_item_container_ui.IsNullOrDestroyed()
            ? null
            : stash_item_container_ui.transform;
        for (
            int depth = 0;
            gridObject.IsNullOrDestroyed() && !current.IsNullOrDestroyed() && depth < 8;
            depth++
        )
        {
            if (current.gameObject.name == "grid-img")
            {
                gridObject = current.gameObject;
            }
            else
            {
                gridObject = Functions.FindDescendant(current.gameObject, "grid-img");
            }
            current = current.parent;
        }
        if (gridObject.IsNullOrDestroyed())
        {
            return false;
        }
        stash_grid_image = gridObject.GetComponent<Image>();
        if (stash_grid_image.IsNullOrDestroyed())
        {
            return false;
        }
        if (default_grid.IsNullOrDestroyed())
        {
            default_grid = stash_grid_image.activeSprite;
            default_grid_type = stash_grid_image.type;
            default_grid_preserve_aspect = stash_grid_image.preserveAspect;
            saved_grid_image_settings = true;
            if (!default_grid.IsNullOrDestroyed())
            {
                Object.DontDestroyOnLoad(default_grid);
            }
        }
        return true;
    }

    static bool TryGridPixels(out float width, out float height)
    {
        width = 0f;
        height = 0f;
        BindGridImage();
        if (!stash_grid_image.IsNullOrDestroyed())
        {
            Rect grid = stash_grid_image.rectTransform.rect;
            width = Mathf.Abs(grid.width);
            height = Mathf.Abs(grid.height);
        }
        if (
            ((width < 10f) || (height < 10f))
            && !stash_item_container_ui.IsNullOrDestroyed()
            && !stash_item_container_ui.rectT.IsNullOrDestroyed()
        )
        {
            Rect panel = stash_item_container_ui.rectT.rect;
            width = Mathf.Abs(panel.width);
            height = Mathf.Abs(panel.height);
        }
        return (width >= 10f) && (height >= 10f);
    }

    static ItemContainer ShownContainer()
    {
        if (stash_item_container.IsNullOrDestroyed())
        {
            return null;
        }
        ItemContainer shown = stash_item_container.GetActiveItemContainer();
        if (!shown.IsNullOrDestroyed())
        {
            return shown;
        }
        int active = stash_item_container.CurrentlyActiveTab;
        if ((active < 0) || (active >= stash_item_container.containers.Count))
        {
            return null;
        }
        return stash_item_container.containers[active];
    }

    static bool ActiveTabIsQuad()
    {
        if (stash_item_container.IsNullOrDestroyed())
        {
            return false;
        }
        return Get.IsQuadStash(stash_item_container.CurrentlyActiveTab);
    }

    static Vector2 QuadSlotSize(float width, float height)
    {
        // Use the visible stash grid rectangle. Native slot padding is not part of this
        // coordinate system and moves quad items outside the panel.
        return new Vector2(width / quad_size.x, height / quad_size.y);
    }

    static bool RefreshActiveGrid()
    {
        if (stash_item_container.IsNullOrDestroyed())
        {
            return false;
        }
        if (stash_item_container_ui.IsNullOrDestroyed())
        {
            stash_item_container_ui = UnityEngine.Object.FindObjectOfType<StashItemContainerUI>();
        }
        if (stash_item_container_ui.IsNullOrDestroyed())
        {
            return false;
        }
        if (!TryGridPixels(out float width, out float height))
        {
            return false;
        }

        SyncEveryTab();
        bool quad = ActiveTabIsQuad();
        if (quad)
        {
            Vector2 slot = QuadSlotSize(width, height);
            stash_item_container_ui.slotSize = slot;
            ResizeVisibleIcons(slot);
            icons_use_quad_size = true;
        }
        else if (icons_use_quad_size)
        {
            RestoreNativeIcons();
        }
        EnsureQuadSprite();
        EnsureOverlay();
        if (!quad_overlay.IsNullOrDestroyed())
        {
            quad_overlay.gameObject.SetActive(quad);
            if (quad && !quad_grid.IsNullOrDestroyed())
            {
                quad_overlay.sprite = quad_grid;
                quad_overlay.color = Color.white;
            }
        }
        if (!stash_grid_image.IsNullOrDestroyed())
        {
            if (default_grid.IsNullOrDestroyed())
            {
                default_grid = stash_grid_image.sprite;
            }
            if (!default_grid.IsNullOrDestroyed())
            {
                stash_grid_image.sprite = default_grid;
                if (saved_grid_image_settings)
                {
                    stash_grid_image.type = default_grid_type;
                    stash_grid_image.preserveAspect = default_grid_preserve_aspect;
                }
            }
            stash_grid_image.enabled = !quad;
        }
        stash_item_container_ui.ReDrawWholeContainer();
        return true;
    }

    static void EnsureOverlay()
    {
        if (stash_grid_image.IsNullOrDestroyed())
        {
            return;
        }
        RectTransform grid_rect = stash_grid_image.rectTransform;
        if (grid_rect.IsNullOrDestroyed())
        {
            return;
        }
        if (quad_overlay.IsNullOrDestroyed())
        {
            Transform existing = grid_rect.Find("quad_grid_overlay");
            GameObject overlay_obj = existing.IsNullOrDestroyed()
                ? new GameObject("quad_grid_overlay")
                : existing.gameObject;
            if (existing.IsNullOrDestroyed())
            {
                overlay_obj.transform.SetParent(grid_rect, false);
            }
            quad_overlay = overlay_obj.GetComponent<Image>();
            if (quad_overlay.IsNullOrDestroyed())
            {
                quad_overlay = overlay_obj.AddComponent<Image>();
            }
            quad_overlay.raycastTarget = false;
            quad_overlay.type = Image.Type.Simple;
            quad_overlay.preserveAspect = false;
        }
        RectTransform overlay_rect = quad_overlay.rectTransform;
        if (overlay_rect.parent != grid_rect)
        {
            overlay_rect.SetParent(grid_rect, false);
        }
        overlay_rect.anchorMin = Vector2.zero;
        overlay_rect.anchorMax = Vector2.one;
        overlay_rect.pivot = new Vector2(0.5f, 0.5f);
        overlay_rect.anchoredPosition = Vector2.zero;
        overlay_rect.sizeDelta = Vector2.zero;
        overlay_rect.offsetMin = Vector2.zero;
        overlay_rect.offsetMax = Vector2.zero;
        overlay_rect.localRotation = Quaternion.identity;
        overlay_rect.localScale = Vector3.one;
    }

    static void RestoreNativeIcons()
    {
        icons_use_quad_size = false;
        if (stash_item_container_ui.IsNullOrDestroyed())
        {
            return;
        }
        // Redraw first so the game reports its own cell size for a normal tab.
        stash_item_container_ui.ReDrawWholeContainer();
        if (!have_native_slot)
        {
            return;
        }
        stash_item_container_ui.slotSize = native_slot_size;
        ResizeVisibleIcons(native_slot_size);
    }

    static void ResizeVisibleIcons(Vector2 slot)
    {
        if (stash_item_container_ui.IsNullOrDestroyed())
        {
            return;
        }
        InventoryItemUI[] icons = stash_item_container_ui.GetComponentsInChildren<InventoryItemUI>(
            false
        );
        for (int i = 0; i < icons.Length; i++)
        {
            InventoryItemUI icon = icons[i];
            if (icon.IsNullOrDestroyed())
            {
                continue;
            }
            if ((icon.slotSize - slot).sqrMagnitude < 0.01f)
            {
                continue;
            }
            icon.slotSize = slot;
            ItemContainerEntry entry = icon.EntryRef;
            if (!entry.IsNullOrDestroyed())
            {
                icon.SetPosition(slot, entry);
            }
        }
    }

    // Runs after the game's own UI update, which otherwise puts the 12x17 grid back.
    static void BeforeRender()
    {
        if (
            stash_panel.IsNullOrDestroyed()
            || !stash_panel.isOpen
            || stash_item_container.IsNullOrDestroyed()
        )
        {
            return;
        }
        if (stash_grid_image.IsNullOrDestroyed() || stash_item_container_ui.IsNullOrDestroyed())
        {
            return;
        }
        bool quad = ActiveTabIsQuad();
        if (quad)
        {
            EnsureQuadSprite();
            EnsureOverlay();
        }
        if (!quad_overlay.IsNullOrDestroyed())
        {
            quad_overlay.gameObject.SetActive(quad);
            quad_overlay.enabled = quad;
            if (quad && !quad_grid.IsNullOrDestroyed())
            {
                quad_overlay.sprite = quad_grid;
                quad_overlay.color = Color.white;
            }
        }
        if (!default_grid.IsNullOrDestroyed())
        {
            stash_grid_image.sprite = default_grid;
        }
        if (saved_grid_image_settings)
        {
            stash_grid_image.type = default_grid_type;
            stash_grid_image.preserveAspect = default_grid_preserve_aspect;
        }
        stash_grid_image.enabled = !quad;
    }

    static void EnsureQuadSprite()
    {
        if (!quad_grid.IsNullOrDestroyed())
        {
            return;
        }
        if (!Hud_Manager.asset_bundle.IsNullOrDestroyed())
        {
            foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
            {
                if (
                    (!name.Contains("/quadstash/"))
                    || (!name.Contains("quad_grid"))
                    || (!Functions.Check_Texture(name))
                )
                {
                    continue;
                }
                Texture2D texture = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<Texture2D>();
                if (texture.IsNullOrDestroyed())
                {
                    continue;
                }
                quad_grid = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                UnityEngine.Object.DontDestroyOnLoad(quad_grid);
                return;
            }
        }
        quad_grid = BuildGridSprite(quad_size.x, quad_size.y);
        UnityEngine.Object.DontDestroyOnLoad(quad_grid);
    }

    static Sprite BuildGridSprite(int columns, int rows)
    {
        const int cell = 16;
        int width = columns * cell;
        int height = rows * cell;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];
        Color32 fill = new Color32(18, 16, 14, 210);
        Color32 line = new Color32(166, 124, 62, 255);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = fill;
        }
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int left = x * cell;
                int bottom = y * cell;
                for (int i = 0; i < cell; i++)
                {
                    pixels[(bottom * width) + left + i] = line;
                    pixels[((bottom + cell - 1) * width) + left + i] = line;
                    pixels[((bottom + i) * width) + left] = line;
                    pixels[((bottom + i) * width) + left + cell - 1] = line;
                }
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), cell);
    }

    private static bool Fits(Vector2Int position, Vector2Int size, Vector2Int bounds)
    {
        return position.x >= 0
            && position.y >= 0
            && size.x > 0
            && size.y > 0
            && size.x <= bounds.x
            && size.y <= bounds.y
            && position.x <= bounds.x - size.x
            && position.y <= bounds.y - size.y;
    }

    private static bool[] GetOccupiedSlots(ItemContainer container)
    {
        if (occupied_slots.TryGetValue(container.Pointer, out var slots))
        {
            return slots;
        }
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
            foreach (int index in Get.SlotsPosition(entry.Position, entry.size))
            {
                if ((index >= 0) && (index < slots.Length))
                {
                    slots[index] = true;
                }
            }
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

        public static bool TryGetTab(int index, out StashTab tab)
        {
            tab = null;
            try
            {
                if (
                    stash_item_container.IsNullOrDestroyed()
                    || stash_item_container.LinkedStash.IsNullOrDestroyed()
                )
                {
                    return false;
                }
                var tabs = stash_item_container.LinkedStash.Tabs;
                if (tabs == null)
                {
                    return false;
                }
                // containers[index] lines up with Tabs[index]. TabId is only a fallback.
                if ((index >= 0) && (index < tabs.Count))
                {
                    tab = tabs[index];
                    if (!tab.IsNullOrDestroyed())
                    {
                        return true;
                    }
                }
                for (int i = 0; i < tabs.Count; i++)
                {
                    StashTab candidate = tabs[i];
                    if ((!candidate.IsNullOrDestroyed()) && (candidate.TabId == index))
                    {
                        tab = candidate;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static int TabStableId(int index)
        {
            if (TryGetTab(index, out StashTab tab))
            {
                return tab.TabId;
            }
            return index;
        }

        public static string TabName(int index)
        {
            if (TryGetTab(index, out StashTab tab) && (tab.DisplayName != null))
            {
                return tab.DisplayName;
            }
            return ActiveTabName(index);
        }

        public static void MigrateIdsFromNames()
        {
            if (ids_migrated)
            {
                return;
            }
            if (Save.Data.UserTabs.names == null)
            {
                Save.Data.UserTabs.names = new System.Collections.Generic.List<string>();
            }
            if (Save.Data.UserTabs.ids == null)
            {
                Save.Data.UserTabs.ids = new System.Collections.Generic.List<int>();
            }
            if (
                stash_item_container.IsNullOrDestroyed()
                || stash_item_container.LinkedStash.IsNullOrDestroyed()
            )
            {
                return;
            }
            if ((Save.Data.UserTabs.ids.Count == 0) && (Save.Data.UserTabs.names.Count > 0))
            {
                int count = stash_item_container.containers.Count;
                for (int i = 0; i < count; i++)
                {
                    string name = TabName(i);
                    if ((name == "") || (!Save.Data.UserTabs.names.Contains(name)))
                    {
                        continue;
                    }
                    int id = TabStableId(i);
                    if (!Save.Data.UserTabs.ids.Contains(id))
                    {
                        Save.Data.UserTabs.ids.Add(id);
                    }
                }
                if (Save.Data.UserTabs.ids.Count > 0)
                {
                    Save.Data.Save();
                }
            }
            ids_migrated = true;
        }

        public static void RememberQuadTabs()
        {
            if (Save.Data.UserTabs.names == null)
            {
                Save.Data.UserTabs.names = new System.Collections.Generic.List<string>();
            }
            if (Save.Data.UserTabs.ids == null)
            {
                Save.Data.UserTabs.ids = new System.Collections.Generic.List<int>();
            }
            Save.Data.UserTabs.names.Clear();
            if (stash_item_container.IsNullOrDestroyed())
            {
                return;
            }
            int count = stash_item_container.containers.Count;
            for (int i = 0; i < count; i++)
            {
                if (!Save.Data.UserTabs.ids.Contains(TabStableId(i)))
                {
                    continue;
                }
                string name = TabName(i);
                if ((name != "") && (!Save.Data.UserTabs.names.Contains(name)))
                {
                    Save.Data.UserTabs.names.Add(name);
                }
            }
        }

        public static bool IsQuadStash()
        {
            if (stash_item_container.IsNullOrDestroyed())
            {
                return false;
            }
            return IsQuadStash(stash_item_container.CurrentlyActiveTab);
        }

        public static bool IsQuadStash(int index)
        {
            try
            {
                MigrateIdsFromNames();
                if (Save.Data.UserTabs.ids == null)
                {
                    Save.Data.UserTabs.ids = new System.Collections.Generic.List<int>();
                }
                if (Save.Data.UserTabs.names == null)
                {
                    Save.Data.UserTabs.names = new System.Collections.Generic.List<string>();
                }
                int id = TabStableId(index);
                if (Save.Data.UserTabs.ids.Count > 0)
                {
                    return Save.Data.UserTabs.ids.Contains(id);
                }
                string tab_name = TabName(index);
                return (tab_name != "") && Save.Data.UserTabs.names.Contains(tab_name);
            }
            catch
            {
                return false;
            }
        }

        public static System.Collections.Generic.List<int> SlotsPosition(
            Vector2Int slot_position,
            Vector2Int item_size
        )
        {
            System.Collections.Generic.List<int> positions =
                new System.Collections.Generic.List<int>();
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
            public static readonly string base_path =
                Directory.GetCurrentDirectory() + @"\Mods\LastEpoch_Hud\QuadStashs\";
            public static string filename = "QuadStashs.json";
            public static string path = "";
            public static Structures.tabs UserTabs = new Structures.tabs
            {
                names = new System.Collections.Generic.List<string>(),
            };

            public class Structures
            {
                public struct tabs
                {
                    public System.Collections.Generic.List<string> names;
                    public System.Collections.Generic.List<int> ids;
                }
            }

            public static void DefaultConfig()
            {
                Main.logger_instance.Msg("QuadStashs : Make DefaultConfig");

                UserTabs = new Structures.tabs
                {
                    names = new System.Collections.Generic.List<string>(),
                    ids = new System.Collections.Generic.List<int>(),
                };
            }

            public static void Load()
            {
                if (string.IsNullOrEmpty(Data.path))
                {
                    return;
                }
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
                        Data.UserTabs = JsonConvert.DeserializeObject<Structures.tabs>(
                            File.ReadAllText(Data.path + filename)
                        );
                        if (Data.UserTabs.names == null)
                        {
                            DefaultConfig();
                        }
                        if (Data.UserTabs.ids == null)
                        {
                            Data.UserTabs.ids = new System.Collections.Generic.List<int>();
                        }
                        Main.logger_instance.Msg("QuadStashs : Loaded");
                    }
                    catch
                    {
                        Main.logger_instance.Error(
                            "QuadStashs : Error loading file : " + Data.path + filename
                        );
                    }
                }
            }

            public static void Save()
            {
                if (string.IsNullOrEmpty(Data.path))
                {
                    return;
                }
                Main.logger_instance.Msg("QuadStashs : Save : " + Data.path + Data.filename);
                string jsonString = JsonConvert.SerializeObject(
                    Data.UserTabs,
                    Newtonsoft.Json.Formatting.Indented
                );
                if (!Directory.Exists(Data.path))
                {
                    Directory.CreateDirectory(Data.path);
                }
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
            static void Postfix(ItemContainer __instance)
            {
                occupied_slots.Remove(__instance.Pointer);
            }
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

        [HarmonyPatch(
            typeof(TabbedItemContainer),
            "AddNewTab",
            new System.Type[] { typeof(ItemContainer) }
        )]
        public class TabbedItemContainer_AddNewTab
        {
            [HarmonyPrefix]
            static void Prefix(TabbedItemContainer __instance, ItemContainer __0)
            {
                var stash = __instance.TryCast<StashItemContainer>();
                if (
                    stash.IsNullOrDestroyed()
                    || __0.IsNullOrDestroyed()
                    || __0.id != ContainerID.STASH
                )
                {
                    return;
                }
                stash_item_container = stash;
                int index = stash.containers.Count;
                // Do not overwrite native dimensions for ordinary/special-purpose tabs.
                if (Get.IsQuadStash(index))
                {
                    __0.size = quad_size;
                }
                occupied_slots.Remove(__0.Pointer);
            }
        }

        [HarmonyPatch(typeof(ItemContainerUI), "CalculateSlotSize")]
        public class ItemContainerUI_CalculateSlotSize
        {
            // The game keeps measuring a 12x17 grid. A quad tab has to use its own cell size,
            // taken from the grid image so icons stay inside it.
            [HarmonyPostfix]
            static void Postfix(ItemContainerUI __instance, ref Vector2 __result)
            {
                StashItemContainerUI stashUi = __instance.TryCast<StashItemContainerUI>();
                if (stashUi.IsNullOrDestroyed())
                {
                    return;
                }
                if (!ActiveTabIsQuad())
                {
                    if ((__result.x > 1f) && (__result.y > 1f))
                    {
                        native_slot_size = __result;
                        have_native_slot = true;
                    }
                    return;
                }
                if (!TryGridPixels(out float width, out float height))
                {
                    return;
                }
                __result = QuadSlotSize(width, height);
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
                if (stash.IsNullOrDestroyed() || ui.IsNullOrDestroyed())
                {
                    return;
                }
                stash_item_container = stash;
                stash_item_container_ui = ui;
                backup_active_tab = -1;
                gridRefreshFrames = 5;
            }
        }

        [HarmonyPatch(
            typeof(ItemContainer),
            "CheckSlotsOccupied",
            new System.Type[] { typeof(Vector2Int), typeof(Vector2Int), typeof(Context) }
        )]
        public class ItemContainer_CheckSlotsOccupied
        {
            [HarmonyPrefix]
            static bool Prefix(
                ItemContainer __instance,
                ref bool __result,
                Vector2Int __0,
                Vector2Int __1
            )
            {
                if (!IsQuadContainer(__instance))
                {
                    return true;
                }
                // Invalid coordinates must never be treated as free space.
                __result = true;
                if (!Fits(__0, __1, __instance.size))
                {
                    return false;
                }
                var slots = GetOccupiedSlots(__instance);
                foreach (int index in Get.SlotsPosition(__0, __1))
                {
                    if ((index < 0) || (index >= slots.Length) || slots[index])
                    {
                        return false;
                    }
                }
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(
            typeof(ItemContainer),
            "CheckSlotsOccupiedExceptArea",
            new System.Type[]
            {
                typeof(Vector2Int),
                typeof(Vector2Int),
                typeof(Vector2Int),
                typeof(Vector2Int),
            }
        )]
        public class ItemContainer_CheckSlotsOccupiedExceptArea
        {
            // The game's occupancy array stays 12x17. Quad tabs have to answer this themselves.
            [HarmonyPrefix]
            static bool Prefix(
                ItemContainer __instance,
                ref bool __result,
                Vector2Int __0,
                Vector2Int __1,
                Vector2Int __2,
                Vector2Int __3
            )
            {
                if (!IsQuadContainer(__instance))
                {
                    return true;
                }
                __result = true;
                if (!Fits(__0, __1, __instance.size))
                {
                    return false;
                }
                var slots = GetOccupiedSlots(__instance);
                foreach (int index in Get.SlotsPosition(__0, __1))
                {
                    int x = index % quad_size.x;
                    int y = index / quad_size.x;
                    if ((x >= __2.x) && (y >= __2.y) && (x < __2.x + __3.x) && (y < __2.y + __3.y))
                    {
                        continue;
                    }
                    if ((index < 0) || (index >= slots.Length) || slots[index])
                    {
                        return false;
                    }
                }
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(
            typeof(ItemContainer),
            "SetSlotsOccupied",
            new System.Type[] { typeof(Vector2Int), typeof(Vector2Int), typeof(bool) }
        )]
        public class ItemContainer_SetSlotsOccupied
        {
            [HarmonyPrefix]
            static bool Prefix(
                ItemContainer __instance,
                ref bool __result,
                Vector2Int __0,
                Vector2Int __1,
                bool __2
            )
            {
                if (!IsQuadContainer(__instance))
                {
                    return true;
                }
                __result = false;
                if (!Fits(__0, __1, __instance.size))
                {
                    return false;
                }
                var slots = GetOccupiedSlots(__instance);
                foreach (int index in Get.SlotsPosition(__0, __1))
                {
                    slots[index] = __2;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemContainer), "GetItemsInArea")]
        public class ItemContainer_GetItemsInArea
        {
            [HarmonyPrefix]
            static bool Prefix(
                ItemContainer __instance,
                ref Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> __result,
                Vector2Int __0,
                Vector2Int __1
            )
            {
                bool r = true;
                if ((__instance.id == ContainerID.STASH) && (__instance.size == quad_size))
                {
                    if (!Fits(__0, __1, quad_size))
                    {
                        __result =
                            new Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry>();
                        return false;
                    }
                    Il2CppSystem.Collections.Generic.List<int> area_positions =
                        new Il2CppSystem.Collections.Generic.List<int>();
                    foreach (int slot_index in Get.SlotsPosition(__0, __1))
                    {
                        area_positions.Add(slot_index);
                    }
                    Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> item_list =
                        new Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry>();
                    foreach (ItemContainerEntry item_container_entry in __instance.content)
                    {
                        System.Collections.Generic.List<int> item_positions = Get.SlotsPosition(
                            item_container_entry.Position,
                            item_container_entry.size
                        );
                        foreach (int slot_position in item_positions)
                        {
                            if (area_positions.Contains(slot_position))
                            {
                                item_list.Add(item_container_entry);
                            }
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
                if (__instance.nameInputTMP.IsNullOrDestroyed())
                {
                    return;
                }
                configure_stash_name_backup = __instance.nameInputTMP.text;
                if (
                    __instance.contents.IsNullOrDestroyed()
                    || __instance.stashPriorityUI.IsNullOrDestroyed()
                )
                {
                    return;
                }
                GameObject content = __instance.contents.gameObject;
                if (!content.IsNullOrDestroyed())
                {
                    QuadStash_obj = Functions.GetChild(content, "quad_stash_row");
                    if (QuadStash_obj.IsNullOrDestroyed())
                    {
                        GameObject priority_section = Functions.FindDescendant(
                            content,
                            "Priority Section"
                        );
                        GameObject priority = __instance.stashPriorityUI.gameObject;
                        if (
                            (!priority_section.IsNullOrDestroyed())
                            && (!priority.IsNullOrDestroyed())
                        )
                        {
                            RectTransform priority_section_rect_transf =
                                priority_section.GetComponent<RectTransform>();
                            if (priority_section_rect_transf.IsNullOrDestroyed())
                            {
                                return;
                            }
                            float row_H = priority_section_rect_transf.rect.height;
                            float margin = 20 * priority_section_rect_transf.lossyScale.x; //Fix Hight resolution scaling

                            GameObject row = Functions.FindDescendant(priority, "Explanation Row");
                            if (row.IsNullOrDestroyed())
                            {
                                return;
                            }
                            QuadStash_obj = UnityEngine.Object.Instantiate(
                                row,
                                new Vector3(
                                    row.transform.position.x,
                                    (row.transform.position.y - row_H - margin),
                                    row.transform.position.z
                                ),
                                Quaternion.identity
                            );
                            QuadStash_obj.name = "quad_stash_row";
                            QuadStash_obj.transform.SetParent(content.transform);

                            GameObject toogle_obj = Functions.GetChild(
                                QuadStash_obj,
                                "Priority Toggle"
                            );
                            if (!toogle_obj.IsNullOrDestroyed())
                            {
                                configure_stash_toggle = toogle_obj.GetComponent<Toggle>();
                                PrepareToggle(configure_stash_toggle);
                            }
                            GameObject text_obj = Functions.GetChild(QuadStash_obj, "OptionText");
                            if (!text_obj.IsNullOrDestroyed())
                            {
                                GameObject title_obj = Functions.GetChild(text_obj, "Title");
                                if (!title_obj.IsNullOrDestroyed())
                                {
                                    configure_stash_toggle_title =
                                        title_obj.GetComponent<TextMeshProUGUI>();
                                }
                                GameObject explanation_obj = Functions.GetChild(
                                    text_obj,
                                    "Explanation"
                                );
                                if (!explanation_obj.IsNullOrDestroyed())
                                {
                                    configure_stash_toggle_explanation =
                                        explanation_obj.GetComponent<TextMeshProUGUI>();
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
                            PrepareToggle(configure_stash_toggle);
                            configure_stash_toggle.SetIsOnWithoutNotify(
                                Get.IsQuadStash(configure_tab_ui.selectedTabID)
                            );
                        }
                        if (!configure_stash_toggle_title.IsNullOrDestroyed())
                        {
                            configure_stash_toggle_title.text = toggle_str;
                        }
                        if (!configure_stash_toggle_explanation.IsNullOrDestroyed())
                        {
                            configure_stash_toggle_explanation.text = toggle_explain_str;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(
            typeof(StashTabbedUIControls),
            "HandleConfigureTabResult",
            new System.Type[] { typeof(Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult) }
        )]
        public class StashTabbedUIControls_HandleConfigureTabResult
        {
            static bool choiceValid;
            static bool wantQuad;

            static bool TryGetStashTab(
                StashTabbedUIControls ui,
                Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult result,
                out ItemContainer container
            )
            {
                container = null;
                if (
                    result.IsNullOrDestroyed()
                    || result.Action != Il2CppLE.UI.PanelSystem.StashConfigureResult.Save
                    || configure_tab_ui.IsNullOrDestroyed()
                    || ui != configure_tab_ui.parentUIController
                    || result.SelectedTabId != configure_tab_ui.selectedTabID
                    || string.IsNullOrEmpty(Save.Data.path)
                    || Save.Data.UserTabs.names == null
                    || stash_item_container.IsNullOrDestroyed()
                )
                {
                    return false;
                }
                int selected = result.SelectedTabId;
                if (selected < 0 || selected >= stash_item_container.containers.Count)
                {
                    return false;
                }
                container = stash_item_container.containers[selected];
                return !container.IsNullOrDestroyed() && container.id == ContainerID.STASH;
            }

            // Read the checkbox before the game closes the window and forces it back on.
            [HarmonyPrefix]
            static void Prefix(
                StashTabbedUIControls __instance,
                Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult __0
            )
            {
                choiceValid = false;
                if (
                    !TryGetStashTab(__instance, __0, out ItemContainer container)
                    || configure_stash_toggle.IsNullOrDestroyed()
                )
                {
                    return;
                }
                wantQuad = configure_stash_toggle.isOn;
                choiceValid = true;
                if (wantQuad)
                {
                    return;
                }
                if (!EjectOverflow(container, default_size))
                {
                    wantQuad = true;
                    Main.logger_instance?.Warning(
                        "Quad Stash stayed on because an item could not be moved to inventory or the ground."
                    );
                }
            }

            [HarmonyPostfix]
            static void Postfix(
                StashTabbedUIControls __instance,
                Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult __0
            )
            {
                if (!choiceValid || !TryGetStashTab(__instance, __0, out ItemContainer container))
                {
                    return;
                }
                choiceValid = false;
                Get.MigrateIdsFromNames();
                if (Save.Data.UserTabs.ids == null)
                {
                    Save.Data.UserTabs.ids = new System.Collections.Generic.List<int>();
                }
                int id = Get.TabStableId(__0.SelectedTabId);
                if (wantQuad)
                {
                    if (!Save.Data.UserTabs.ids.Contains(id))
                    {
                        Save.Data.UserTabs.ids.Add(id);
                    }
                }
                else
                {
                    Save.Data.UserTabs.ids.Remove(id);
                }
                Get.RememberQuadTabs();
                Save.Data.Save();
                SyncTabSize(__0.SelectedTabId);
                backup_active_tab = -1;
                gridRefreshFrames = 5;
                RefreshActiveGrid();
            }
        }

        [HarmonyPatch(typeof(ItemContainerUI), "OnUpdateTick")]
        public class ItemContainerUI_OnUpdateTick
        {
            // Bank_Quad's Unity LateUpdate is not invoked reliably by this Il2Cpp build.
            // Run deferred presentation from the stash UI's own update instead.
            [HarmonyPostfix]
            static void Postfix(ItemContainerUI __instance)
            {
                StashItemContainerUI ui = __instance.TryCast<StashItemContainerUI>();
                if (ui.IsNullOrDestroyed() || stash_item_container.IsNullOrDestroyed())
                {
                    return;
                }
                stash_item_container_ui = ui;
                if (!BindGridImage())
                {
                    return;
                }
                int active = stash_item_container.CurrentlyActiveTab;
                bool needsRefresh = (backup_active_tab != active) || (gridRefreshFrames > 0);
                if (needsRefresh && RefreshActiveGrid())
                {
                    backup_active_tab = active;
                    if (gridRefreshFrames > 0)
                    {
                        gridRefreshFrames--;
                    }
                }
                BeforeRender();
            }
        }
    }
}
