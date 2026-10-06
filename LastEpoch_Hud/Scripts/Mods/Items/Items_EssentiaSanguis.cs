using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Services.Models.Items;
using Il2CppLE.Services.Visuals;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items;

[RegisterTypeInIl2Cpp]
public class Items_EssentiaSanguis : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    public static Items_EssentiaSanguis instance { get; private set; }

    public Items_EssentiaSanguis(System.IntPtr ptr)
        : base(ptr) { }

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!Assets.Loaded)
        {
            Assets.Load();
        }
        _registrar.Update();
    }

    private static CustomUniqueDefinition CreateDefinition()
    {
        return new CustomUniqueDefinition
        {
            Spec = CustomUniqueSpecs.EssentiaSanguis,
            SubtypeName = EssentiaSanguisTexts.SubtypeName,
            UniqueName = EssentiaSanguisTexts.UniqueName,
            Lore = EssentiaSanguisTexts.Lore,
            Description = EssentiaSanguisTexts.Description.For,
            Flags = () => CustomUniqueFlags.NoSettings,
            Implicits = Implicits,
            Mods = Mods,
            TooltipEntries = TooltipEntries,
        };
    }

    private static Il2CppSystem.Collections.Generic.List<ItemList.EquipmentImplicit> Implicits()
    {
        var implicits = new Il2CppSystem.Collections.Generic.List<ItemList.EquipmentImplicit>();
        implicits.Add(
            new ItemList.EquipmentImplicit
            {
                implicitMaxValue = 50,
                implicitValue = 50,
                property = SP.DodgeRating,
                specialTag = 0,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
            }
        );

        return implicits;
    }

    private static Il2CppSystem.Collections.Generic.List<UniqueItemMod> Mods()
    {
        var result = new Il2CppSystem.Collections.Generic.List<UniqueItemMod>();
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.DodgeRating,
                tags = AT.None,
                type = BaseStats.ModType.INCREASED,
                maxValue = 0.7f,
                value = 0.5f,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.HealthLeech,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 1f,
                value = 0.5f,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Damage,
                tags = AT.Lightning,
                type = BaseStats.ModType.ADDED,
                maxValue = 50,
                value = 30,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Intelligence,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 25,
                value = 15,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.LightningResistance,
                tags = AT.None,
                type = BaseStats.ModType.INCREASED,
                maxValue = 0.35f,
                value = 0.25f,
            }
        );

        return result;
    }

    private static Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry> TooltipEntries()
    {
        var result = new Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry>();
        result.Add(new UniqueModDisplayListEntry(0));
        result.Add(new UniqueModDisplayListEntry(1));
        result.Add(new UniqueModDisplayListEntry(2));
        result.Add(new UniqueModDisplayListEntry(3));
        result.Add(new UniqueModDisplayListEntry(4));
        result.Add(new UniqueModDisplayListEntry(128));

        return result;
    }

    public class Assets
    {
        public static bool Loaded = false;
        public static bool loading = false;
        public static bool attempted = false;

        public static void Load()
        {
            if (
                (!Loaded)
                && (!attempted)
                && (!Hud_Manager.asset_bundle.IsNullOrDestroyed())
                && (!loading)
            )
            {
                loading = true;
                attempted = true;
                try
                {
                    foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                    {
                        if (name.Contains("/essentiasanguis/"))
                        {
                            if (
                                (Functions.Check_Texture(name))
                                && (name.Contains("icon"))
                                && (Unique.Icon.IsNullOrDestroyed())
                            )
                            {
                                Texture2D texture = Hud_Manager
                                    .asset_bundle.LoadAsset(name)
                                    .TryCast<Texture2D>();
                                Unique.Icon = Sprite.Create(
                                    texture,
                                    new Rect(0, 0, texture.width, texture.height),
                                    Vector2.zero
                                );
                            }
                        }
                    }
                    if (!Unique.Icon.IsNullOrDestroyed())
                    {
                        Loaded = true;
                    }
                    else
                    {
                        Loaded = false;
                    }
                }
                catch
                {
                    Main.logger_instance?.Error("Essentia Sanguis Asset Error");
                }
                loading = false;
            }
        }
    }

    public class Unique
    {
        public static Sprite Icon = null;

        public static string Get_Unique_Name()
        {
            return CustomItemLocalization.Text(EssentiaSanguisTexts.UniqueName);
        }

        [HarmonyPatch(typeof(InventoryItemUI), "SetImageSpritesAndColours")]
        public class InventoryItemUI_SetImageSpritesAndColours
        {
            [HarmonyPostfix]
            static void Postfix(ref Il2Cpp.InventoryItemUI __instance)
            {
                if (
                    (__instance.EntryRef.data.getAsUnpacked().FullName == Get_Unique_Name())
                    && (!Icon.IsNullOrDestroyed())
                )
                {
                    __instance.contentImage.sprite = Icon;
                }
            }
        }

        public class UITooltipItem_GetItemSprite
        {
            [HarmonyPostfix]
            static void Postfix(ref UnityEngine.Sprite __result, ItemData __0)
            {
                if (
                    (__0.getAsUnpacked().FullName == Get_Unique_Name())
                    && (!Icon.IsNullOrDestroyed())
                )
                {
                    __result = Icon;
                }
            }
        }
    }

    public class Visual
    {
        [HarmonyPatch(typeof(ClientVisualsService), "GetItemVisual")]
        public class ClientVisualsService_GetItemVisual
        {
            [HarmonyPrefix]
            static void Prefix(ClientVisualsService __instance, ref ItemVisualKey __0)
            {
                if (
                    (__0.EquipmentType == EquipmentType.GLOVES)
                    && (__0.SubType == CustomUniqueSpecs.EssentiaSanguis.BaseId)
                    && (__0.UniqueID == CustomUniqueSpecs.EssentiaSanguis.UniqueId)
                )
                {
                    __0.SubType = 0;
                    __0.UniqueID = 22; //Keeper's Gloves
                }
            }
        }
    }

    public class Hooks
    {
        [HarmonyPatch(typeof(PlayerLeechTracker), "AddLifeLeech")]
        public class PlayerLeechTracker_AddLifeLeech
        {
            [HarmonyPrefix]
            static bool Prefix(PlayerLeechTracker __instance, float __0)
            {
                bool r = true;
                if (
                    (!Refs_Manager.player_actor.IsNullOrDestroyed())
                    && (!Refs_Manager.player_protection_class.IsNullOrDestroyed())
                )
                {
                    if (
                        Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(
                            CustomUniqueSpecs.EssentiaSanguis.UniqueId
                        )
                    )
                    {
                        float ward = Refs_Manager.player_protection_class.CurrentWard;
                        Refs_Manager.player_protection_class.CurrentWard += __0;
                        r = false;
                    }
                }
                return r;
            }
        }
    }
}
