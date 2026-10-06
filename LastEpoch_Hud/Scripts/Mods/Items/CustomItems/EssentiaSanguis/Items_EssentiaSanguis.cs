using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Mods.Items.CustomItems;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.EssentiaSanguis;

[RegisterTypeInIl2Cpp]
public class Items_EssentiaSanguis : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    public Items_EssentiaSanguis(System.IntPtr ptr)
        : base(ptr) { }

    private void Update()
    {
        _registrar.Update();
    }

    private static CustomUniqueDefinition CreateDefinition()
    {
        return new CustomUniqueDefinition
        {
            Spec = CustomUniqueSpecs.EssentiaSanguis,
            SubtypeNameKey = CustomItemLocaleKeys.EssentiaSanguisSubtype,
            UniqueNameKey = CustomItemLocaleKeys.EssentiaSanguisName,
            LoreKey = CustomItemLocaleKeys.EssentiaSanguisLore,
            Description = texts =>
                LocaleText.Get(texts, CustomItemLocaleKeys.EssentiaSanguisDescription),
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
}
