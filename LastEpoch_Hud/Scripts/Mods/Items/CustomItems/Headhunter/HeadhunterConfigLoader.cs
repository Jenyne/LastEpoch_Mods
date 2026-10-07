using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Exports the default Headhunter config when missing, then reads and holds it.</summary>
internal static class HeadhunterConfigLoader
{
    private static readonly CustomItemConfigStore _store = new("headhunter.json");

    private static readonly Dictionary<string, int> _statIds = EnumIdMap.Build(typeof(SP));

    public static HeadhunterConfig Current { get; private set; } = HeadhunterConfigDefaults.Config;
    public static HeadhunterResolvedConfig Resolved { get; private set; }
    public static IHeadhunterMechanic Mechanic { get; private set; }

    public static void Load()
    {
        ExportDefaultsIfMissing();
        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            _store.Read(),
            new HashSet<string>(_statIds.Keys, StringComparer.Ordinal)
        );
        LogProblems(result.Problems);
        Current = result.Config;
        ResolveAndCreateMechanic();
        Main.logger_instance?.Msg(
            "Headhunter config loaded: " + Current.Stats.Count + " stat(s) from " + _store.FilePath
        );
    }

    private static void ExportDefaultsIfMissing()
    {
        if (_store.Exists())
        {
            return;
        }
        _store.Write(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
    }

    private static void ResolveAndCreateMechanic()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterResolvedConfig resolved = HeadhunterConfigResolver.Resolve(
            Current,
            _statIds,
            problems
        );
        Mechanic = HeadhunterMechanics.Create(resolved, problems);
        Resolved = resolved;
        LogProblems(problems);
    }

    private static void LogProblems(IReadOnlyList<HeadhunterConfigProblem> problems)
    {
        foreach (HeadhunterConfigProblem problem in problems)
        {
            Main.logger_instance?.Warning(
                "Headhunter config: " + problem.Path + ": " + problem.Message
            );
        }
    }
}
