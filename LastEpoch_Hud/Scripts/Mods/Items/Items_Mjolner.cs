//______________________________________________________________________//
//https://discord.com/channels/1366160878579351756/1372660677491036272
//https://github.com/zakt4n

using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Items;

[RegisterTypeInIl2Cpp]
public class Items_Mjolner : MonoBehaviour
{
    private static readonly CustomUniqueRegistrar _registrar = new(CreateDefinition());

    public static Items_Mjolner instance { get; private set; }

    public Items_Mjolner(System.IntPtr ptr)
        : base(ptr) { }

    private bool InGame = false;

    void Awake()
    {
        instance = this;
        SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
    }

    void Update()
    {
        _registrar.Update();
        Events.Update();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Scenes.IsGameScene())
        {
            Trigger.Initialize_SocketedSkills();
            if (!InGame)
            {
                Events.Reset();
            }
            InGame = true;
        }
        else if (InGame)
        {
            InGame = false;
        }
    }

    private static CustomUniqueDefinition CreateDefinition()
    {
        return new CustomUniqueDefinition
        {
            Spec = CustomUniqueSpecs.Mjolner,
            UniqueName = MjolnerTexts.UniqueName,
            Lore = MjolnerTexts.Lore,
            Description = _ => Locales.DescriptionWhenSaveReady(),
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

    private class Locales
    {
        internal static string Get_UniqueDescription()
        {
            string description = "";
            int str_requirement = Save_Manager.instance.data.Items.Mjolner.StrRequirement;
            int int_requirement = Save_Manager.instance.data.Items.Mjolner.IntRequirement;
            if (Save_Manager.instance.data.Items.Mjolner.ProcAnyLightningSpell)
            {
                int display_min_chance = (int)(
                    (Save_Manager.instance.data.Items.Mjolner.MinTriggerChance / 255f) * 100f
                );
                int display_max_chance = (int)(
                    (Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance / 255f) * 100f
                );
                switch (LastEpoch_Hud.Locales.current)
                {
                    case LastEpoch_Hud.Locales.Selected.English:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.French:
                    {
                        description =
                            "Si vous avez au moins "
                            + str_requirement
                            + " de Force et "
                            + int_requirement
                            + " d'Intelligence, "
                            + display_min_chance
                            + " à "
                            + display_max_chance
                            + "% de chance de déclencher un sort de foudre lors d'une attaque réussie";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Korean:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.German:
                    {
                        description =
                            "Wenn Sie mindestens "
                            + str_requirement
                            + " Stärke und "
                            + int_requirement
                            + " Intelligenz haben, "
                            + display_min_chance
                            + " bis "
                            + display_max_chance
                            + "% Chance, bei Treffer mit einem Angriff einen Blitzzauber auszulösen";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Russian:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Polish:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Portuguese:
                    {
                        description =
                            "Se você tiver pelo menos "
                            + str_requirement
                            + " de Força e "
                            + int_requirement
                            + " de Inteligência, ganhe "
                            + display_min_chance
                            + " a "
                            + display_max_chance
                            + "% de chance para Ativar uma Magia de Raio ao Acertar um Ataque";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Chinese:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Spanish:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, "
                            + display_min_chance
                            + " to "
                            + display_max_chance
                            + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                        break;
                    }
                }
            }
            else
            {
                double cooldown = (
                    Save_Manager.instance.data.Items.Mjolner.SocketedCooldown / 1000
                );
                string skill_0 = Save_Manager.instance.data.Items.Mjolner.SockectedSkill_0;
                string skill_1 = Save_Manager.instance.data.Items.Mjolner.SockectedSkill_1;
                string skill_2 = Save_Manager.instance.data.Items.Mjolner.SockectedSkill_2;
                switch (LastEpoch_Hud.Locales.current)
                {
                    case LastEpoch_Hud.Locales.Selected.English:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.French:
                    {
                        description =
                            "Si vous avez au moins "
                            + str_requirement
                            + " de Force et "
                            + int_requirement
                            + " d'Intelligence, déclenche "
                            + skill_0
                            + ", "
                            + skill_1
                            + " et "
                            + skill_2
                            + " à l'impact, avec un temps de recharge de "
                            + cooldown
                            + " seconde";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Korean:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.German:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Russian:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Polish:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Portuguese:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Chinese:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                    case LastEpoch_Hud.Locales.Selected.Spanish:
                    {
                        description =
                            "If you have at least "
                            + str_requirement
                            + " Strength and "
                            + int_requirement
                            + " Intelligence, Trigger "
                            + skill_0
                            + ", "
                            + skill_1
                            + " and "
                            + skill_2
                            + " on Hit, with a "
                            + cooldown
                            + " second Cooldown";
                        break;
                    }
                }
            }

            return description;
        }

        internal static string DescriptionWhenSaveReady()
        {
            if (Save_Manager.instance.IsNullOrDestroyed() || !Save_Manager.instance.initialized)
            {
                return null;
            }

            return Get_UniqueDescription();
        }
    }

    internal static Ability[] GetTriggerAbilities()
    {
        return Trigger.Abilities;
    }

    private class Trigger
    {
        internal static void AllSkills(Actor hitActor)
        {
            if ((!hitActor.IsNullOrDestroyed()) && (!trigger))
            {
                trigger = true;
                float item_roll = Random.Range(
                    Save_Manager.instance.data.Items.Mjolner.MinTriggerChance,
                    Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance
                );
                float item_roll_percent = (item_roll / 255) * 100;
                float roll_percent = Random.Range(0f, 100f);
                if (
                    (roll_percent <= item_roll_percent)
                    && (!Refs_Manager.player_treedata.IsNullOrDestroyed())
                )
                {
                    foreach (
                        Ability ability in Refs_Manager.player_actor.GetAbilityList().abilities
                    )
                    {
                        if (ability.tags.HasFlag(AT.Lightning) && ability.tags.HasFlag(AT.Spell))
                        {
                            float backup_manacost = ability.manaCost;
                            ability.manaCost = 0; //Remove ManaCost
                            //We need AbilityMutator here for addedManaCost variable
                            ability.castAtTargetFromConstructorAfterDelay(
                                Refs_Manager.player_actor.abilityObjectConstructor,
                                Vector3.zero,
                                hitActor.position(),
                                0,
                                UseType.Indirect
                            );
                            ability.manaCost = backup_manacost; //Reset ManaCost
                        }
                    }
                }
                trigger = false;
            }
        }

        internal static void Initialize_SocketedSkills()
        {
            if (!Initializing)
            {
                Initializing = true;
                Abilities = new Ability[3];
                Times = new System.DateTime[3];
                if (!Refs_Manager.ability_manager.IsNullOrDestroyed())
                {
                    int i = 0;
                    foreach (Ability ability in Refs_Manager.ability_manager.abilities)
                    {
                        if ((!ability.IsNullOrDestroyed()) && (i < 3))
                        {
                            if (
                                (
                                    ability.abilityName
                                    == Save_Manager.instance.data.Items.Mjolner.SockectedSkill_0
                                )
                                || (
                                    ability.abilityName
                                    == Save_Manager.instance.data.Items.Mjolner.SockectedSkill_1
                                )
                                || (
                                    ability.abilityName
                                    == Save_Manager.instance.data.Items.Mjolner.SockectedSkill_2
                                )
                            )
                            {
                                Abilities[i] = ability;
                                Times[i] = System.DateTime.Now;
                                i++;
                            }
                        }
                    }
                }
                Initializing = false;
            }
        }

        internal static void SocketedSkills(Actor hitActor)
        {
            if ((!hitActor.IsNullOrDestroyed()) && (!trigger))
            {
                trigger = true;
                for (int i = 0; i < Abilities.Length; i++)
                {
                    if ((!Abilities[i].IsNullOrDestroyed()) && ((i < Times.Length)))
                    {
                        bool run = false;
                        System.Double cd = Save_Manager
                            .instance
                            .data
                            .Items
                            .Mjolner
                            .SocketedCooldown;
                        if (cd < 250)
                        {
                            cd = 250;
                        }

                        if ((System.DateTime.Now - Times[i]).TotalMilliseconds > cd)
                        {
                            run = true;
                        }

                        if (run)
                        {
                            float backup_manacost = Abilities[i].manaCost;
                            Abilities[i].manaCost = 0; //Remove ManaCost
                            Abilities[i]
                                .castAtTargetFromConstructorAfterDelay(
                                    Refs_Manager.player_actor.abilityObjectConstructor,
                                    Vector3.zero,
                                    hitActor.position(),
                                    0,
                                    UseType.Indirect
                                );
                            Abilities[i].manaCost = backup_manacost; //Reset ManaCost
                            Times[i] = System.DateTime.Now;
                        }
                    }
                }
                trigger = false;
            }
        }

        internal static Ability[] Abilities = null;
        private static System.DateTime[] Times = null;
        private static bool Initializing = false;
        private static bool trigger = false;
    }

    private class Events
    {
        internal static void Update()
        {
            if (!OnHitEvent_Initialized)
            {
                Init_OnHitEvent();
            }
        }

        internal static void Reset()
        {
            OnHitEvent_Initialized = false;
        }

        private static bool OnHitEvent_Initialized = false;

        private static void Init_OnHitEvent()
        {
            if (!Refs_Manager.player_actor.IsNullOrDestroyed())
            {
                if (!Refs_Manager.player_actor.gameObject.IsNullOrDestroyed())
                {
                    AbilityEventListener listener =
                        Refs_Manager.player_actor.gameObject.GetComponent<AbilityEventListener>();
                    if (!listener.IsNullOrDestroyed())
                    {
                        listener.add_onHitEvent(OnHitAction);
                        OnHitEvent_Initialized = true;
                    }
                }
            }
        }

        private static readonly System.Action<Ability, Actor> OnHitAction = new System.Action<
            Ability,
            Actor
        >(OnHit);

        private static void OnHit(Ability ability, Actor hitActor)
        {
            if (!Refs_Manager.player_actor.IsNullOrDestroyed())
            {
                if (
                    Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(
                        CustomUniqueSpecs.Mjolner.UniqueId
                    )
                    && (
                        Refs_Manager.player_actor.stats.GetAttributeValue(
                            CoreAttribute.Attribute.Strength
                        ) >= Save_Manager.instance.data.Items.Mjolner.StrRequirement
                    )
                    && (
                        Refs_Manager.player_actor.stats.GetAttributeValue(
                            CoreAttribute.Attribute.Intelligence
                        ) >= Save_Manager.instance.data.Items.Mjolner.IntRequirement
                    )
                )
                {
                    if (
                        Save_Manager.instance.data.Items.Mjolner.ProcAnyLightningSpell
                        && (!ability.tags.HasFlag(AT.Spell))
                    )
                    {
                        Trigger.AllSkills(hitActor);
                    }
                    else
                    {
                        Trigger.SocketedSkills(hitActor);
                    }
                }
            }
        }
    }
}
