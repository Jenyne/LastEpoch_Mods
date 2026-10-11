using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LastEpoch_Hud.Tests.Patches;

public sealed class CosmeticsOfflineGuardTests
{
    private const string CosmeticsNamespace = "LastEpoch_Hud.Scripts.Mods.Cosmetics";
    private const string GuardType = "Il2CppEHG.Multiplayer.GameplayEnvironment";
    private const string GuardGetter = "get_IsOnlinePlay";

    private static readonly string[] _patchAttributes =
    {
        "HarmonyLib.HarmonyPrefix",
        "HarmonyLib.HarmonyPostfix",
        "HarmonyLib.HarmonyFinalizer",
    };

    private static readonly string[] _patchMethodNames = { "Prefix", "Postfix", "Finalizer" };

    private static readonly OpCode[] _skippedOpCodes =
    {
        OpCodes.Nop,
        OpCodes.Stloc,
        OpCodes.Stloc_S,
        OpCodes.Stloc_0,
        OpCodes.Stloc_1,
        OpCodes.Stloc_2,
        OpCodes.Stloc_3,
        OpCodes.Ldloc,
        OpCodes.Ldloc_S,
        OpCodes.Ldloc_0,
        OpCodes.Ldloc_1,
        OpCodes.Ldloc_2,
        OpCodes.Ldloc_3,
    };

    private static readonly OpCode[] _branchOpCodes =
    {
        OpCodes.Brtrue,
        OpCodes.Brtrue_S,
        OpCodes.Brfalse,
        OpCodes.Brfalse_S,
    };

    [Fact]
    public void CosmeticsPatches_CheckOnlinePlayFirst()
    {
        GameEnvironment.SkipWithoutModBuild();
        using var module = ModuleDefinition.ReadModule(GameEnvironment.ModDll);

        var patchMethods = module
            .GetTypes()
            .Where(IsInCosmeticsNamespace)
            .SelectMany(type => type.Methods.Where(method => IsPatchMethod(type, method)))
            .ToList();

        Assert.NotEmpty(patchMethods);

        var offenders = patchMethods
            .Where(method => !CallsGuardFirst(method) || !GuardControlsBranch(method))
            .Select(method => $"{method.DeclaringType.FullName}::{method.Name}")
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Patches without a leading IsOnlinePlay check: " + string.Join(", ", offenders)
        );
    }

    private static TypeDefinition OutermostType(TypeDefinition type)
    {
        TypeDefinition current = type;
        while (current.DeclaringType != null)
        {
            current = current.DeclaringType;
        }

        return current;
    }

    private static bool IsInCosmeticsNamespace(TypeDefinition type)
    {
        string ns = OutermostType(type).Namespace;
        return ns == CosmeticsNamespace || ns.StartsWith(CosmeticsNamespace + ".");
    }

    private static bool IsPatchMethod(TypeDefinition type, MethodDefinition method)
    {
        if (
            method.CustomAttributes.Any(attribute =>
                _patchAttributes.Contains(attribute.AttributeType.FullName)
            )
        )
        {
            return true;
        }

        return _patchMethodNames.Contains(method.Name)
            && type.CustomAttributes.Any(attribute =>
                attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch"
            );
    }

    private static bool GuardControlsBranch(MethodDefinition method)
    {
        Instruction current = FirstCall(method)?.Next;
        while (current != null && _skippedOpCodes.Contains(current.OpCode))
        {
            current = current.Next;
        }

        return current != null && _branchOpCodes.Contains(current.OpCode);
    }

    private static Instruction FirstCall(MethodDefinition method) =>
        method.HasBody
            ? method.Body.Instructions.FirstOrDefault(instruction =>
                instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt
            )
            : null;

    private static bool CallsGuardFirst(MethodDefinition method)
    {
        Instruction first = FirstCall(method);
        if (first?.Operand is not MethodReference callee)
        {
            return false;
        }

        return callee.DeclaringType.FullName == GuardType && callee.Name == GuardGetter;
    }
}
