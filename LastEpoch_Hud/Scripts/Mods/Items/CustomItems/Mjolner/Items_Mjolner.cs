using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Affixes;
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
            UniqueNameKey = CustomItemLocaleKeys.MjolnerName,
            LoreKey = CustomItemLocaleKeys.MjolnerLore,
            Description = MjolnerLocales.CurrentDescription,
            Flags = () =>
                new CustomUniqueFlags(
                    Save_Manager.instance.data.Items.Mjolner.WeaverWill,
                    Save_Manager.instance.data.Items.Mjolner.UniqueDrop,
                    true
                ),
            Mods = CustomUniqueAffixes.MjolnerMods,
            TooltipEntries = () => CustomUniqueAffixes.MjolnerTooltip,
        };
    }
}
