using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Services.Models.Items;
using Il2CppLE.Services.Visuals;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items;

[RegisterTypeInIl2Cpp]
public class Items_SandsOfSilk : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    public static Items_SandsOfSilk instance { get; private set; }

    public Items_SandsOfSilk(System.IntPtr ptr)
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
            Spec = CustomUniqueSpecs.SandsOfSilk,
            SubtypeName = SandsOfSilkTexts.SubtypeName,
            UniqueName = SandsOfSilkTexts.UniqueName,
            Lore = SandsOfSilkTexts.Lore,
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
                implicitMaxValue = 204,
                implicitValue = 153,
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
                maxValue = 1f,
                value = 0.5f,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Mana,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 80,
                value = 50,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Dexterity,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 20,
                value = 10,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Intelligence,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 20,
                value = 10,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.FireResistance,
                tags = AT.None,
                type = BaseStats.ModType.INCREASED,
                maxValue = 0.15f,
                value = 0.1f,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.IncreasedCooldownRecoverySpeed,
                tags = AT.None,
                type = BaseStats.ModType.INCREASED,
                maxValue = 0.3f,
                value = 0.15f,
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
        result.Add(new UniqueModDisplayListEntry(5));

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
                        if (name.Contains("/sandsofsilk/"))
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
                    Main.logger_instance?.Error("Sands Of Silk Asset Error");
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
            return CustomItemLocalization.Text(SandsOfSilkTexts.UniqueName);
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
                    (__0.EquipmentType == EquipmentType.BODY_ARMOR)
                    && (__0.SubType == CustomUniqueSpecs.SandsOfSilk.BaseId)
                    && (__0.UniqueID == CustomUniqueSpecs.SandsOfSilk.UniqueId)
                )
                {
                    __0.SubType = 0;
                    __0.UniqueID = 7; //The Krestel
                }
            }
        }
    }
}
