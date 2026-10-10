using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

// Utilities > Character. The legacy Character, Skills, Data, Blessings, and
// Factions controls are represented here without changing their game logic.
internal static class Utilities_Character
{
    private static HudFormPage page;
    private static int factionSelection;
    private static float factionFavor;
    private static float factionRank;
    private static float factionReputation;
    private static Dropdown classSource;

    public static void Build(GameObject parent, GameObject hud, Font font)
    {
        page = HudFormPage.Build(parent, hud, font, "Utilities_Character");
        if (page == null)
            return;

        page.AddButtonGrid(
            "CharacterActions",
            new[] { "Level Up Once", "Level Up To Level 100", "Skip Campaign", "Reset Mastery" },
            new System.Action[]
            {
                Hud_Manager.Content.Character.Cheats.LevelUpOnce_Click,
                Hud_Manager.Content.Character.Cheats.LevelUpMax_Click,
                Hud_Manager.Content.Character.Cheats.CompleteQuest_Click,
                Hud_Manager.Content.Character.Cheats.Masteries_Click,
            }
        );
        BuildCheats();
        BuildData();
        BuildBlessings();
        BuildFactions();
    }

    public static void Show()
    {
        RefreshClassOptions();
        page?.Show();
    }

    public static void Hide() => page?.Hide();

    public static void Refresh()
    {
        RefreshClassOptions();
        page?.RefreshValues();
    }

    private static void BuildCheats()
    {
        var card = page.AddCard("Cheats", "Cheats");
        // Preserve native lens/double-reward behavior; only the transient item count changes.
        page.AddToggleSlider(
            card,
            "ProphecyRewardMultiplier",
            "Prophecy Reward Multiplier",
            "x",
            1f,
            10f,
            true,
            () => ModSettings.ProphecyRewards.Multiplier.Enabled,
            enabled => ModSettings.ProphecyRewards.Multiplier.SetEnabled(enabled),
            () => ModSettings.ProphecyRewards.Multiplier.Value,
            value => ModSettings.ProphecyRewards.Multiplier.SetValue(value)
        );

        page.AddToggle(
            card,
            "IdolRerollFreeAmber",
            "No Memory Amber Cost",
            () => ModSettings.IdolReroll.FreeMemoryAmber.Value,
            value => ModSettings.IdolReroll.FreeMemoryAmber.Set(value)
        );
        page.AddToggle(
            card,
            "IdolRerollUnlimitedUses",
            "Unlimited Idol Altar Uses",
            () => ModSettings.IdolReroll.UnlimitedUses.Value,
            value => ModSettings.IdolReroll.UnlimitedUses.Set(value)
        );

        page.AddToggle(
            card,
            "GodMode",
            "God Mode",
            () => HasSave() && Save_Manager.instance.data.Character.Cheats.Enable_GodMode,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Character.Cheats.Enable_GodMode = value;
            }
        );
        page.AddToggle(
            card,
            "LowLife",
            "Force Low Life",
            () => HasSave() && Save_Manager.instance.data.Character.Cheats.Enable_LowLife,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Character.Cheats.Enable_LowLife = value;
            }
        );
        page.AddToggle(
            card,
            "WeaponRestrictions",
            "Ignore Weapon Restrictions",
            () =>
                HasSave() && Save_Manager.instance.data.Character.Cheats.Enable_TwoHandedWithShield,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Character.Cheats.Enable_TwoHandedWithShield = value;
            }
        );
        page.AddToggle(
            card,
            "ManaCost",
            "Remove Mana Cost",
            () => HasSave() && Save_Manager.instance.data.Skills.Enable_RemoveManaCost,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Enable_RemoveManaCost = value;
            }
        );
        page.AddToggle(
            card,
            "ChannelCost",
            "Remove Mana Cost While Channeling",
            () => HasSave() && Save_Manager.instance.data.Skills.Enable_RemoveChannelCost,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Enable_RemoveChannelCost = value;
            }
        );
        page.AddToggle(
            card,
            "ChannelRegen",
            "Mana Regeneration While Channeling",
            () => HasSave() && Save_Manager.instance.data.Skills.Enable_NoManaRegenWhileChanneling,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Enable_NoManaRegenWhileChanneling = value;
            }
        );
        page.AddToggle(
            card,
            "ContinueOutOfMana",
            "Don't Stop When Out Of Mana",
            () => HasSave() && Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana = value;
            }
        );
        page.AddToggle(
            card,
            "Cooldown",
            "No Cooldown",
            () => HasSave() && Save_Manager.instance.data.Skills.Enable_RemoveCooldown,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Enable_RemoveCooldown = value;
            }
        );
        page.AddToggle(
            card,
            "NodeRequirements",
            "Remove Node Requirements",
            () => HasSave() && Save_Manager.instance.data.Skills.Disable_NodeRequirement,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Skills.Disable_NodeRequirement = value;
            }
        );

        page.AddSlider(
            card,
            "PassivePoints",
            "Passive Points",
            string.Empty,
            0f,
            255f,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Skills.Enable_PassivePoints
                    ? Save_Manager.instance.data.Skills.PassivePoints
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Skills.Enable_PassivePoints = enabled;
                if (enabled)
                    Save_Manager.instance.data.Skills.PassivePoints = value;
                Mods.Skills.Passives_Points.Sync();
            }
        );
        page.AddSlider(
            card,
            "PassiveMultiplier",
            "Passive Point Multiplier Per Level",
            "x",
            0f,
            10f,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Skills.Enable_PassivePointMultiplier
                    ? Save_Manager.instance.data.Skills.PassivePointMultiplier
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Skills.Enable_PassivePointMultiplier = enabled;
                if (enabled)
                    Save_Manager.instance.data.Skills.PassivePointMultiplier = SettingRow.Clamp(
                        value
                    );
                Mods.Skills.Passives_Points.Sync();
            }
        );
        page.AddSlider(
            card,
            "SkillLevel",
            "Skill Level",
            string.Empty,
            0f,
            255f,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Skills.Enable_SkillLevel
                    ? Save_Manager.instance.data.Skills.SkillLevel
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Skills.Enable_SkillLevel = enabled;
                if (enabled)
                    Save_Manager.instance.data.Skills.SkillLevel = value;
                Mods.Skills.Skills_Level.Sync();
            }
        );
        page.AddSlider(
            card,
            "SkillMultiplier",
            "Skill Point Multiplier",
            "x",
            0f,
            10f,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Skills.Enable_SkillLevelMultiplier
                    ? Save_Manager.instance.data.Skills.SkillLevelMultiplier
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Skills.Enable_SkillLevelMultiplier = enabled;
                if (enabled)
                    Save_Manager.instance.data.Skills.SkillLevelMultiplier = SettingRow.Clamp(
                        value
                    );
                Mods.Skills.Skills_Level.Sync();
            }
        );
        page.AddSlider(
            card,
            "WeaverPoints",
            "Weaver Tree Points",
            string.Empty,
            0f,
            Mods.Factions.TheWoven.Faction_Woven_TreePoints.SliderMax,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints
                    ? Save_Manager.instance.data.Factions.TheWoven.TreePoints
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints = enabled;
                if (enabled)
                {
                    Save_Manager.instance.data.Factions.TheWoven.TreePoints = (int)value;
                    Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                }
                else
                {
                    Mods.Factions.TheWoven.Faction_Woven_TreePoints.ReleaseToRealPoints();
                }
            }
        );
        page.AddSlider(
            card,
            "WeaverMultiplier",
            "Weaver Tree Points Multiplier",
            "x",
            0f,
            10f,
            true,
            () =>
                HasSave() && Save_Manager.instance.data.Factions.TheWoven.Enable_PointMultiplier
                    ? Save_Manager.instance.data.Factions.TheWoven.PointMultiplier
                    : 0f,
            value =>
            {
                if (!HasSave())
                    return;
                bool enabled = value > 0.0001f;
                Save_Manager.instance.data.Factions.TheWoven.Enable_PointMultiplier = enabled;
                if (enabled)
                    Save_Manager.instance.data.Factions.TheWoven.PointMultiplier = SettingRow.Clamp(
                        value
                    );
                Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
            }
        );
        page.AddToggle(
            card,
            "FreeWeaverRespec",
            "Free Weaver Respec",
            () => HasSave() && Save_Manager.instance.data.Factions.TheWoven.Enable_FreeRespe,
            value =>
            {
                if (HasSave())
                    Save_Manager.instance.data.Factions.TheWoven.Enable_FreeRespe = value;
            }
        );
    }

    private static void BuildData()
    {
        var card = page.AddCard("Data", "Data");
        classSource = Hud_Manager.Content.Character.Data.class_dropdown;
        page.AddDropdown(
            card,
            "Class",
            "Class",
            classSource,
            () =>
                Refs_Manager.player_data.IsNullOrDestroyed()
                    ? 0
                    : Refs_Manager.player_data.CharacterClass,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.CharacterClass = value;
            }
        );
        page.AddToggle(
            card,
            "Died",
            "Died",
            () => !Refs_Manager.player_data.IsNullOrDestroyed() && Refs_Manager.player_data.Died,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.Died = value;
            }
        );
        page.AddSlider(
            card,
            "Deaths",
            "Deaths",
            string.Empty,
            0f,
            255f,
            true,
            () =>
                Refs_Manager.player_data.IsNullOrDestroyed() ? 0f : Refs_Manager.player_data.Deaths,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.Deaths = (int)value;
            }
        );
        page.AddToggle(
            card,
            "Hardcore",
            "Hardcore",
            () =>
                !Refs_Manager.player_data.IsNullOrDestroyed() && Refs_Manager.player_data.Hardcore,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.Hardcore = value;
            }
        );
        page.AddToggle(
            card,
            "Masochist",
            "Masochist",
            () =>
                !Refs_Manager.player_data.IsNullOrDestroyed() && Refs_Manager.player_data.Masochist,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.Masochist = value;
            }
        );
        page.AddToggle(
            card,
            "Portal",
            "Portal Unlocked",
            () =>
                !Refs_Manager.player_data.IsNullOrDestroyed()
                && Refs_Manager.player_data.PortalUnlocked,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.PortalUnlocked = value;
            }
        );
        page.AddToggle(
            card,
            "SoloChallenge",
            "Solo Challenge",
            () =>
                !Refs_Manager.player_data.IsNullOrDestroyed()
                && Refs_Manager.player_data.SoloChallenge,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.SoloChallenge = value;
            }
        );
        page.AddSlider(
            card,
            "LanternLuminance",
            "Lantern Luminance",
            string.Empty,
            0f,
            255f,
            true,
            () =>
                Refs_Manager.player_data.IsNullOrDestroyed()
                    ? 0f
                    : Refs_Manager.player_data.LanternLuminance,
            value =>
            {
                if (!Refs_Manager.player_data.IsNullOrDestroyed())
                    Refs_Manager.player_data.LanternLuminance = (int)value;
            }
        );
        page.AddButton(
            card,
            "Save",
            "Save Character Data",
            Hud_Manager.Content.Character.Data.Save_Click
        );
    }

    private static void BuildBlessings()
    {
        var card = page.AddCard("Blessings", "Blessings");
        page.AddButton(
            card,
            "Choose",
            "Choose Blessings",
            Mods.Character.Character_Blessings.ChooseBlessings
        );
        page.AddButton(
            card,
            "Discover",
            "Discover All Blessings",
            Hud_Manager.Content.Character.Cheats.DiscoverAllBlessings_Click
        );
        page.AddButton(
            card,
            "Max",
            "Max Out Blessings",
            Mods.Character.Character_Blessings.MaxOutBlessings
        );
        page.AddButton(
            card,
            "Slots",
            "Unlock Blessing Slots",
            Mods.Character.Character_Blessings.UnlockBlessingSlots
        );
    }

    private static void BuildFactions()
    {
        var dropdownSource = Hud_Manager.Content.Character.Faction_Tracker.factions_dropdown;
        var favorSource = Hud_Manager.Content.Character.Faction_Tracker.factions_favor_slider;
        var rankSource = Hud_Manager.Content.Character.Faction_Tracker.factions_rank_slider;
        var reputationSource = Hud_Manager
            .Content
            .Character
            .Faction_Tracker
            .factions_reputation_slider;
        factionSelection = dropdownSource.IsNullOrDestroyed() ? 0 : dropdownSource.value;
        factionFavor = favorSource.IsNullOrDestroyed() ? 0f : favorSource.value;
        factionRank = rankSource.IsNullOrDestroyed() ? 0f : rankSource.value;
        factionReputation = reputationSource.IsNullOrDestroyed() ? 0f : reputationSource.value;

        var card = page.AddCard("Factions", "Factions");
        var dropdown = page.AddDropdown(
            card,
            "Faction",
            "Faction",
            dropdownSource,
            () => factionSelection,
            value =>
            {
                factionSelection = value;
                Hud_Manager.Content.Character.Update_Faction_Data();
            }
        );
        if (!dropdown.IsNullOrDestroyed())
            Hud_Manager.Content.Character.Faction_Tracker.factions_dropdown = dropdown;

        var favor = page.AddSlider(
            card,
            "Favor",
            "Favor",
            string.Empty,
            SliderMinimum(favorSource, 0f),
            SliderMaximum(favorSource, 999999f),
            true,
            () => factionFavor,
            value => factionFavor = value
        );
        if (!favor.IsNullOrDestroyed())
            Hud_Manager.Content.Character.Faction_Tracker.factions_favor_slider = favor;
        page.AddButton(
            card,
            "GainFavor",
            "Gain Favor",
            Hud_Manager.Content.Character.Faction_Tracker.factions_gain_favor_Click
        );

        var rank = page.AddSlider(
            card,
            "Rank",
            "Rank",
            string.Empty,
            SliderMinimum(rankSource, 0f),
            SliderMaximum(rankSource, 12f),
            true,
            () => factionRank,
            value => factionRank = value
        );
        if (!rank.IsNullOrDestroyed())
            Hud_Manager.Content.Character.Faction_Tracker.factions_rank_slider = rank;
        page.AddButton(
            card,
            "SetRank",
            "Set Rank",
            Hud_Manager.Content.Character.Faction_Tracker.factions_set_rank_Click
        );

        var reputation = page.AddSlider(
            card,
            "Reputation",
            "Reputation",
            string.Empty,
            SliderMinimum(reputationSource, 0f),
            SliderMaximum(reputationSource, 999999f),
            true,
            () => factionReputation,
            value => factionReputation = value
        );
        if (!reputation.IsNullOrDestroyed())
            Hud_Manager.Content.Character.Faction_Tracker.factions_reputation_slider = reputation;
        page.AddButton(
            card,
            "SetReputation",
            "Set Reputation",
            Hud_Manager.Content.Character.Faction_Tracker.factions_set_reputation_Click
        );
    }

    private static float SliderMinimum(Slider slider, float fallback) =>
        slider.IsNullOrDestroyed() ? fallback : slider.minValue;

    private static float SliderMaximum(Slider slider, float fallback) =>
        slider.IsNullOrDestroyed() ? fallback : slider.maxValue;

    private static void RefreshClassOptions()
    {
        if (
            classSource.IsNullOrDestroyed() || Refs_Manager.character_class_list.IsNullOrDestroyed()
        )
        {
            return;
        }
        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new();
        foreach (CharacterClass characterClass in Refs_Manager.character_class_list.classes)
            options.Add(new Dropdown.OptionData { text = characterClass.className });
        classSource.options = options;
    }

    private static bool HasSave() =>
        !Save_Manager.instance.IsNullOrDestroyed() && Save_Manager.instance.initialized;
}
