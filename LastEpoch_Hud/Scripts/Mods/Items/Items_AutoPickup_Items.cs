using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_AutoPickup_Items
    {
        [HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
        public class GroundItemManager_dropItemForPlayer
        {
            [HarmonyPrefix]
            static bool Prefix(ref GroundItemManager __instance, ref Actor __0, ref ItemData __1, ref UnityEngine.Vector3 __2, bool __3)
            {
                long profAlloc;
                long profStart = Diagnostics.DiagnosticsDumper.BeginOperation(out profAlloc);
                bool result = true;
                if (Scenes.IsGameScene())
                {
                    //Fix Load items with more than 4 affixs
                    if ((__1.rarity == 5) || (__1.rarity == 6))
                    {
                        __1.rarity = 4;
                        __1.RefreshIDAndValues();
                    }
                    //AutoPickup / AutoSell                    
                    ItemDataUnpacked item = __1.TryCast<ItemDataUnpacked>();
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (!item.IsNullOrDestroyed()))
                    {
                        if (((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Keys) && (Item.isKey(__1.itemType))) ||
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_WovenEchoes) && (__1.itemType == 107)) ||
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Materials) && (ItemList.isCraftingItem(__1.itemType))))
                        {
                            long pickupAlloc;
                            long pickupStart = Diagnostics.DiagnosticsDumper.BeginOperation(out pickupAlloc);
                            bool pickup = ItemContainersManager.Instance.attemptToPickupItem(__1, __0.position());
                            Diagnostics.DiagnosticsDumper.EndOperation("AutoPickup.AttemptPickup", pickupStart, pickupAlloc);
                            if (pickup) { result = false; }
                        }
                        else if ((__1.itemType < 34) &&
                            (!Refs_Manager.filter_manager.IsNullOrDestroyed()) &&
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter) ||
                            (Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_FromFilter)))
                        {
                            if (!Refs_Manager.filter_manager.Filter.IsNullOrDestroyed())
                            {
                                bool FilterShow = false;
                                int playerLevel = Refs_Manager.player_actor.IsNullOrDestroyed()
                                    ? 0
                                    : Refs_Manager.player_actor.stats.level;

                                long filterAlloc;
                                long filterStart = Diagnostics.DiagnosticsDumper.BeginOperation(out filterAlloc);
                                foreach (Rule rule in Refs_Manager.filter_manager.Filter.rules)
                                {
                                    if ((rule.isEnabled) && (rule.Match(item, playerLevel))) //&&
                                        //(((rule.levelDependent) && (rule.LevelInBounds(__0.stats.level))) ||
                                        //(!rule.levelDependent)))
                                    {
                                        if (rule.type == Rule.RuleOutcome.SHOW) //|| (rule.type == Rule.RuleOutcome.HIGHLIGHT_DEPRECATED))
                                        {
                                            FilterShow = true;
                                            break;
                                        }
                                    }
                                }
                                Diagnostics.DiagnosticsDumper.EndOperation("LootFilter.MatchDrop", filterStart, filterAlloc);
                                if ((FilterShow) && (Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter))
                                {
                                    long pickupAlloc;
                                    long pickupStart = Diagnostics.DiagnosticsDumper.BeginOperation(out pickupAlloc);
                                    bool pickup = ItemContainersManager.Instance.attemptToPickupItem(__1, __0.position());
                                    Diagnostics.DiagnosticsDumper.EndOperation("AutoPickup.AttemptPickup", pickupStart, pickupAlloc);
                                    if (pickup) { result = false; }
                                }
                                else if ((!FilterShow) && (Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_FromFilter))
                                {
                                    __0.goldTracker.modifyGold(item.VendorSaleValue);
                                    result = false;
                                }
                            }
                        }
                    }
                }

                Diagnostics.DiagnosticsDumper.EndOperation("Loot.DropProcessing", profStart, profAlloc);
                return result;
            }
        }
    }
}