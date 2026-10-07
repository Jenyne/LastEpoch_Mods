using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Exports the default Headhunter config when missing, then reads and holds it.</summary>
internal static class HeadhunterConfigLoader
{
    private static readonly CustomItemConfigStore _store = new("headhunter.json");

    public static HeadhunterConfig Current { get; private set; } = HeadhunterConfigDefaults.Config;

    public static void Load()
    {
        ExportDefaultsIfMissing();
        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            _store.Read(),
            KnownStatNames()
        );
        LogProblems(result.Problems);
        Current = result.Config;
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

    private static HashSet<string> KnownStatNames()
    {
        return new HashSet<string>(Enum.GetNames(typeof(SP)), StringComparer.Ordinal);
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
