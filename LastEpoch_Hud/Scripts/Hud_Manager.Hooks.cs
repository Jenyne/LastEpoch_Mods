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
                    }
                }
            }
        }
    }
}
