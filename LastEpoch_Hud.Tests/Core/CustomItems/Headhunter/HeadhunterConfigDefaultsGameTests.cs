using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

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

    private static HashSet<string> ReadStatNames()
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(GameEnvironment.Il2CppDir);
        using var module = ModuleDefinition.ReadModule(
            Path.Combine(GameEnvironment.Il2CppDir, "Il2CppLE.dll"),
            new ReaderParameters { AssemblyResolver = resolver }
        );
        TypeDefinition sp = module.GetType("Il2Cpp", "SP");
        Assert.NotNull(sp);
        return sp
            .Fields.Where(field => field.IsStatic && field.IsLiteral)
            .Select(field => field.Name)
            .ToHashSet(StringComparer.Ordinal);
    }
}
