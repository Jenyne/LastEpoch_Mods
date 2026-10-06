//______________________________________________________________________//
//https://discord.com/channels/1366160878579351756/1372660677491036272
//https://github.com/zakt4n

using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Mjolner;

[RegisterTypeInIl2Cpp]
public class Items_Mjolner : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    private bool _inGame;

    public Items_Mjolner(System.IntPtr ptr)
        : base(ptr) { }

    internal static Ability[] GetTriggerAbilities()
    {
        return MjolnerTrigger.Abilities;
    }

    private void Awake()
    {
        SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
    }

    private void Update()
    {
        _registrar.Update();
        MjolnerHitEvents.Update();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Scenes.IsGameScene())
        {
            _inGame = false;
            return;
        }

        MjolnerTrigger.InitializeSocketedSkills();
        if (!_inGame)
        {
            MjolnerHitEvents.Reset();
        }

        _inGame = true;
    }

    private static CustomUniqueDefinition CreateDefinition()
    {
        return new CustomUniqueDefinition
        {
            Spec = CustomUniqueSpecs.Mjolner,
            UniqueName = MjolnerTexts.UniqueName,
            Lore = MjolnerTexts.Lore,
            Description = MjolnerLocales.CurrentDescription,
            Flags = () =>
                new CustomUniqueFlags(
                    Save_Manager.instance.data.Items.Mjolner.WeaverWill,
                    Save_Manager.instance.data.Items.Mjolner.UniqueDrop,
                    true
                ),
            Mods = Mods,
            TooltipEntries = TooltipEntries,
        };
    }

    private static Il2CppSystem.Collections.Generic.List<UniqueItemMod> Mods()
    {
        var mods = new Il2CppSystem.Collections.Generic.List<UniqueItemMod>();
        mods.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Damage,
                tags = AT.Lightning,
                type = BaseStats.ModType.INCREASED,
                maxValue = 1.0f,
                value = 0.8f,
            }
        );
        mods.Add(
            new UniqueItemMod
            {
                canRoll = true,
                property = SP.Damage,
                tags = AT.Physical,
                type = BaseStats.ModType.INCREASED,
                maxValue = 1.2f,
                value = 0.8f,
            }
        );

        return mods;
    }

    private static Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry> TooltipEntries()
    {
        var entries = new Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry>();
        entries.Add(new UniqueModDisplayListEntry(0));
        entries.Add(new UniqueModDisplayListEntry(1));
        if (Save_Manager.instance.data.Items.Mjolner.ProcAnyLightningSpell)
        {
            entries.Add(new UniqueModDisplayListEntry(2));
        }
        entries.Add(new UniqueModDisplayListEntry(128));

        return entries;
    }
}
