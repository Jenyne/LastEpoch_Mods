using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.ModUI;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

[RegisterTypeInIl2Cpp]
public class Items_HeadHunter : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());
    private static readonly HeadhunterRunResetWatch _resetWatch = new();

    public Items_HeadHunter(System.IntPtr ptr)
        : base(ptr) { }

    private void Awake()
    {
        HeadhunterConfigLoader.Load();
        SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _resetWatch.MarkSceneLoaded();
    }

    private void Update()
    {
        _registrar.Update();
        ResetRunIfNeeded();
        HeadhunterKillSource.EnsureHooked();
        HeadhunterConfigLoader.ReloadIfChanged(Time.unscaledTime);
        HeadhunterBuffBar.Tick(Time.unscaledTime);
        HeadhunterBarHover.Tick();
        MonsterModDump.Tick(Time.unscaledTime);
    }

    private static void ResetRunIfNeeded()
    {
        if (!_resetWatch.ShouldReset(PlayerId()))
        {
            return;
        }

        HeadhunterBuffClearer.ClearAll(HeadhunterConfigLoader.Resolved);
        if (ModSettings.Debug.Enabled.Value)
        {
            Main.logger_instance?.Msg("Headhunter run state reset");
        }
    }

    private static long PlayerId()
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return 0;
        }

        return Refs_Manager.player_actor.Pointer.ToInt64();
    }

    private static CustomUniqueDefinition CreateDefinition()
    {
        return new CustomUniqueDefinition
        {
            Spec = CustomUniqueSpecs.Headhunter,
            SubtypeNameKey = CustomItemLocaleKeys.HeadhunterSubtype,
            UniqueNameKey = CustomItemLocaleKeys.HeadhunterName,
            LoreKey = CustomItemLocaleKeys.HeadhunterLore,
            Description = HeadhunterLocales.Description,
            Flags = () =>
                new CustomUniqueFlags(
                    Save_Manager.instance.data.Items.Headhunter.WeaverWill,
                    Save_Manager.instance.data.Items.Headhunter.UniqueDrop,
                    Save_Manager.instance.data.Items.Headhunter.BaseDrop
                ),
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
                implicitMaxValue = 40,
                implicitValue = 25,
                property = SP.Health,
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
                property = SP.Strength,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 55,
                value = 40,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Dexterity,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 55,
                value = 40,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Health,
                tags = AT.None,
                type = BaseStats.ModType.ADDED,
                maxValue = 60,
                value = 50,
            }
        );
        result.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Damage,
                tags = AT.None,
                type = BaseStats.ModType.INCREASED,
                maxValue = 0.3f,
                value = 0.2f,
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
        result.Add(new UniqueModDisplayListEntry(128));

        return result;
    }
}
