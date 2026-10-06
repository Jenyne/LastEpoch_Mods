using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

public static class HeadhunterKillEvents
{
    public static bool OnKillEventInitialized;
    public static bool OnMinionKillEventInitialized;

    private static readonly System.Action<Ability, Actor> _onKillAction = new(OnKill);
    private static readonly System.Action<Summoned, Ability, Actor> _onMinionKillAction = new(
        OnMinionKill
    );

    public static void InitOnKillEvent()
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return;
        }
        if (Refs_Manager.player_actor.gameObject.IsNullOrDestroyed())
        {
            return;
        }
        AbilityEventListener listener =
            Refs_Manager.player_actor.gameObject.GetComponent<AbilityEventListener>();
        if (listener.IsNullOrDestroyed())
        {
            return;
        }
        listener.add_onKillEvent(_onKillAction);
        OnKillEventInitialized = true;
    }

    public static void InitOnMinionKillEvent()
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return;
        }
        if (Refs_Manager.player_actor.gameObject.IsNullOrDestroyed())
        {
            return;
        }
        SummonTracker listener = Refs_Manager.player_actor.gameObject.GetComponent<SummonTracker>();
        if (listener.IsNullOrDestroyed())
        {
            return;
        }
        listener.add_minionKillEvent(_onMinionKillAction);
        OnMinionKillEventInitialized = true;
    }

    private static void OnKill(Ability ability, Actor killedActor)
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return;
        }
        if (
            !Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(
                CustomUniqueSpecs.Headhunter.UniqueId
            )
        )
        {
            return;
        }
        HeadhunterBuffs.GenerateBuffs();
    }

    private static void OnMinionKill(Summoned summon, Ability ability, Actor killedActor)
    {
        OnKill(ability, killedActor);
    }
}
