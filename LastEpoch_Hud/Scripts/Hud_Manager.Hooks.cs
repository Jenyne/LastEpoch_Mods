using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Hooks
    {
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
                        else
                        {
                            switch (__instance.name)
                            {
                                case "Toggle_UnlockAllSkills":
                                {
                                    Save_Manager.instance.data.Skills.Enable_AllSkills =
                                        __instance.isOn;
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
                                    break;
                                }
                                case "Slider_Character_Cheats_DensityMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.DensityMultiplier =
                                        __0;
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
                                    break;
                                }
                                case "Slider_Character_Cheats_AbilityMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.AbilityMultiplier =
                                        __0;
                                    break;
                                }
                                case "Slider_Character_Cheats_FavorMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.FavorMultiplier =
                                        __0;
                                    break;
                                }
                                case "Slider_Character_Cheats_MemoryAmberMultiplier":
                                {
                                    uint multiplier = (uint)
                                        Mathf.Clamp(Mathf.RoundToInt(__0), 1, 255);
                                    Save_Manager
                                        .instance
                                        .data
                                        .Character
                                        .Cheats
                                        .MemoryAmberMultiplier = multiplier;
                                    break;
                                }
                                case "Slider_Character_Cheats_ItemDropMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier =
                                        __0;
                                    break;
                                }
                                case "Slider_Character_Cheats_ItemDropChance":
                                {
                                    Save_Manager.instance.data.Character.Cheats.ItemDropChance =
                                        __0;
                                    break;
                                }
                                case "Slider_Character_Cheats_GoldDropMultiplier":
                                {
                                    Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier =
                                        __0;
                                    break;
                                }
                                case "Slider_Character_Cheats_GoldDropChance":
                                {
                                    Save_Manager.instance.data.Character.Cheats.GoldDropChance =
                                        __0;
                                    break;
                                }
                                //Data
                                case "Slider_Character_Data_Deaths":
                                {
                                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                    {
                                        Refs_Manager.player_data.Deaths = (int)__0;
                                    }
                                    break;
                                }
                                case "Slider_Character_Data_LanternLuminance":
                                {
                                    if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                    {
                                        Refs_Manager.player_data.LanternLuminance = (int)__0;
                                    }
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
