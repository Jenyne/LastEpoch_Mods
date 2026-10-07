using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Maps a mechanic id from the config to its implementation.</summary>
public static class HeadhunterMechanics
{
    public const string RareModsId = "rare_mods";

    public static IHeadhunterMechanic Create(
        HeadhunterResolvedConfig config,
        HeadhunterStackState stacks,
        IHeadhunterRandom random,
        ICollection<HeadhunterConfigProblem> problems
    )
    {
        if (string.Equals(config.Mechanic, RareModsId, StringComparison.Ordinal))
        {
            return new RareModsMechanic(config, stacks, random);
        }

        problems.Add(
            new HeadhunterConfigProblem(
                HeadhunterConfigKeys.Mechanic,
                "Unknown mechanic, using " + RareModsId
            )
        );
        return new RareModsMechanic(config, stacks, random);
    }
}
