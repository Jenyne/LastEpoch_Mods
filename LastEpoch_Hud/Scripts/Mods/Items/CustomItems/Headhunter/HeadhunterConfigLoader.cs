using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Exports the default Headhunter config when missing, then reads and holds it.</summary>
internal static class HeadhunterConfigLoader
{
    private static readonly CustomItemConfigStore _store = new("headhunter.json");

    private static readonly Dictionary<string, int> _statIds = EnumIdMap.Build(typeof(SP));

    private static readonly HashSet<string> _knownStats = new(
        _statIds.Keys,
        StringComparer.Ordinal
    );

    private static readonly ConfigChangeDetector _changes = new(1.0);

    public static HeadhunterConfig Current { get; private set; } = HeadhunterConfigDefaults.Config;
    public static HeadhunterResolvedConfig Resolved { get; private set; }
    public static IHeadhunterMechanic Mechanic { get; private set; }

    public static void Load()
    {
        ExportDefaultsIfMissing();
        HeadhunterConfigParseResult result = ParseFile();
        LogProblems(result.Problems);
        Current = result.Config;
        ResolveAndCreateMechanic();
        Main.logger_instance?.Msg(
            "Headhunter config loaded: " + Current.Stats.Count + " stat(s) from " + _store.FilePath
        );
        _changes.Remember(_store.LastWriteUtc());
    }

    public static void ReloadIfChanged(double now)
    {
        if (!_changes.IsCheckDue(now))
        {
            return;
        }
        if (!_changes.HasChanged(_store.LastWriteUtc()))
        {
            return;
        }

        Reload();
    }

    private static void Reload()
    {
        HeadhunterConfigParseResult result = ParseFile();
        LogProblems(result.Problems);
        if (!result.IsReadable)
        {
            return;
        }

        HeadhunterBuffClearer.ClearAll(Resolved);
        Current = result.Config;
        ResolveAndCreateMechanic();
        Main.logger_instance?.Msg(
            "Headhunter config reloaded: " + Current.Stats.Count + " stat(s)"
        );
    }

    private static HeadhunterConfigParseResult ParseFile()
    {
        return HeadhunterConfigParser.Parse(_store.Read(), _knownStats);
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
