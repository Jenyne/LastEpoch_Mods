using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

/// <summary>Between kills: re-adds live rows with grown values when the total stacks change the factor.</summary>
public sealed class HeadhunterGrowthReapply
{
    private readonly HeadhunterResolvedConfig _config;
    private readonly HeadhunterStackState _stacks;
    private readonly HeadhunterValueGrowth _growth;
    private readonly List<BuffAction> _actions = new();

    public HeadhunterGrowthReapply(
        HeadhunterResolvedConfig config,
        HeadhunterStackState stacks,
        HeadhunterValueGrowth growth
    )
    {
        _config = config;
        _stacks = stacks;
        _growth = growth;
    }

    /// <summary>Syncs stacks to the remaining times, then returns the re-adds. The list is reused: valid until the next call.</summary>
    public IReadOnlyList<BuffAction> Check(IReadOnlyList<float> remaining)
    {
        _actions.Clear();
        _stacks.SyncRemaining(remaining);
        if (!_growth.Update(_stacks.Total))
        {
            return _actions;
        }

        for (int row = 0; row < _config.Stats.Count; row++)
        {
            AddLiveRow(row, remaining);
        }

        return _actions;
    }

    private void AddLiveRow(int row, IReadOnlyList<float> remaining)
    {
        int stacks = _stacks.Get(row);
        if (stacks == 0)
        {
            return;
        }

        HeadhunterBuffStat stat = _config.Stats[row];
        _actions.Add(
            new BuffAction(
                BuffActionKind.Add,
                stat.BuffName,
                stat.StatId,
                stat.AddedFor(stacks, _growth.Factor),
                stat.IncreasedFor(stacks, _growth.Factor),
                remaining[row],
                stacks,
                stat.Tags
            )
        );
    }
}
