using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Rare, boss or miniboss kill: gain its mod stats as buffs, refresh all active ones.</summary>
public sealed class RareModsMechanic : IHeadhunterMechanic
{
    private readonly HeadhunterResolvedConfig _config;
    private readonly List<BuffAction> _actions = new();
    private readonly HashSet<int> _handled = new();

    public RareModsMechanic(HeadhunterResolvedConfig config)
    {
        _config = config;
    }

    public IReadOnlyList<BuffAction> OnKill(KillInfo kill, IReadOnlySet<int> activeStatIds)
    {
        _actions.Clear();
        _handled.Clear();
        if (!Fires(kill))
        {
            return _actions;
        }

        AddKillStats(kill.ModStatIds, activeStatIds);
        RefreshOtherActive(activeStatIds);
        return _actions;
    }

    private bool Fires(KillInfo kill)
    {
        if (kill.ByMinion && !_config.Triggers.MinionKills)
        {
            return false;
        }

        return kill.Kind switch
        {
            KillKind.Rare => _config.Triggers.Rare,
            KillKind.Boss => _config.Triggers.Boss,
            KillKind.Miniboss => _config.Triggers.Miniboss,
            KillKind.Magic => _config.Triggers.Magic,
            _ => false,
        };
    }

    private void AddKillStats(IReadOnlyList<int> ids, IReadOnlySet<int> active)
    {
        if (ids == null)
        {
            return;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            if (!_config.TryGetStat(ids[i], out HeadhunterBuffStat stat) || !_handled.Add(ids[i]))
            {
                continue;
            }

            Emit(active.Contains(stat.StatId) ? BuffActionKind.Refresh : BuffActionKind.Add, stat);
        }
    }

    private void RefreshOtherActive(IReadOnlySet<int> active)
    {
        IReadOnlyList<HeadhunterBuffStat> stats = _config.Stats;
        for (int i = 0; i < stats.Count; i++)
        {
            if (!active.Contains(stats[i].StatId) || !_handled.Add(stats[i].StatId))
            {
                continue;
            }

            Emit(BuffActionKind.Refresh, stats[i]);
        }
    }

    private void Emit(BuffActionKind kind, HeadhunterBuffStat stat)
    {
        _actions.Add(
            new BuffAction(
                kind,
                stat.BuffName,
                stat.StatId,
                stat.Added,
                stat.Increased,
                _config.DurationSeconds
            )
        );
    }
}
