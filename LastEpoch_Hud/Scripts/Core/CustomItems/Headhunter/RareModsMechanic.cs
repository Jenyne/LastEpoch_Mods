using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Rare, boss or miniboss kill: stack its mod stats as buffs, refresh all live ones.</summary>
public sealed class RareModsMechanic : IHeadhunterMechanic
{
    private readonly HeadhunterResolvedConfig _config;
    private readonly HeadhunterStackState _stacks;
    private readonly List<BuffAction> _actions = new();
    private readonly HashSet<int> _handled = new();

    public RareModsMechanic(HeadhunterResolvedConfig config, HeadhunterStackState stacks)
    {
        _config = config;
        _stacks = stacks;
    }

    public IReadOnlyList<BuffAction> OnKill(KillInfo kill, IReadOnlySet<int> liveRows)
    {
        _actions.Clear();
        _handled.Clear();
        if (!Fires(kill))
        {
            return _actions;
        }

        _stacks.Sync(liveRows);
        AddKillStats(kill.ModStatIds);
        RefreshOtherLive();
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

    private void AddKillStats(IReadOnlyList<int> ids)
    {
        if (ids == null)
        {
            return;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            if (!_config.TryGetRow(ids[i], out int row) || !_handled.Add(row))
            {
                continue;
            }

            bool added = _stacks.TryAdd(row, _config.MaxStacks);
            Emit(added ? BuffActionKind.Add : BuffActionKind.Refresh, row);
        }
    }

    private void RefreshOtherLive()
    {
        for (int row = 0; row < _config.Stats.Count; row++)
        {
            if (_stacks.Get(row) == 0 || !_handled.Add(row))
            {
                continue;
            }

            Emit(BuffActionKind.Refresh, row);
        }
    }

    private void Emit(BuffActionKind kind, int row)
    {
        HeadhunterBuffStat stat = _config.Stats[row];
        int stacks = _stacks.Get(row);
        _actions.Add(
            new BuffAction(
                kind,
                stat.BuffName,
                stat.StatId,
                stat.AddedFor(stacks),
                stat.IncreasedFor(stacks),
                _config.DurationSeconds,
                stacks
            )
        );
    }
}
