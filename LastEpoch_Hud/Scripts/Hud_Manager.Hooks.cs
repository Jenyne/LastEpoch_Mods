using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppRewired.Components;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Hooks
    {
        //Select Shards
        [HarmonyPatch(typeof(Button), "Press")]
        public class Button_Press
        {
            [HarmonyPostfix]
            static void Postfix(ref Button __instance)
            {
                if (Content.OdlForceDrop.enable)
                {
                    if (__instance.name.Contains(Content.OdlForceDrop.shard_btn_name))
                    {
                        try
                        {
                            int i = System.Convert.ToInt32(__instance.name.Split('_')[1]);
                            //GameObject shard_id_object = Functions.GetChild(shard_btn_object, "shard_id");
                            //Text shard_id = shard_id_object.GetComponent<Text>();

                            GameObject shard_name_object = Functions.GetChild(
                                __instance.gameObject,
                                "shard_name"
                            );
                            Text shard_name = shard_name_object.GetComponent<Text>();
                            Content.OdlForceDrop.SelectShard(i, shard_name.text);
                        }
                        catch { }
                    }
                }
            }
        }

        //All Hooks have to be replace by Unity Actions
        [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
        public class Toggle_OnPointerClick
        {
            [HarmonyPostfix]
            static void Postfix(
                ref Toggle __instance,
                UnityEngine.EventSystems.PointerEventData __0
            )
            {
                if (
                    (!hud_object.IsNullOrDestroyed())
                    && (!Save_Manager.instance.IsNullOrDestroyed())
                    && (!Refs_Manager.player_data.IsNullOrDestroyed())
                )
                {
                    if (hud_object.active) //&& (!Save_Manager.instance.data.IsNullOrDestroyed()))
                    {
                        if (__instance.name.Contains("Toggle_Character_"))
                        {
                            switch (__instance.name)
                            {
                                case "Toggle_Character_Data_Died":
                                {
                                    if (
                                        (!Refs_Manager.player_data.IsNullOrDestroyed())
                                        && (!Content.Character.Data.died_toggle.IsNullOrDestroyed())
                                    )
                                    {
                                        Refs_Manager.player_data.Died = Content
                                            .Character
                                            .Data
                                            .died_toggle
                                            .isOn;
                                    }
                                    break;
                                }
                                case "Toggle_Character_Data_Hardcore":
                                {
                                    if (
                                        (!Refs_Manager.player_data.IsNullOrDestroyed())
                                        && (
                                            !Content.Character.Data.hardcore_toggle.IsNullOrDestroyed()
                                        )
                                    )
                                    {
                                        Refs_Manager.player_data.Hardcore = Content
                                            .Character
                                            .Data
                                            .hardcore_toggle
                                            .isOn;
                                    }
                                    break;
                                }
                                case "Toggle_Character_Data_Masochist":
                                {
                                    if (
                                        (!Refs_Manager.player_data.IsNullOrDestroyed())
                                        && (
                                            !Content.Character.Data.masochist_toggle.IsNullOrDestroyed()
                                        )
                                    )
                                    {
                                        Refs_Manager.player_data.Masochist = Content
                                            .Character
                                            .Data
                                            .masochist_toggle
                                            .isOn;
                                    }
                                    break;
                                }
                                case "Toggle_Character_Data_Portal":
                                {
                                    if (
                                        (!Refs_Manager.player_data.IsNullOrDestroyed())
                                        && (
                                            !Content.Character.Data.portal_toggle.IsNullOrDestroyed()
                                        )
                                    )
                                    {
                                        Refs_Manager.player_data.PortalUnlocked = Content
                                            .Character
                                            .Data
                                            .portal_toggle
                                            .isOn;
                                    }
                                    break;
                                }
                                case "Toggle_Character_Data_SoloChallenge":
                                {
                                    if (
                                        (!Refs_Manager.player_data.IsNullOrDestroyed())
                                        && (!Content.Character.Data.solo_toggle.IsNullOrDestroyed())
                                    )
                                    {
                                        Refs_Manager.player_data.SoloChallenge = Content
                                            .Character
                                            .Data
                                            .solo_toggle
                                            .isOn;
                                    }
                                    break;
                                }
                                case "Toggle_Character_Cheats_TwoHandeWithShield":
                                {
                                    if (
                                        !Content.Character.Cheats.twohanded_shield_toggle.IsNullOrDestroyed()
                                    )
                                    {
                                        Save_Manager
                                            .instance
                                            .data
                                            .Character
                                            .Cheats
                                            .Enable_TwoHandedWithShield = Content
                                            .Character
                                            .Cheats
                                            .twohanded_shield_toggle
                                            .isOn;
                                    }
                                    break;
                                }

                                case "Toggle_Character_Buffs_Enable":
                                {
                                    Save_Manager.instance.data.Character.PermanentBuffs.Enable_Mod =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_MoveSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_MoveSpeed_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Damage":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Damage_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_AttackSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_AttackSpeed_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_CastingSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_CastSpeed_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_CriticalChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_CriticalChance_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_CriticalMultiplier":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_CriticalMultiplier_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_HealthRegen":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_HealthRegen_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_ManaRegen":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_ManaRegen_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Strenght":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Str_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Intelligence":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Int_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Dexterity":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Dex_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Vitality":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Vit_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_Attunement":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_Att_Buff = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Character_Buffs_AreaOfEffect":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Enable_AoE_Buff = __instance.isOn;
                                    break;
                                }
                            }
                        }
                        else if (__instance.name.Contains("Toggle_Items_"))
                        {
                            switch (__instance.name)
                            {
                                case "Toggle_Items_Drop_ForceUnique":
                                {
                                    if (__instance.isOn)
                                    {
                                        Save_Manager.instance.data.Items.Drop.Enable_ForceSet =
                                            false;
                                        Save_Manager
                                            .instance
                                            .data
                                            .Items
                                            .Drop
                                            .Enable_ForceLegendary = false;
                                        Content.Items.Drop.force_set_toggle.isOn = false;
                                        Content.Items.Drop.force_legendary_toggle.isOn = false;
                                    }
                                    Save_Manager.instance.data.Items.Drop.Enable_ForceUnique =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_ForceSet":
                                {
                                    if (__instance.isOn)
                                    {
                                        Save_Manager.instance.data.Items.Drop.Enable_ForceUnique =
                                            false;
                                        Save_Manager
                                            .instance
                                            .data
                                            .Items
                                            .Drop
                                            .Enable_ForceLegendary = false;
                                        Content.Items.Drop.force_unique_toggle.isOn = false;
                                        Content.Items.Drop.force_legendary_toggle.isOn = false;
                                    }
                                    Save_Manager.instance.data.Items.Drop.Enable_ForceSet =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_ForceLegendary":
                                {
                                    if (__instance.isOn)
                                    {
                                        Save_Manager.instance.data.Items.Drop.Enable_ForceUnique =
                                            false;
                                        Save_Manager.instance.data.Items.Drop.Enable_ForceSet =
                                            false;
                                        Content.Items.Drop.force_set_toggle.isOn = false;
                                        Content.Items.Drop.force_unique_toggle.isOn = false;
                                    }
                                    Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_Implicits":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_Implicits =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_ForginPotencial":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_ForginPotencial =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_ForceSeal":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_ForceSeal =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_SealTier":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_SealTier =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_SealValue":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_SealValue =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_NbAffixes":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_AffixCount =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_AffixesTiers":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_AffixTiers =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_AffixesValues":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_AffixValues =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_UniqueMods":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_UniqueMods =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_LegendaryPotencial":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Drop
                                        .Enable_LegendaryPotencial = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Drop_WeaverWill":
                                {
                                    Save_Manager.instance.data.Items.Drop.Enable_WeaverWill =
                                        __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Pickup_AutoPickup_Gold":
                                {
                                    Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Gold =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_Keys":
                                {
                                    Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Keys =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_Pots":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_Potions = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_XpTome":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_XpTome = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_FavorTome":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_FavorTome = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_MemoryAmber":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_MemoryAmber = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_WovenEchoes":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_WovenEchoes = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_Materials":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_Materials = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoPickup_Filters":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoPickup_FromFilter = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoStore_OnDrop":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoStore_OnDrop = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoStore_OnInventoryOpen":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoStore_OnInventoryOpen = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoStore_Timer":
                                {
                                    Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_Timer =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoSell_FromFilter":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoSell_FromFilter = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoShatter_FromFilter":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoShatter_FromFilter = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_AutoShatter_Rune":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_AutoShatter_UseRune = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Pickup_Range_Pickup":
                                {
                                    Save_Manager.instance.data.Items.Pickup.Enable_RangePickup =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Pickup_Hide_Notifications":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .Enable_HideMaterialsNotifications = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_Enable":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Enable_Mod =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_ForginPotencial":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_ForginPotencial = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_Implicit0":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Implicit_0 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_Implicit1":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Implicit_1 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_Implicit2":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Implicit_2 = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_SealTier":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Enable_Seal_Tier =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_SealValue":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Seal_Value = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_AffixTier0":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_0_Tier = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixTier1":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_1_Tier = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixTier2":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_2_Tier = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixTier3":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_3_Tier = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_AffixValue0":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_0_Value = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixValue1":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_1_Value = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixValue2":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_2_Value = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_AffixValue3":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_Affix_3_Value = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_UniqueMod0":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_0 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod1":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_1 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod2":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_2 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod3":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_3 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod4":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_4 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod5":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_5 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod6":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_6 = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_UniqueMod7":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_UniqueMod_7 = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Items_Craft_LegendaryPotencial":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_LegendaryPotencial = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Items_Craft_WeaverWill":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .Enable_WeaverWill = __instance.isOn;
                                    break;
                                }
                            }
                        }
                        else if (__instance.name.Contains("Toggle_Scenes_"))
                        {
                            switch (__instance.name)
                            {
                                case "Toggle_Scenes_Camera_Enable":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_Mod =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_ZoomMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_ZoomMinimum =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_ZoomPerScroll":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_ZoomPerScroll =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_ZoomSpeed":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_ZoomSpeed =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_DefaultRotation":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Camera
                                        .Enable_DefaultRotation = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_OffsetMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMinimum =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_OffsetMaximum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMaximum =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_AngleMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_AngleMinimum =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_AngleMaximum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_AngleMaximum =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Camera_LoadOnStart":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.Enable_LoadOnStart =
                                        __instance.isOn;
                                    break;
                                }

                                case "Toggle_Scenes_Dungeons_EnterWithoutKey":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Dungeons
                                        .Enable_EnterWithoutKey = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Scenes_Minimap_MaxZoomOut":
                                {
                                    Save_Manager.instance.data.Scenes.Minimap.Enable_MaxZoomOut =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Minimap_RemoveFogOfWar":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Minimap
                                        .Enable_RemoveFogOfWar = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Scenes_Monoliths_MaxStability":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_MaxStability = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_MobsDensity":
                                {
                                    Save_Manager.instance.data.Scenes.Monoliths.Enable_MobsDensity =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_MobsDefeatOnStart":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_MobsDefeatOnStart = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_BlessingSlots":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_BlessingSlots = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_MaxStabilityOnStart":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_MaxStabilityOnStart = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_MaxStabilityOnStabilityChanged":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_MaxStabilityOnStabilityChanged = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_ObjectiveReveal":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_ObjectiveReveal = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_CompleteObjective":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_CompleteObjective = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scenes_Monoliths_NoLostWhenDie":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Scenes
                                        .Monoliths
                                        .Enable_NoLostWhenDie = __instance.isOn;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            switch (__instance.name)
                            {
                                //Skills
                                case "Toggle_RemoveManaCost":
                                {
                                    Save_Manager.instance.data.Skills.Enable_RemoveManaCost =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_RemoveChannelCost":
                                {
                                    Save_Manager.instance.data.Skills.Enable_RemoveChannelCost =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_ManaRegenWhenChanneling":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Enable_NoManaRegenWhileChanneling = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DontStopWhenOOM":
                                {
                                    Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_NoCooldown":
                                {
                                    Save_Manager.instance.data.Skills.Enable_RemoveCooldown =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_UnlockAllSkills":
                                {
                                    Save_Manager.instance.data.Skills.Enable_AllSkills =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_RemoveNodeRequirements":
                                {
                                    Save_Manager.instance.data.Skills.Disable_NodeRequirement =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_SpecializationSlots":
                                {
                                    Save_Manager.instance.data.Skills.Enable_SpecializationSlots =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_SkillLevel":
                                {
                                    Save_Manager.instance.data.Skills.Enable_SkillLevel =
                                        __instance.isOn;
                                    Mods.Skills.Skills_Level.Sync();
                                    break;
                                }
                                case "Toggle_SkillLevelMultiplier":
                                {
                                    Save_Manager.instance.data.Skills.Enable_SkillLevelMultiplier =
                                        __instance.isOn;
                                    Mods.Skills.Skills_Level.Sync();
                                    break;
                                }
                                case "Toggle_PassivePoints":
                                {
                                    Save_Manager.instance.data.Skills.Enable_PassivePoints =
                                        __instance.isOn;
                                    Mods.Skills.Passives_Points.Sync();
                                    break;
                                }
                                case "Toggle_PassivePointMultiplier":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Enable_PassivePointMultiplier = __instance.isOn;
                                    Mods.Skills.Passives_Points.Sync();
                                    break;
                                }
                                case "Toggle_Weaver_PointMultiplier":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Factions
                                        .TheWoven
                                        .Enable_PointMultiplier = __instance.isOn;
                                    Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                                    break;
                                }
                                case "Toggle_NoTarget":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .MovementSkills
                                        .Enable_NoTarget = __instance.isOn;
                                    break;
                                }
                                case "Toggle_ImmuneDuringMovement":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .MovementSkills
                                        .Enable_ImmuneDuringMovement = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DisableSimplePath":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .MovementSkills
                                        .Disable_SimplePath = __instance.isOn;
                                    break;
                                }
                                //Companions
                                case "Toggle_MaximumCompanions":
                                {
                                    Save_Manager.instance.data.Skills.Companion.Enable_Limit =
                                        __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wolf_SummonToMax":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Companion
                                        .Wolf
                                        .Enable_SummonMax = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wolf_SummonLimit":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Companion
                                        .Wolf
                                        .Enable_SummonLimit = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wolf_StunImmunity":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Companion
                                        .Wolf
                                        .Enable_StunImmunity = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Scorpions_SummonLimit":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Companion
                                        .Scorpion
                                        .Enable_BabyQuantity = __instance.isOn;
                                    break;
                                }
                                //Minions
                                case "Toggle_Skeleteon_SummonQuantityFromPassive":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_additionalSkeletonsFromPassives = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleteon_SummonQuantityFromSkillTree":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_additionalSkeletonsFromSkillTree = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleteon_SummonQuantityPerCast":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_additionalSkeletonsPerCast = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleteon_ChanceToResummonOnDeath":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_chanceToResummonOnDeath = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleton_ForceArcher":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_forceArcher = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleton_ForceBrawler":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_forceBrawler = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Skeleton_ForceWarrior":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .Enable_forceWarrior = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Wraiths_SummonMax":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .Enable_additionalMaxWraiths = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wraiths_Delayed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .Enable_delayedWraiths = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wraiths_CastSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .Enable_increasedCastSpeed = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wraiths_DisableLimitTo2":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .Enable_limitedTo2Wraiths = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Wraiths_DisableDecay":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .Enable_wraithsDoNotDecay = __instance.isOn;
                                    break;
                                }

                                case "Toggle_Mages_SummonQuantityFromPassive":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_additionalSkeletonsFromPassives = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_SummonQuantityFromSkillTree":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_additionalSkeletonsFromSkillTree = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_SummonQuantityFromItems":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_additionalSkeletonsFromItems = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_SummonPerCast":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_additionalSkeletonsPerCast = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_ChanceForExtraPorjectiles":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_chanceForTwoExtraProjectiles = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_ForceCryomancer":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_forceCryomancer = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_ForceDeathKnight":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_forceDeathKnight = __instance.isOn;
                                    break;
                                }
                                case "Toggle_Mages_ForcePyromancer":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .Enable_forcePyromancer = __instance.isOn;
                                    break;
                                }

                                case "Toggle_BoneGolem_GolemPerSkeletons":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_addedGolemsPer4Skeletons = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_SelfResurectChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_selfResurrectChance = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_IncreaseFireAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_increasedFireAuraArea = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_IncreaseArmorAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_undeadArmorAura = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_IncreaseMoveSpeedAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_undeadMovespeedAura = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_IncreaseMoveSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_increasedMoveSpeed = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_Twins":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_twins = __instance.isOn;
                                    break;
                                }
                                case "Toggle_BoneGolem_Slam":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .Enable_hasSlamAttack = __instance.isOn;
                                    break;
                                }

                                case "Toggle_VolatileZombies_ChanceOnMinionDeath":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .Enable_chanceToCastFromMinionDeath = __instance.isOn;
                                    break;
                                }
                                case "Toggle_VolatileZombies_InfernalShadeChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .Enable_chanceToCastInfernalShadeOnDeath = __instance.isOn;
                                    break;
                                }
                                case "Toggle_VolatileZombies_MarrowShardsChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .Enable_chanceToCastMarrowShardsOnDeath = __instance.isOn;
                                    break;
                                }

                                case "Toggle_DreadShades_Duration":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_Duration = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DreadShades_Max":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_Max = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DreadShades_Decay":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_ReduceDecay = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DreadShades_Radius":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_Radius = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DreadShades_DisableLimit":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_DisableLimit = __instance.isOn;
                                    break;
                                }
                                case "Toggle_DreadShades_DisableHealthDrain":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .DreadShades
                                        .Enable_DisableHealthDrain = __instance.isOn;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Slider), "Set", new System.Type[] { typeof(float), typeof(bool) })]
        public class Slider_Set
        {
            [HarmonyPostfix]
            static void Postfix(Slider __instance, bool sendCallback)
            {
                if (!sendCallback || Content.Character.Data.suppressWeaverSlider)
                {
                    return;
                }
                if (__instance.IsNullOrDestroyed() || __instance.name != "Slider_Weaver_TreePoints")
                {
                    return;
                }
                if (
                    hud_object.IsNullOrDestroyed()
                    || !hud_object.active
                    || Save_Manager.instance.IsNullOrDestroyed()
                )
                {
                    return;
                }
                Content.Character.Data.SetWeaverTreePoints(__instance.value);
            }
        }

        [HarmonyPatch(typeof(Slider), "set_value")]
        public class Slider_set_value
        {
            [HarmonyPostfix]
            static void Postfix(ref Slider __instance, float __0)
            {
                if (Content.Character.Data.suppressWeaverSlider)
                {
                    return;
                }
                if (
                    !(hud_object.IsNullOrDestroyed())
                    && (!Save_Manager.instance.IsNullOrDestroyed())
                )
                {
                    if ((hud_object.active) && (!Refs_Manager.player_data.IsNullOrDestroyed()))
                    {
                        if (__instance.name == "Slider_Weaver_TreePoints")
                        {
                            Content.Character.Data.SetWeaverTreePoints(__instance.value);
                            return;
                        }
                        if (__instance.name == "Slider_Weaver_PointMultiplier")
                        {
                            int multiplier = SettingRow.Clamp(__0);
                            Save_Manager.instance.data.Factions.TheWoven.PointMultiplier =
                                multiplier;
                            if (!Content.Character.Data.weaver_multiplier_text.IsNullOrDestroyed())
                            {
                                Content.Character.Data.weaver_multiplier_text.text =
                                    multiplier + "x";
                            }
                            Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                            return;
                        }
                        if (__instance.name.Contains("Slider_Character_"))
                        {
                            switch (__instance.name)
                            {
                                case "Slider_Character_Cheats_AutoPotions":
                                {
                                    Save_Manager.instance.data.Character.Cheats.autoPot = __0;
                                    //Content.Character.Cheats.autopot_text.text = (int)((Save_Manager.instance.data.Character.Cheats.autoPot / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Cheats_DensityMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.DensityMultiplier =
                                        __0;
                                    //Content.Character.Cheats.density_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.DensityMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_ExperienceMultiplier":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .Cheats
                                        .ExperienceMultiplier = __0;
                                    //Content.Character.Cheats.experience_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ExperienceMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_AbilityMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.AbilityMultiplier =
                                        __0;
                                    //Content.Character.Cheats.ability_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.AbilityMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_FavorMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.FavorMultiplier =
                                        __0;
                                    //Content.Character.Cheats.favor_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.FavorMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_MemoryAmberMultiplier":
                                {
                                    uint multiplier = (uint)
                                        Mathf.Clamp(Mathf.RoundToInt(__0), 1, 10000);
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .Cheats
                                        .MemoryAmberMultiplier = multiplier;
                                    if (
                                        !Content.Character.Cheats.memoryamber_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content.Character.Cheats.memoryamber_text.text =
                                            "x " + multiplier;
                                    }
                                    break;
                                }
                                case "Slider_Character_Cheats_ItemDropMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier =
                                        __0;
                                    //Content.Character.Cheats.itemdropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_ItemDropChance":
                                {
                                    Save_Manager.instance.data.Character.Cheats.ItemDropChance =
                                        __0;
                                    //Content.Character.Cheats.itemdropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.ItemDropChance / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Cheats_GoldDropMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier =
                                        __0;
                                    //Content.Character.Cheats.golddropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier);
                                    break;
                                }
                                case "Slider_Character_Cheats_GoldDropChance":
                                {
                                    Save_Manager.instance.data.Character.Cheats.GoldDropChance =
                                        __0;
                                    //Content.Character.Cheats.golddropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.GoldDropChance / 255) * 100) + " %";
                                    break;
                                }
                                //Data
                                case "Slider_Character_Data_Deaths":
                                {
                                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                    {
                                        Refs_Manager.player_data.Deaths = (int)__0;
                                    }

                                    //Content.Character.Data.deaths_text.text = ((int)__0).ToString();
                                    break;
                                }
                                case "Slider_Character_Data_LanternLuminance":
                                {
                                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                    {
                                        Refs_Manager.player_data.LanternLuminance = (int)__0;
                                    }
                                    //Content.Character.Data.lantern_text.text = ((int)__0).ToString();
                                    break;
                                }
                                //Buffs
                                case "Slider_Character_Buffs_MoveSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .MoveSpeed_Buff_Value = __0;
                                    //Content.Character.Buffs.movespeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.MoveSpeed_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Damage":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Damage_Buff_Value = __0;
                                    //Content.Character.Buffs.damage_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Damage_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_AttackSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .AttackSpeed_Buff_Value = __0;
                                    //Content.Character.Buffs.attackspeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.AttackSpeed_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_CastingSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .CastSpeed_Buff_Value = __0;
                                    //Content.Character.Buffs.castingspeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CastSpeed_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_CriticalChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .CriticalChance_Buff_Value = __0;
                                    //Content.Character.Buffs.criticalchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_CriticalMultiplier":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .CriticalMultiplier_Buff_Value = __0;
                                    //Content.Character.Buffs.criticalmultiplier_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CriticalMultiplier_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_HealthRegen":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .HealthRegen_Buff_Value = __0;
                                    //Content.Character.Buffs.healthregen_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.HealthRegen_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_ManaRegen":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .ManaRegen_Buff_Value = __0;
                                    //Content.Character.Buffs.manaregen_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.ManaRegen_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Strenght":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Str_Buff_Value = __0;
                                    //Content.Character.Buffs.str_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Str_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Intelligence":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Int_Buff_Value = __0;
                                    //Content.Character.Buffs.int_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Int_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Dexterity":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Dex_Buff_Value = __0;
                                    //Content.Character.Buffs.dex_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Dex_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Vitality":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Vit_Buff_Value = __0;
                                    //Content.Character.Buffs.vit_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Vit_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_Attunement":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .Att_Buff_Value = __0;
                                    //Content.Character.Buffs.att_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Att_Buff_Value / 255) * 100) + " %";
                                    break;
                                }
                                case "Slider_Character_Buffs_AreaOfEffect":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .PermanentBuffs
                                        .AoE_Buff_Value = __0;
                                    break;
                                }
                            }
                        }
                        else if (__instance.name.Contains("Slider_Items_"))
                        {
                            switch (__instance.name)
                            {
                                case "Slider_Items_Drop_Implicits_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.Implicits_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.Implicits_Max)
                                    {
                                        Content.Items.Drop.implicits_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_Implicits_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.Implicits_Max = __0;
                                    if (__0 < Save_Manager.instance.data.Items.Drop.Implicits_Min)
                                    {
                                        Content.Items.Drop.implicits_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_ForginPotencial_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.ForginPotencial_Min = __0;
                                    if (
                                        __0
                                        > Save_Manager.instance.data.Items.Drop.ForginPotencial_Max
                                    )
                                    {
                                        Content.Items.Drop.forgin_potencial_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_ForginPotencial_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.ForginPotencial_Max = __0;
                                    if (
                                        __0
                                        < Save_Manager.instance.data.Items.Drop.ForginPotencial_Min
                                    )
                                    {
                                        Content.Items.Drop.forgin_potencial_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_SealTier_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.SealTier_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.SealTier_Max)
                                    {
                                        Content.Items.Drop.seal_tier_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_SealTier_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.SealTier_Max = __0;
                                    if (__0 < Save_Manager.instance.data.Items.Drop.SealTier_Min)
                                    {
                                        Content.Items.Drop.seal_tier_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_SealValue_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.SealValue_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.SealValue_Max)
                                    {
                                        Content.Items.Drop.seal_value_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_SealValue_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.SealValue_Max = __0;
                                    if (__0 < Save_Manager.instance.data.Items.Drop.SealValue_Min)
                                    {
                                        Content.Items.Drop.seal_value_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_NbAffixes_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.AffixCount_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.AffixCount_Max)
                                    {
                                        Content.Items.Drop.affix_count_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_NbAffixes_Max":
                                {
                                    if (Save_Manager.instance.data.Items.Drop.AffixCount_Max != __0)
                                    {
                                        Save_Manager.instance.data.Items.Drop.AffixCount_Max = __0;
                                    }
                                    if (__0 < Save_Manager.instance.data.Items.Drop.AffixCount_Min)
                                    {
                                        Content.Items.Drop.affix_count_slider_min.value = __0;
                                    }

                                    break;
                                }
                                case "Slider_Items_Drop_AffixesTiers_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.AffixTiers_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.AffixTiers_Max)
                                    {
                                        Content.Items.Drop.affix_tiers_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_AffixesTiers_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.AffixTiers_Max = __0;
                                    if (__0 < Save_Manager.instance.data.Items.Drop.AffixTiers_Min)
                                    {
                                        Content.Items.Drop.affix_tiers_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_AffixesValues_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.AffixValues_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.AffixValues_Max)
                                    {
                                        Content.Items.Drop.affix_values_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_AffixesValues_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.AffixValues_Max = __0;
                                    if (
                                        Save_Manager.instance.data.Items.Drop.AffixValues_Max
                                        < Save_Manager.instance.data.Items.Drop.AffixValues_Min
                                    )
                                    {
                                        Content.Items.Drop.affix_values_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_UniqueMods_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.UniqueMods_Min = __0;
                                    if (
                                        Save_Manager.instance.data.Items.Drop.UniqueMods_Min
                                        > Save_Manager.instance.data.Items.Drop.UniqueMods_Max
                                    )
                                    {
                                        Content.Items.Drop.unique_mods_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_UniqueMods_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.UniqueMods_Max = __0;
                                    if (
                                        Save_Manager.instance.data.Items.Drop.UniqueMods_Max
                                        < Save_Manager.instance.data.Items.Drop.UniqueMods_Min
                                    )
                                    {
                                        Content.Items.Drop.unique_mods_slider_min.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_LegendaryPotencial_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min =
                                        __0;
                                    if (
                                        Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min
                                        > Save_Manager
                                            .instance
                                            .data
                                            .Items
                                            .Drop
                                            .LegendaryPotencial_Max
                                    )
                                    {
                                        Content.Items.Drop.legendary_potencial_slider_max.value =
                                            __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_LegendaryPotencial_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max =
                                        __0;
                                    if (
                                        __0
                                        < Save_Manager
                                            .instance
                                            .data
                                            .Items
                                            .Drop
                                            .LegendaryPotencial_Min
                                    )
                                    {
                                        Content.Items.Drop.legendary_potencial_slider_min.value =
                                            __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_WeaverWill_Min":
                                {
                                    Save_Manager.instance.data.Items.Drop.WeaverWill_Min = __0;
                                    if (__0 > Save_Manager.instance.data.Items.Drop.WeaverWill_Max)
                                    {
                                        Content.Items.Drop.weaver_will_slider_max.value = __0;
                                    }
                                    break;
                                }
                                case "Slider_Items_Drop_WeaverWill_Max":
                                {
                                    Save_Manager.instance.data.Items.Drop.WeaverWill_Max = __0;
                                    if (__0 < Save_Manager.instance.data.Items.Drop.WeaverWill_Min)
                                    {
                                        Content.Items.Drop.weaver_will_slider_min.value = __0;
                                    }
                                    break;
                                }
                                //Craft
                                case "Slider_Items_Craft_ForginPotencial":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.ForginPotencial =
                                        __0;
                                    break;
                                }
                                case "Slider_Items_Craft_Implicit0":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Implicit_0 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_Implicit1":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Implicit_1 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_Implicit2":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Implicit_2 = __0;
                                    break;
                                }

                                case "Slider_Items_Craft_SealTier":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Seal_Tier =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_SealValue":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Seal_Value = __0;
                                    break;
                                }

                                case "Slider_Items_Craft_AffixTier0":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Tier =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixTier1":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Tier =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixTier2":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Tier =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixTier3":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Tier =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixValue0":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Value =
                                        __0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixValue1":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Value =
                                        __0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixValue2":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Value =
                                        __0;
                                    break;
                                }
                                case "Slider_Items_Craft_AffixValue3":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Value =
                                        __0;
                                    break;
                                }

                                case "Slider_Items_Craft_UniqueMod0":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_0 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod1":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_1 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod2":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_2 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod3":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_3 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod4":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_4 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod5":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_5 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod6":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_6 = __0;
                                    break;
                                }
                                case "Slider_Items_Craft_UniqueMod7":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_7 = __0;
                                    break;
                                }

                                case "Slider_Items_Craft_LegendaryPotencial":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .CraftingSlot
                                        .LegendaryPotencial = (int)__0;
                                    break;
                                }
                                case "Slider_Items_Craft_WeaverWill":
                                {
                                    Save_Manager.instance.data.Items.CraftingSlot.WeaverWill =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Pickup_AutoStore_Timer":
                                {
                                    Save_Manager.instance.data.Items.Pickup.AutoStore_Timer =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Items_Pickup_AutoShatter_Chance":
                                {
                                    Save_Manager.instance.data.Items.Pickup.AutoShatter_Chance =
                                        (int)__0;
                                    if (
                                        !Content.Items.Pickup.autoshatter_chance_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content.Items.Pickup.autoshatter_chance_text.text =
                                            (int)__0 + " %";
                                    }
                                    break;
                                }
                                case "Slider_Items_Pickup_AutoShatter_AffixChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .AutoShatter_AffixChance = (int)__0;
                                    if (
                                        !Content.Items.Pickup.autoshatter_affix_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content.Items.Pickup.autoshatter_affix_text.text =
                                            (int)__0 + " %";
                                    }
                                    break;
                                }
                                case "Slider_Items_Pickup_AutoShatter_QuantityChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Items
                                        .Pickup
                                        .AutoShatter_QuantityChance = (int)__0;
                                    if (
                                        !Content.Items.Pickup.autoshatter_quantity_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content.Items.Pickup.autoshatter_quantity_text.text =
                                            (int)__0 + " %";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (__instance.name.Contains("Slider_Scenes_"))
                        {
                            switch (__instance.name)
                            {
                                case "Slider_Scenes_Camera_ZoomMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.ZoomMinimum = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_ZoomPerScroll":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.ZoomPerScroll = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_ZoomSpeed":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.ZoomSpeed = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_DefaultRotation":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.DefaultRotation = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_OffsetMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.OffsetMinimum = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_OffsetMaximum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.OffsetMaximum = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_AngleMinimum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.AngleMinimum = __0;
                                    break;
                                }
                                case "Slider_Scenes_Camera_AngleMaximum":
                                {
                                    Save_Manager.instance.data.Scenes.Camera.AngleMaximum = __0;
                                    break;
                                }

                                case "Slider_Scenes_Monoliths_MaxStability":
                                {
                                    Save_Manager.instance.data.Scenes.Monoliths.MaxStability = __0;
                                    break;
                                }
                                case "Slider_Scenes_Monoliths_MobsDensity":
                                {
                                    Save_Manager.instance.data.Scenes.Monoliths.MobsDensity = __0;
                                    break;
                                }
                                case "Slider_Scenes_Monoliths_MobsDefeatOnStart":
                                {
                                    Save_Manager.instance.data.Scenes.Monoliths.MobsDefeatOnStart =
                                        __0;
                                    break;
                                }
                                case "Slider_Scenes_Monoliths_BlessingSlots":
                                {
                                    Save_Manager.instance.data.Scenes.Monoliths.BlessingSlots =
                                        (int)__0;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            switch (__instance.name)
                            {
                                case "Slider_SpecializationSlots":
                                {
                                    Save_Manager.instance.data.Skills.SpecializationSlots = __0;
                                    break;
                                }
                                case "Slider_SkillLevel":
                                {
                                    Save_Manager.instance.data.Skills.SkillLevel = __0;
                                    Mods.Skills.Skills_Level.ApplyAll();
                                    break;
                                }
                                case "Slider_SkillLevelMultiplier":
                                {
                                    int multiplier = SettingRow.Clamp(__0);
                                    Save_Manager.instance.data.Skills.SkillLevelMultiplier =
                                        multiplier;
                                    if (
                                        !Content.Skills.SkillTree.skill_level_multiplier_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content.Skills.SkillTree.skill_level_multiplier_text.text =
                                            multiplier + "x";
                                    }
                                    Mods.Skills.Skills_Level.ApplyAll();
                                    break;
                                }
                                case "Slider_PassivePoints":
                                {
                                    Save_Manager.instance.data.Skills.PassivePoints = __0;
                                    Mods.Skills.Passives_Points.Apply();
                                    break;
                                }
                                case "Slider_PassivePointMultiplier":
                                {
                                    int multiplier = SettingRow.Clamp(__0);
                                    Save_Manager.instance.data.Skills.PassivePointMultiplier =
                                        multiplier;
                                    if (
                                        !Content.Skills.SkillTree.passive_point_multiplier_text.IsNullOrDestroyed()
                                    )
                                    {
                                        Content
                                            .Skills
                                            .SkillTree
                                            .passive_point_multiplier_text
                                            .text = multiplier + "x";
                                    }
                                    Mods.Skills.Passives_Points.Apply();
                                    break;
                                }

                                case "Slider_MaximumCompanions":
                                {
                                    Save_Manager.instance.data.Skills.Companion.Limit = (int)__0;
                                    break;
                                }
                                case "Slider_Wolf_SummonLimit":
                                {
                                    Save_Manager.instance.data.Skills.Companion.Wolf.SummonLimit =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_Scorpions_SummonLimit":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Companion
                                        .Scorpion
                                        .BabyQuantity = (int)__0;
                                    break;
                                }

                                case "Slider_Skeleteon_SummonQuantityFromPassive":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .additionalSkeletonsFromPassives = (int)__0;
                                    break;
                                }
                                case "Slider_Skeleteon_SummonQuantityFromSkillTree":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .additionalSkeletonsFromSkillTree = (int)__0;
                                    break;
                                }
                                case "Slider_Skeleteon_SummonQuantityPerCast":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .additionalSkeletonsPerCast = (int)__0;
                                    break;
                                }
                                case "Slider_Skeleteon_ChanceToResummonOnDeath":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Skeletons
                                        .chanceToResummonOnDeath = (int)__0;
                                    break;
                                }

                                case "Slider_Wraiths_SummonMax":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .additionalMaxWraiths = (int)__0;
                                    break;
                                }
                                case "Slider_Wraiths_Delayed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .delayedWraiths = (int)__0;
                                    break;
                                }
                                case "Slider_Wraiths_CastSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Wraiths
                                        .increasedCastSpeed = (int)__0;
                                    break;
                                }

                                case "Slider_Mages_SummonQuantityFromPassive":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .additionalSkeletonsFromPassives = (int)__0;
                                    break;
                                }
                                case "Slider_Mages_SummonQuantityFromSkillTree":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .additionalSkeletonsFromSkillTree = (int)__0;
                                    break;
                                }
                                case "Slider_Mages_SummonQuantityFromItems":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .additionalSkeletonsFromItems = (int)__0;
                                    break;
                                }
                                case "Slider_Mages_SummonPerCast":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .additionalSkeletonsPerCast = (int)__0;
                                    break;
                                }
                                case "Slider_Mages_ChanceForExtraPorjectiles":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .Mages
                                        .chanceForTwoExtraProjectiles = (int)__0;
                                    break;
                                }

                                case "Slider_BoneGolem_GolemPerSkeletons":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .addedGolemsPer4Skeletons = (int)__0;
                                    break;
                                }
                                case "Slider_BoneGolem_SelfResurectChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .selfResurrectChance = (int)__0;
                                    break;
                                }
                                case "Slider_BoneGolem_IncreaseFireAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .increasedFireAuraArea = (int)__0;
                                    break;
                                }
                                case "Slider_BoneGolem_IncreaseArmorAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .undeadArmorAura = (int)__0;
                                    break;
                                }
                                case "Slider_BoneGolem_IncreaseMoveSpeedAura":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .undeadMovespeedAura = (int)__0;
                                    break;
                                }
                                case "Slider_BoneGolem_IncreaseMoveSpeed":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .BoneGolems
                                        .increasedMoveSpeed = (int)__0;
                                    break;
                                }

                                case "Slider_VolatileZombies_ChanceOnMinionDeath":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .chanceToCastFromMinionDeath = (int)__0;
                                    break;
                                }
                                case "Slider_VolatileZombies_InfernalShadeChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .chanceToCastInfernalShadeOnDeath = (int)__0;
                                    break;
                                }
                                case "Slider_VolatileZombies_MarrowShardsChance":
                                {
                                    Save_Manager
                                        .instance
                                        .data
                                        .Skills
                                        .Minions
                                        .VolatileZombies
                                        .chanceToCastMarrowShardsOnDeath = (int)__0;
                                    break;
                                }

                                case "Slider_DreadShades_Duration":
                                {
                                    Save_Manager.instance.data.Skills.Minions.DreadShades.Duration =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_DreadShades_Max":
                                {
                                    Save_Manager.instance.data.Skills.Minions.DreadShades.max =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_DreadShades_Decay":
                                {
                                    Save_Manager.instance.data.Skills.Minions.DreadShades.decay =
                                        (int)__0;
                                    break;
                                }
                                case "Slider_DreadShades_Radius":
                                {
                                    Save_Manager.instance.data.Skills.Minions.DreadShades.radius =
                                        (int)__0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
