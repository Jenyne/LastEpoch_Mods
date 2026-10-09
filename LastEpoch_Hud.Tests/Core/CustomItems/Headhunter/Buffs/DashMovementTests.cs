using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

/// <summary>The mirror enum must keep the game's names and numbers.</summary>
public sealed class DashMovementTests
{
    [Fact]
    public void Mirror_MatchesGameAbilityMovement()
    {
        GameEnvironment.SkipWithoutGame();

        (string Name, int Value)[] game = ReadGameValues();

        (string, int)[] mirror = Enum.GetValues<DashMovement>()
            .Select(value => (value.ToString(), (int)value))
            .ToArray();
        Assert.Equal(game, mirror);
    }

    private static (string Name, int Value)[] ReadGameValues()
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(GameEnvironment.Il2CppDir);
        using var module = ModuleDefinition.ReadModule(
            Path.Combine(GameEnvironment.Il2CppDir, "Il2CppLE.dll"),
            new ReaderParameters { AssemblyResolver = resolver }
        );
        TypeDefinition type = module.GetType("Il2Cpp", "AbilityMovement");
        Assert.NotNull(type);
        return type
            .Fields.Where(field => field.IsStatic && field.IsLiteral)
            .Select(field => (field.Name, Convert.ToInt32(field.Constant)))
            .ToArray();
    }
}
