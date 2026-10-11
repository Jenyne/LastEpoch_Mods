namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Maps names between Il2CppInterop assemblies and cpp2il output.</summary>
internal static class Cpp2IlNames
{
    private const string InteropPrefix = "Il2Cpp";

    public static string FromInteropType(string interopTypeFullName)
    {
        if (interopTypeFullName.StartsWith(InteropPrefix + ".", StringComparison.Ordinal))
        {
            return interopTypeFullName[(InteropPrefix.Length + 1)..];
        }

        if (interopTypeFullName.StartsWith(InteropPrefix, StringComparison.Ordinal))
        {
            return interopTypeFullName[InteropPrefix.Length..];
        }

        return interopTypeFullName;
    }

    public static string ToInteropMethod(string cpp2IlMethodName) =>
        cpp2IlMethodName.Replace('.', '_');
}
