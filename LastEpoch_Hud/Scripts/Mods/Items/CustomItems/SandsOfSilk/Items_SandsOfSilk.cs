using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Mods.Items.CustomItems;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.SandsOfSilk;

[RegisterTypeInIl2Cpp]
public class Items_SandsOfSilk : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    public Items_SandsOfSilk(System.IntPtr ptr)
        : base(ptr) { }

    private void Update()
    {
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
}
