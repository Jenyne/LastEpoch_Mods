using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Character
{
    [RegisterTypeInIl2Cpp]
    public class Character_Blessings : MonoBehaviour
    {
        public static Character_Blessings instance { get; private set; }
        public Character_Blessings(System.IntPtr ptr) : base(ptr) { }

        static int openingStage, openingFrame;
        static bool choosing;
        static InventoryBlessingSlotUI hoveredBlessing;
        static int appliedFrame = -1;
        static float openingDeadline;
        void Awake() { instance = this; }

        void Update()
        {
            if (choosing && (!Scenes.IsGameScene() || !IsBlessingOpen()))
            { choosing = false; hoveredBlessing = null; }
            if (choosing && Input.GetKeyDown(KeyCode.Mouse0) && !hoveredBlessing.IsNullOrDestroyed())
                ApplyBlessing(Refs_Manager.InventoryPanelUI, hoveredBlessing.referenceBlessingID);
            if (openingStage == 0 || Time.frameCount <= openingFrame) return;
            if (!Scenes.IsGameScene() || Time.unscaledTime > openingDeadline)
            {
                Main.logger_instance?.Msg("Choose Blessings: inventory did not become ready.");
                openingStage = 0;
                return;
            }
            try
            {
                if (openingStage == 1)
                {
                    var ui = UIBase.instance;
                    if (ui.IsNullOrDestroyed()) return;
                    // Open the managed parent panel, including normal initialization.
                    ui.openInventory(true, false, false, 0, true);
                    openingStage = 2;
                    openingFrame = Time.frameCount;
                    return;
                }
                var inventory = Refs_Manager.InventoryPanelUI;
                if (inventory.IsNullOrDestroyed() || !inventory.gameObject.activeInHierarchy) return;
                openingStage = 0;
                inventory.OpenBlessingPanel(false, false);
                choosing = !inventory.blessingPanel.IsNullOrDestroyed() && inventory.blessingPanel.activeInHierarchy;
                if (inventory.blessingPanel.IsNullOrDestroyed() || !inventory.blessingPanel.activeInHierarchy)
                    Main.logger_instance?.Msg("Choose Blessings: inventory opened but blessing tab is not visible.");
            }
            catch (System.Exception ex)
            {
                openingStage = 0;
                Main.logger_instance?.Error("Could not open Choose Blessings: " + ex.Message);
            }
        }

        public static void ChooseBlessings()
        {
            if (!Scenes.IsGameScene() || instance.IsNullOrDestroyed() || UIBase.instance.IsNullOrDestroyed() ||
                Refs_Manager.player_data_tracker.IsNullOrDestroyed() ||
                ItemContainersManager.Instance.IsNullOrDestroyed())
            {
                Main.logger_instance?.Msg("Enter the game before choosing blessings.");
                return;
            }
            try
            {
                if (openingStage != 0) return;
                Hud_Manager.Hud_Base.Resume_Click();
                // Let the mod HUD finish closing before opening the game window.
                openingStage = 1;
                openingFrame = Time.frameCount;
                openingDeadline = Time.unscaledTime + 3f;
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("Could not open Choose Blessings: " + ex.Message);
            }
        }

        public static Il2CppLE.Data.BlessingData CreateBlessingDataForSave(ushort subtype, bool maxRolls = false)
        {
            var result = new Il2CppLE.Data.BlessingData
            {
                SubtypeId = subtype,
                ImplicitRollByte0 = 255,
                ImplicitRollByte1 = 255,
                ImplicitRollByte2 = 255
            };
            if (!maxRolls)
            {
                result.ImplicitRollByte0 = (byte)UnityEngine.Random.Range(0, 256);
                result.ImplicitRollByte1 = (byte)UnityEngine.Random.Range(0, 256);
                result.ImplicitRollByte2 = (byte)UnityEngine.Random.Range(0, 256);
            }
            return result;
        }

        private static bool adding_blessings = false;

        public static bool IsBlessingOpen()
        {
            bool result = false;
            if (!Refs_Manager.BlessingsPanel.IsNullOrDestroyed()) //Don't use .net6 nullable here
            {
                result = Refs_Manager.BlessingsPanel.active;
            }
            return result;
        }
        public static void DiscoverAllBlessings()
        {
            if (adding_blessings) return;
            var manager = ItemContainersManager.Instance;
            var tracker = Refs_Manager.player_data_tracker;
            var list = ItemList.get();
            if (!Scenes.IsGameScene() || manager.IsNullOrDestroyed() ||
                tracker.IsNullOrDestroyed() || tracker.charData.IsNullOrDestroyed() ||
                list.IsNullOrDestroyed() || manager.blessingStorage.IsNullOrDestroyed())
            {
                Main.logger_instance?.Msg("Enter the game before discovering blessings.");
                return;
            }
            adding_blessings = true;
            try
            {
                int unlocked = 0;
                foreach (var type in list.EquippableItems)
                {
                    if (type.baseTypeID != 34) continue;
                    foreach (var blessing in type.subItems)
                    {
                        int id = blessing.subTypeID;
                        // Shared storage controls whether the native UI considers it discovered.
                        if (!manager.blessingStorage.BlessingIsUnlocked(id))
                        {
                            manager.blessingStorage.AddUnlocked(id);
                            unlocked++;
                        }
                        if (!tracker.charData.BlessingsDiscovered.Contains(id))
                            tracker.charData.BlessingsDiscovered.Add(id);
                        bool hasRolls = false;
                        foreach (var saved in tracker.charData.OpenBlessings)
                            if (saved.SubtypeId == id)
                            {
                                // Discovery preserves rolls on blessings already owned.
                                hasRolls = true; break;
                            }
                        if (!hasRolls)
                            tracker.charData.OpenBlessings.Add(CreateBlessingDataForSave((ushort)id));
                    }
                    break;
                }
                tracker.charData.SaveItems(manager);
                tracker.charData.SaveData();
                Main.logger_instance?.Msg("Discover All Blessings: unlocked " + unlocked + " blessings.");
                ChooseBlessings();
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("Discover All Blessings: " + ex.Message);
            }
            finally { adding_blessings = false; }
        }

        public static void MaxOutBlessings()
        {
            if (adding_blessings) return;
            var manager = ItemContainersManager.Instance;
            var tracker = Refs_Manager.player_data_tracker;
            if (!Scenes.IsGameScene() || manager.IsNullOrDestroyed() ||
                manager.blessings.IsNullOrDestroyed() || manager.blessingStorage.IsNullOrDestroyed() ||
                tracker.IsNullOrDestroyed() || tracker.charData.IsNullOrDestroyed())
            {
                Main.logger_instance?.Msg("Enter the game before maximizing blessings.");
                return;
            }
            adding_blessings = true;
            try
            {
                int equippedCount = 0, savedCount = 0;
                // Use the native swap so equipped stats and save tracking update.
                // Do this first: swapping can return the old rolls to OpenBlessings.
                for (int slot = 0; slot < manager.blessings.Containers.Count; slot++)
                {
                    var container = manager.blessings.Containers[slot];
                    if (container.IsNullOrDestroyed() || !container.HasContent() ||
                        !container.TryGetContentItemData(out ItemData equipped) ||
                        equipped.IsNullOrDestroyed() || equipped.itemType != 34) continue;
                    EquipBlessing(manager, slot, CreateBlessingDataForSave((ushort)equipped.subType, true));
                    equippedCount++;
                }
                foreach (var saved in tracker.charData.OpenBlessings)
                {
                    saved.ImplicitRollByte0 = saved.ImplicitRollByte1 = saved.ImplicitRollByte2 = 255;
                    savedCount++;
                }
                tracker.charData.SaveItems(manager);
                tracker.charData.SaveData();
                Main.logger_instance?.Msg("Max Out Blessings: maximized " + savedCount +
                    " discovered rolls and " + equippedCount + " equipped blessings.");
                ChooseBlessings();
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("Max Out Blessings: " + ex.Message);
            }
            finally { adding_blessings = false; }
        }

        public static void UnlockBlessingSlots()
        {
            var manager = ItemContainersManager.Instance;
            var tracker = Refs_Manager.player_data_tracker;
            if (!Scenes.IsGameScene() || manager.IsNullOrDestroyed() ||
                manager.blessings.IsNullOrDestroyed() || manager.blessingStorage.IsNullOrDestroyed() ||
                tracker.IsNullOrDestroyed() || tracker.charData.IsNullOrDestroyed())
            {
                Main.logger_instance?.Msg("Enter the game before unlocking blessing slots.");
                return;
            }
            try
            {
                int opened = 0, missing = 0;
                for (int slot = 0; slot < manager.blessings.Containers.Count; slot++)
                {
                    var container = manager.blessings.Containers[slot];
                    if (container.IsNullOrDestroyed() || container.HasContent()) continue;
                    Il2CppLE.Data.BlessingData candidate = null;
                    foreach (var saved in tracker.charData.OpenBlessings)
                    {
                        if (!manager.blessingStorage.BlessingIsUnlocked(saved.SubtypeId)) continue;
                        var target = manager.blessings.GetContainerForBlessing(saved.SubtypeId);
                        if (!target.IsNullOrDestroyed() && target.GetContainerID().Equals(container.GetContainerID()))
                        { candidate = saved; break; }
                    }
                    if (candidate.IsNullOrDestroyed()) { missing++; continue; }
                    // An empty slot has no separate unlock flag. Equip a compatible,
                    // discovered blessing via the native path; preserve occupied slots.
                    EquipBlessing(manager, slot, candidate);
                    opened++;
                }
                tracker.charData.SaveData();
                Main.logger_instance?.Msg("Unlock Blessing Slots: opened " + opened + " slots." +
                    (missing > 0 ? " Use Discover All Blessings first for the remaining " + missing + " slots." : ""));
                ChooseBlessings();
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Error("Unlock Blessing Slots: " + ex.Message);
            }
        }

        static void EquipBlessing(ItemContainersManager manager, int slot, Il2CppLE.Data.BlessingData blessing)
        {
            var container = manager.blessings.Containers[slot];
            if (container.HasContent())
                manager.SwapBlessing(slot, blessing);
            else
            {
                // SwapBlessing replaces an existing entry; it does not initialize
                // an empty slot. Add through the typed container so native equip
                // events, stats and save tracking run for the first blessing too.
                var item = Il2CppLE.Data.BlessingDataFactory.Convert(blessing);
                if (item.IsNullOrDestroyed() || item.itemType != 34 || item.subType != blessing.SubtypeId ||
                    !container.TryAddItem(item, 1, Context.DEFAULT))
                    throw new System.InvalidOperationException("Native container rejected blessing " +
                        blessing.SubtypeId + " for slot " + slot);
            }
            if (!container.TryGetContentItemData(out ItemData equipped) || equipped.IsNullOrDestroyed() ||
                equipped.subType != blessing.SubtypeId)
                throw new System.InvalidOperationException("Blessing was not equipped in slot " + slot);
        }

        static void ApplyBlessing(InventoryPanelUI inventory, int blessingId)
        {
            if (inventory.IsNullOrDestroyed() || appliedFrame == Time.frameCount) return;
            appliedFrame = Time.frameCount;
                try
                {
                    var manager = ItemContainersManager.Instance;
                    var tracker = Refs_Manager.player_data_tracker;
                    if (blessingId < 0 || manager.IsNullOrDestroyed() || tracker.IsNullOrDestroyed() ||
                        manager.blessingStorage.IsNullOrDestroyed() ||
                        !manager.blessingStorage.BlessingIsUnlocked(blessingId))
                    {
                        Main.logger_instance?.Msg("Discover this blessing before selecting it.");
                        return;
                    }
                    var target = manager.blessings.GetContainerForBlessing(blessingId);
                    if (target.IsNullOrDestroyed())
                        throw new System.InvalidOperationException("No timeline container accepts this blessing");
                    int slot = -1;
                    for (int i = 0; i < manager.blessings.Containers.Count; i++)
                        if (manager.blessings.Containers[i].GetContainerID().Equals(target.GetContainerID()))
                        { slot = i; break; }
                    if (slot < 0)
                        throw new System.InvalidOperationException("Blessing timeline slot was not found");
                    Il2CppLE.Data.BlessingData requested = null;
                    foreach (var saved in tracker.charData.OpenBlessings)
                        if (saved.SubtypeId == blessingId) { requested = saved; break; }
                    if (requested.IsNullOrDestroyed())
                        throw new System.InvalidOperationException("No discovered roll data exists for this blessing");
                    // Native swap updates containers, stats and serialization together.
                    EquipBlessing(manager, slot, requested);
                    tracker.charData.SaveData();
                    inventory.SelectTimelineForBlessingDisplayAndUpdateDropdown(slot);
                    Main.logger_instance?.Msg("Choose Blessings: selected blessing " + requested.SubtypeId +
                        " for timeline slot " + slot + ".");
                }
                catch (System.Exception ex)
                {
                    Main.logger_instance?.Error("Choose Blessings selection: " + ex.Message);
                }

        }

        [HarmonyPatch(typeof(InventoryBlessingSlotUI), "UnityEngine_EventSystems_IPointerEnterHandler_OnPointerEnter")]
        public class HoverBlessing
        {
            [HarmonyPostfix]
            static void Postfix(InventoryBlessingSlotUI __instance)
            {
                hoveredBlessing = null;
                if (!choosing || Refs_Manager.InventoryPanelUI.IsNullOrDestroyed()) return;
                // Accept only actual discovered-grid slots, never the equipped row.
                foreach (var slot in Refs_Manager.InventoryPanelUI.discoveredBlessingsSlots)
                    if (!slot.IsNullOrDestroyed() && slot.Pointer == __instance.Pointer)
                    { hoveredBlessing = __instance; break; }
            }
        }
        [HarmonyPatch(typeof(InventoryBlessingSlotUI), "UnityEngine_EventSystems_IPointerExitHandler_OnPointerExit")]
        public class LeaveBlessing
        {
            [HarmonyPostfix]
            static void Postfix(InventoryBlessingSlotUI __instance)
            {
                if (!hoveredBlessing.IsNullOrDestroyed() && hoveredBlessing.Pointer == __instance.Pointer)
                    hoveredBlessing = null;
            }
        }

        [HarmonyPatch(typeof(InventoryPanelUI), "SetSelectBlessingIndex")]
        public class SelectBlessing
        {
            [HarmonyPrefix]
            static bool Prefix(InventoryPanelUI __instance, int __0, ItemDataUnpacked __1)
            {
                // Only the screen opened by the mod gets direct selection behavior.
                if (!choosing || !__instance.gameObject.activeInHierarchy ||
                    __instance.blessingPanel.IsNullOrDestroyed() || !__instance.blessingPanel.activeInHierarchy)
                    return true;
                if (!__1.IsNullOrDestroyed() && __1.itemType == 34)
                    ApplyBlessing(__instance, __1.subType);
                return false;
            }
        }
    }
}
