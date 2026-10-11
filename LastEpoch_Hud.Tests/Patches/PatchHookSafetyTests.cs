using Iced.Intel;
using LastEpoch_Hud.Tests.Support;
using Microsoft.Win32.SafeHandles;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>No patch may sit on code MelonLoader's detour cannot relocate safely.</summary>
public sealed class PatchHookSafetyTests
{
    private const string Header =
        "Hook-unsafe patch targets: MelonLoader's detour overwrites the first 6 bytes and mis-relocates a short branch there, so the game crashes (int3). Remove the patch or hook another method:";

    private static readonly Lazy<Cpp2IlAddressIndex> _index = new(() =>
        Cpp2IlAddressIndex.Load(GameEnvironment.Cpp2IlOutDir)
    );

    [Fact]
    public void Patches_MapToCpp2IlMethods()
    {
        (IReadOnlyList<PatchDeclaration> patches, Cpp2IlAddressIndex index) = Index();

        var unmapped = Resolved(patches)
            .Where(p => !index.TryGetOffsets(p.Target.FullName, p.Method, out _))
            .Select(p => $"{p.Id} -> {p.Target.FullName}.{p.Method}")
            .ToList();

        Assert.True(
            unmapped.Count == 0,
            "Patch targets missing in cpp2il output (mapping drift or stale cpp2il_out):\n"
                + string.Join("\n", unmapped)
        );
    }

    [Fact]
    public void Patches_AreHookSafe()
    {
        (IReadOnlyList<PatchDeclaration> patches, Cpp2IlAddressIndex index) = Index();
        using SafeFileHandle file = File.OpenHandle(GameEnvironment.GameAssemblyDll);

        var hits = Resolved(patches).SelectMany(p => Hits(p, index, file)).ToList();

        Assert.True(hits.Count == 0, Header + "\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Patches_HaveNativeOffsets()
    {
        (IReadOnlyList<PatchDeclaration> patches, Cpp2IlAddressIndex index) = Index();

        var withoutOffsets = Resolved(patches)
            .Where(p => !HasOffsets(p, index))
            .Select(p => $"{p.Id} -> {p.Target.FullName}.{p.Method}")
            .ToList();

        Assert.True(
            withoutOffsets.Count < Resolved(patches).Count(),
            "No patch target has a native offset (cpp2il attribute format changed?):\n"
                + string.Join("\n", withoutOffsets)
        );
    }

    private static bool HasOffsets(PatchDeclaration patch, Cpp2IlAddressIndex index) =>
        index.TryGetOffsets(patch.Target.FullName, patch.Method, out IReadOnlyList<long> offsets)
        && offsets.Count > 0;

    private static IEnumerable<PatchDeclaration> Resolved(
        IReadOnlyList<PatchDeclaration> patches
    ) => patches.Where(p => PatchTargetResolver.Problem(p) == null);

    private static IEnumerable<string> Hits(
        PatchDeclaration patch,
        Cpp2IlAddressIndex index,
        SafeFileHandle file
    )
    {
        if (
            !index.TryGetOffsets(
                patch.Target.FullName,
                patch.Method,
                out IReadOnlyList<long> offsets
            )
        )
        {
            yield break;
        }

        string overloads = offsets.Count > 1 ? $" (one of {offsets.Count} overloads)" : "";
        foreach (long offset in offsets)
        {
            if (!HookSafety.TryFindShortBranch(ReadCode(file, offset), out Instruction branch))
            {
                continue;
            }

            yield return $"{patch.Id} -> {patch.Target.FullName}.{patch.Method}: +0x{branch.IP:x} {Describe(branch)}{overloads}";
        }
    }

    private static string Describe(Instruction branch) =>
        $"{branch.Mnemonic.ToString().ToLowerInvariant()} short";

    private static byte[] ReadCode(SafeFileHandle file, long offset)
    {
        byte[] code = new byte[HookSafety.CodeBytes];
        RandomAccess.Read(file, code, offset);
        return code;
    }

    private static (IReadOnlyList<PatchDeclaration> Patches, Cpp2IlAddressIndex Index) Index()
    {
        IReadOnlyList<PatchDeclaration> patches = ModPatches.Load();
        GameEnvironment.SkipWithoutCpp2IlOut();
        return (patches, _index.Value);
    }
}
