using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One Headhunter rule set.</summary>
public interface IHeadhunterMechanic
{
    /// <summary>Buff changes for one kill. The list is reused: valid until the next call.</summary>
    IReadOnlyList<BuffAction> OnKill(KillInfo kill, IReadOnlySet<int> liveRows);

    /// <summary>Drops all per-run state (stacks and anything kept between kills).</summary>
    void Reset();
}
