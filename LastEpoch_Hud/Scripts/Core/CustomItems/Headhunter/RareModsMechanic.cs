using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Rare or magic kill: stack its mod stats as buffs. Boss or miniboss kill: one random not-live buff. Always refreshes all live ones.</summary>
public sealed class RareModsMechanic : IHeadhunterMechanic
{
    private readonly HeadhunterResolvedConfig _config;
    private readonly HeadhunterStackState _stacks;
    private readonly List<BuffAction> _actions = new();
    private readonly HashSet<int> _handled = new();
    private readonly IHeadhunterRandom _random;

    public RareModsMechanic(
        HeadhunterResolvedConfig config,
        HeadhunterStackState stacks,
        IHeadhunterRandom random
    )
    {
        _config = config;
        _stacks = stacks;
        _random = random;
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
        AddOwnRows(kill);
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

    private void AddOwnRows(KillInfo kill)
    {
        if (kill.Kind is KillKind.Boss or KillKind.Miniboss)
        {
            AddRandomRow();
            return;
        }

        AddKillStats(kill.ModStats);
    }

    private void AddRandomRow()
    {
        int count = 0;
        for (int row = 0; row < _config.Stats.Count; row++)
        {
            if (_stacks.Get(row) == 0)
            {
                count++;
            }
        }

        if (count == 0)
        {
            return;
        }

        int skip = _random.Next(count);
        for (int row = 0; row < _config.Stats.Count; row++)
        {
            if (_stacks.Get(row) != 0 || skip-- > 0)
            {
                continue;
            }

            _handled.Add(row);
            _stacks.TryAdd(row, _config.MaxStacks);
            Emit(BuffActionKind.Add, row);
            return;
        }
    }

    private void AddKillStats(IReadOnlyList<HeadhunterStatKey> mods)
    {
        if (mods == null)
        {
            return;
        }

        for (int i = 0; i < mods.Count; i++)
        {
            if (!_config.TryGetRow(mods[i], out int row) || !_handled.Add(row))
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
                stacks,
                stat.Tags
            )
        );
    }
}
