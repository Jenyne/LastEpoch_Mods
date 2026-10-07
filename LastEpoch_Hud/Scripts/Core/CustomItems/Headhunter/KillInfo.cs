using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>One kill as Core sees it. ModStatIds may be null (no mods).</summary>
public readonly record struct KillInfo(KillKind Kind, bool ByMinion, IReadOnlyList<int> ModStatIds);
