using System;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Character;

internal static class Character_Gold
{
    public static Button SpawnButton { get; private set; }

    public static void BuildButton(Button source)
    {
        if (source.IsNullOrDestroyed() || !SpawnButton.IsNullOrDestroyed())
            return;
        var clone = UnityEngine.Object.Instantiate(
            source.gameObject,
            source.transform.parent,
            false
        );
        clone.name = "Btn_Character_Cheats_SpawnGold";
        SpawnButton = clone.GetComponent<Button>();
        SpawnButton.onClick = new Button.ButtonClickedEvent();
        Hud_Manager.Events.Set_Button_Event(SpawnButton, new Action(SpawnMillion));
        foreach (var label in clone.GetComponentsInChildren<Text>(true))
            LocaleRegistry.Apply(label, "Spawn 1,000,000 Gold");
        foreach (var label in clone.GetComponentsInChildren<Il2CppTMPro.TMP_Text>(true))
            LocaleRegistry.Apply(label, "Spawn 1,000,000 Gold");
        SpawnButton.interactable = true;
        clone.SetActive(true);
    }

    static void SpawnMillion()
    {
        if (
            !Scenes.IsGameScene()
            || Refs_Manager.player_actor.IsNullOrDestroyed()
            || Refs_Manager.ground_item_manager.IsNullOrDestroyed()
        )
            return;
        try
        {
            Refs_Manager.ground_item_manager.dropGoldForPlayer(
                Refs_Manager.player_actor,
                1000000,
                Refs_Manager.player_actor.position(),
                false
            );
            Main.logger_instance?.Msg("Gold: spawned 1,000,000 for the local player.");
        }
        catch (Exception ex)
        {
            Main.logger_instance?.Error("Spawn Gold failed: " + ex.Message);
        }
    }
}
