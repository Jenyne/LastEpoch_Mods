using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Hooks the player's kill events and reports Headhunter-relevant kills.</summary>
internal static class HeadhunterKillSource
{
    private static readonly System.Action<Ability, Actor> _onKillAction = new(OnKill);
    private static readonly System.Action<Summoned, Ability, Actor> _onMinionKillAction = new(
        OnMinionKill
    );
    private static readonly List<int> _modStatIds = new();
    private static bool _killHooked;
    private static bool _minionKillHooked;

    public static void EnsureHooked()
    {
        if (!_killHooked)
        {
            HookKill();
        }

        if (!_minionKillHooked)
        {
            HookMinionKill();
        }
    }

    public static void ResetHooks()
    {
        _killHooked = false;
        _minionKillHooked = false;
    }

    internal static bool IsHeadhunterWorn()
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return false;
        }

        return Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(
            CustomUniqueSpecs.Headhunter.UniqueId
        );
    }

    private static void HookKill()
    {
        AbilityEventListener listener = PlayerComponent<AbilityEventListener>();
        if (listener.IsNullOrDestroyed())
        {
            return;
        }

        listener.add_onKillEvent(_onKillAction);
        _killHooked = true;
    }

    private static void HookMinionKill()
    {
        SummonTracker tracker = PlayerComponent<SummonTracker>();
        if (tracker.IsNullOrDestroyed())
        {
            return;
        }

        tracker.add_minionKillEvent(_onMinionKillAction);
        _minionKillHooked = true;
    }

    private static T PlayerComponent<T>()
        where T : UnityEngine.Component
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return null;
        }

        if (Refs_Manager.player_actor.gameObject.IsNullOrDestroyed())
        {
            return null;
        }

        return Refs_Manager.player_actor.gameObject.GetComponent<T>();
    }

    private static void OnKill(Ability ability, Actor killedActor)
    {
        Report(killedActor, false);
    }

    private static void OnMinionKill(Summoned summon, Ability ability, Actor killedActor)
    {
        Report(killedActor, true);
    }

    private static void Report(Actor killed, bool byMinion)
    {
        if (killed.IsNullOrDestroyed())
        {
            return;
        }

        KillKind kind = KillKindClassifier.Classify(
            killed.isBoss(),
            killed.isMiniboss(),
            killed.IsRare
        );
        if (kind == KillKind.Normal)
        {
            return;
        }

        if (!IsHeadhunterWorn())
        {
            return;
        }

        ReadModStatIds(killed);
        HeadhunterKillHandler.Handle(new KillInfo(kind, byMinion, _modStatIds));
    }

    private static void ReadModStatIds(Actor killed)
    {
        _modStatIds.Clear();
        var manager = MonsterRarityManager.getInstance();
        if (manager.IsNullOrDestroyed())
        {
            return;
        }

        Il2CppSystem.Collections.Generic.HashSet<int> keys = killed.getAppliedModKeys();
        if (keys == null)
        {
            return;
        }

        foreach (int key in keys)
        {
            AddModStats(manager, key);
        }
    }

    private static void AddModStats(MonsterRarityManager manager, int key)
    {
        if (!manager.TryGetMonsterModFromKey(key, out MonsterMod mod))
        {
            return;
        }

        if (mod.IsNullOrDestroyed())
        {
            return;
        }

        if (mod.modType != MonsterMod.ModType.Prefix && mod.modType != MonsterMod.ModType.Suffix)
        {
            return;
        }

        StatsMonsterMod statsMod = mod.TryCast<StatsMonsterMod>();
        if (statsMod.IsNullOrDestroyed() || statsMod.stats == null)
        {
            return;
        }

        foreach (Stats.Stat stat in statsMod.stats)
        {
            _modStatIds.Add((int)stat.property);
        }
    }
}
