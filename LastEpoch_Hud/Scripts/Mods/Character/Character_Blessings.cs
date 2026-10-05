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
        static float openingDeadline;
        void Awake() { instance = this; }

        void Update()
        {
            if (choosing && (!Scenes.IsGameScene() || !IsBlessingOpen())) choosing = false;
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

        public static Il2CppLE.Data.BlessingData CreateBlessingDataForSave(ushort subtype)
        {
            Il2CppLE.Data.BlessingData result = new Il2CppLE.Data.BlessingData
            {
                SubtypeId = subtype,
                ImplicitRollByte0 = (byte)UnityEngine.Random.Range(0f, 255f),
                ImplicitRollByte1 = (byte)UnityEngine.Random.Range(0f, 255f),
                ImplicitRollByte2 = (byte)UnityEngine.Random.Range(0f, 255f)
            };

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
                            if (saved.SubtypeId == id) { hasRolls = true; break; }
                        if (!hasRolls)
                            tracker.charData.OpenBlessings.Add(CreateBlessingDataForSave((ushort)id));
                    }
                    break;
                }
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
                try
                {
                    var manager = ItemContainersManager.Instance;
                    var tracker = Refs_Manager.player_data_tracker;
                    if (__1.IsNullOrDestroyed() || __1.itemType != 34 ||
                        manager.IsNullOrDestroyed() || tracker.IsNullOrDestroyed() ||
                        manager.blessingStorage.IsNullOrDestroyed() ||
                        !manager.blessingStorage.BlessingIsUnlocked(__1.subType))
                    {
                        Main.logger_instance?.Msg("Discover this blessing before selecting it.");
                        return false;
                    }
                    var target = manager.blessings.GetContainerForBlessing(__1.subType);
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
                        if (saved.SubtypeId == __1.subType) { requested = saved; break; }
                    if (requested.IsNullOrDestroyed())
                        throw new System.InvalidOperationException("No discovered roll data exists for this blessing");
                    // Native swap updates containers, stats and serialization together.
                    manager.SwapBlessing(slot, requested);
                    tracker.charData.SaveData();
                    __instance.SelectTimelineForBlessingDisplayAndUpdateDropdown(slot);
                    Main.logger_instance?.Msg("Choose Blessings: selected blessing " + requested.SubtypeId +
                        " for timeline slot " + slot + ".");
                }
                catch (System.Exception ex)
                {
                    Main.logger_instance?.Error("Choose Blessings selection: " + ex.Message);
                }
                return false;
            }
        }
    }
}
