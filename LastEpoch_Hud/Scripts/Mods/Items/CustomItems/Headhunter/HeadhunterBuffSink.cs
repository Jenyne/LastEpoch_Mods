using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Applies Headhunter buff actions to the player's buffs.</summary>
internal static class HeadhunterBuffSink
{
    public static StatBuffs PlayerBuffs()
    {
        if (Refs_Manager.player_actor.IsNullOrDestroyed())
        {
            return null;
        }

        StatBuffs buffs = Refs_Manager.player_actor.statBuffs;
        return buffs.IsNullOrDestroyed() ? null : buffs;
    }

    public static void FillActive(
        StatBuffs buffs,
        IReadOnlyList<HeadhunterBuffStat> stats,
        HashSet<int> active
    )
    {
        active.Clear();
        for (int i = 0; i < stats.Count; i++)
        {
            if (TryGetLive(buffs, stats[i].BuffName, out _))
            {
                active.Add(stats[i].StatId);
            }
        }
    }

    public static void FillRemaining(
        StatBuffs buffs,
        IReadOnlyList<HeadhunterBuffStat> stats,
        float[] remaining
    )
    {
        for (int i = 0; i < stats.Count; i++)
        {
            remaining[i] = TryGetLive(buffs, stats[i].BuffName, out Buff buff)
                ? buff.remainingDuration
                : 0f;
        }
    }

    public static void Apply(StatBuffs buffs, IReadOnlyList<BuffAction> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            ApplyOne(buffs, actions[i]);
        }
    }

    private static void ApplyOne(StatBuffs buffs, BuffAction action)
    {
        switch (action.Kind)
        {
            case BuffActionKind.Add:
                Replace(buffs, action);
                break;
            case BuffActionKind.Refresh:
                Refresh(buffs, action);
                break;
            case BuffActionKind.Remove:
                buffs.removeBuffsWithName(action.BuffName);
                break;
        }
    }

    private static void Refresh(StatBuffs buffs, BuffAction action)
    {
        if (!TryGetLive(buffs, action.BuffName, out Buff buff))
        {
            Replace(buffs, action);
            return;
        }

        buff.remainingDuration = action.DurationSeconds;
    }

    private static void Replace(StatBuffs buffs, BuffAction action)
    {
        buffs.removeBuffsWithName(action.BuffName);
        buffs.addBuff(
            action.DurationSeconds,
            (SP)action.StatId,
            action.Added,
            action.Increased,
            null,
            AT.None,
            0,
            0,
            action.BuffName
        );
    }

    private static bool TryGetLive(StatBuffs buffs, string name, out Buff buff)
    {
        buff = null;
        if (buffs.activeBuffNames == null)
        {
            return false;
        }

        if (!buffs.activeBuffNames.TryGetValue(name, out buff))
        {
            return false;
        }

        return !buff.IsNullOrDestroyed() && buff.remainingDuration > 0f;
    }
}
