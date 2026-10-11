using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Patches of the built mod, loaded once.</summary>
internal static class ModPatches
{
    private static readonly Lazy<IReadOnlyList<PatchDeclaration>> _cache = new(Scan);

    public static IReadOnlyList<PatchDeclaration> Load()
    {
        GameEnvironment.SkipWithoutGame();
        GameEnvironment.SkipWithoutModBuild();
        return _cache.Value;
    }

    private static IReadOnlyList<PatchDeclaration> Scan() =>
        PatchScanner.Scan(GameEnvironment.ReadGameModule(GameEnvironment.ModDll));
}
