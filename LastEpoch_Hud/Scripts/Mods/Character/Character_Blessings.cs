using MelonLoader;
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
        static float openingDeadline;
        void Awake() { instance = this; }

        void Update()
        {
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
            if (!adding_blessings)
            {
                adding_blessings = true;

                Hud_Manager.Hud_Base.Resume_Click(); //Close Hud

                //Unlock all timelines
                if (!ItemContainersManager.Instance.IsNullOrDestroyed())
                {
                    System.Collections.Generic.List<TimelineID> timelines_id = new System.Collections.Generic.List<TimelineID>();
                    timelines_id.Add(TimelineID.UndeadAbom);
                    timelines_id.Add(TimelineID.OsprixWithLance);
                    timelines_id.Add(TimelineID.VoidRahyeh);
                    timelines_id.Add(TimelineID.FrostLich);
                    timelines_id.Add(TimelineID.Lagon);
                    timelines_id.Add(TimelineID.UndeadVsVoid);
                    timelines_id.Add(TimelineID.Dragons);
                    timelines_id.Add(TimelineID.Gaspar);
                    timelines_id.Add(TimelineID.Heorot);
                    timelines_id.Add(TimelineID.Volcano);
                    
                    if (BlessingRewardPanelManager.instance.IsNullOrDestroyed())
                    {
                        BlessingRewardPanelManager.instance?.OnOptionsPopulated(TimelineID.UndeadAbom, 0, 3);
                    }
                    if (!BlessingRewardPanelManager.instance.IsNullOrDestroyed())
                    {
                        GameObject ui = BlessingRewardPanelManager.instance.gameObject;
                        ui.active = true;
                        foreach (TimelineID t_id in timelines_id)
                        {
                            ItemContainersManager.Instance.populateBlessingOptions(t_id, 0, 3, 2);
                            BlessingRewardPanelManager.instance._selectedOption = 1;
                            BlessingRewardPanelManager.instance.ConfirmSelection();
                        }
                        ui.active = false;
                    }
                    else { Main.logger_instance?.Error("BlessingRewardPanelManager.instance is null"); }
                }
                //Add all blessing
                if (!Refs_Manager.item_list.IsNullOrDestroyed())
                {
                    int base_id = 34;
                    int index = 0;
                    bool found = false;
                    foreach (ItemList.BaseEquipmentItem n_item in Refs_Manager.item_list.EquippableItems)
                    {
                        if (n_item.baseTypeID == base_id) { found = true; break; }
                        index++;
                    }
                    if ((found) && (!Refs_Manager.player_data_tracker.IsNullOrDestroyed()))
                    {
                        foreach (ItemList.EquipmentItem item in Refs_Manager.item_list.EquippableItems[index].subItems)
                        {
                            //Add BlessingsDiscovered
                            /*bool blessing_already_in_player = false;
                            foreach (int blessing_id in Refs_Manager.player_data_tracker.charData.BlessingsDiscovered)
                            {
                                if (blessing_id == item.subTypeID)
                                {
                                    blessing_already_in_player = true;
                                    break;
                                }
                            }
                            if (!blessing_already_in_player) { Refs_Manager.player_data_tracker.charData.BlessingsDiscovered.Add(item.subTypeID); }
                            */

                            //Add OpenBlessings
                            bool blessing_data_already_in_player = false;
                            foreach (Il2CppLE.Data.BlessingData blessing_data in Refs_Manager.player_data_tracker.charData.OpenBlessings)
                            {
                                if (blessing_data.SubtypeId == item.subTypeID)
                                {
                                    blessing_data_already_in_player = true;
                                    break;
                                }
                            }
                            if (!blessing_data_already_in_player) { Refs_Manager.player_data_tracker.charData.OpenBlessings.Add(CreateBlessingDataForSave(System.Convert.ToUInt16(item.subTypeID))); }
                        }
                        Refs_Manager.player_data_tracker.charData.SaveData();
                    }
                    else { Main.logger_instance?.Error("Blessings not found in itemlist"); }
                }
                else { Main.logger_instance?.Error("ItemList is null"); }

                

                adding_blessings = false;
            }
        }

    }
}
