using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config;

/// <summary>The exported default stats must exist in the game's SP enum.</summary>
public sealed class HeadhunterConfigDefaultsGameTests
{
    [Fact]
    public void DefaultStats_AreAllGameStatNames()
    {
        GameEnvironment.SkipWithoutGame();
        HashSet<string> spNames = ReadStatNames();

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config),
            spNames
        );

        Assert.Empty(result.Problems);
        Assert.Equal(HeadhunterConfigDefaults.Stats.Count, result.Config.Stats.Count);
    }

    [Fact]
    public void DefaultStats_ResolveAgainstGameStatsAndTags()
    {
        GameEnvironment.SkipWithoutGame();
        Dictionary<string, int> spIds = ReadEnumIds("SP");
        Dictionary<string, int> atIds = ReadEnumIds("AT");
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig resolved = HeadhunterConfigResolver.Resolve(
            HeadhunterConfigDefaults.Config,
            spIds,
            atIds,
            problems
        );

        Assert.Empty(problems);
        Assert.Equal(HeadhunterConfigDefaults.Stats.Count, resolved.Stats.Count);
        Assert.Equal(HeadhunterAffixDefaults.AffixMap.Count, resolved.AffixCount);
    }

    private static HashSet<string> ReadStatNames()
    {
        return ReadEnumIds("SP").Keys.ToHashSet(StringComparer.Ordinal);
    }

    private static Dictionary<string, int> ReadEnumIds(string typeName)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(GameEnvironment.Il2CppDir);
        using var module = ModuleDefinition.ReadModule(
            Path.Combine(GameEnvironment.Il2CppDir, "Il2CppLE.dll"),
            new ReaderParameters { AssemblyResolver = resolver }
        );
        TypeDefinition type = module.GetType("Il2Cpp", typeName);
        Assert.NotNull(type);
        return type
            .Fields.Where(field => field.IsStatic && field.IsLiteral)
            .ToDictionary(
                field => field.Name,
                field => Convert.ToInt32(field.Constant),
                StringComparer.Ordinal
            );
    }
}
