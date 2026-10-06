using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Mjolner;

internal static class MjolnerHitEvents
{
    private static bool _onHitEventInitialized;
    private static readonly System.Action<Ability, Actor> _onHitAction = new(OnHit);

    internal static void Update()
    {
        if (!_onHitEventInitialized)
        {
            InitOnHitEvent();
        }
    }

    internal static void Reset()
    {
        _onHitEventInitialized = false;
    }

    private static void InitOnHitEvent()
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

        listener.add_onHitEvent(_onHitAction);
        _onHitEventInitialized = true;
    }

    private static void OnHit(Ability ability, Actor hitActor)
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return;
        }

        if (!IsMjolnerActive())
        {
            return;
        }

        if (
            Save_Manager.instance.data.Items.Mjolner.ProcAnyLightningSpell
            && !ability.tags.HasFlag(AT.Spell)
        )
        {
            MjolnerTrigger.AllSkills(hitActor);
            return;
        }

        MjolnerTrigger.SocketedSkills(hitActor);
    }

    private static bool IsMjolnerActive()
    {
        Save_Manager.Data.Mjolner settings = Save_Manager.instance.data.Items.Mjolner;
        return Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(
                CustomUniqueSpecs.Mjolner.UniqueId
            )
            && Refs_Manager.player_actor.stats.GetAttributeValue(CoreAttribute.Attribute.Strength)
                >= settings.StrRequirement
            && Refs_Manager.player_actor.stats.GetAttributeValue(
                CoreAttribute.Attribute.Intelligence
            ) >= settings.IntRequirement;
    }
}
