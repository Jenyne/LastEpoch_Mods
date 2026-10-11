using System.Globalization;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Method name to GameAssembly.dll file offsets, read from cpp2il output.</summary>
internal sealed class Cpp2IlAddressIndex
{
    private const string AddressAttribute = "AddressAttribute";
    private const string OffsetField = "Offset";

    private readonly Dictionary<string, List<long>> _offsets;

    private Cpp2IlAddressIndex(Dictionary<string, List<long>> offsets) => _offsets = offsets;

    public static Cpp2IlAddressIndex Load(string cpp2IlOutDir)
    {
        var offsets = new Dictionary<string, List<long>>();
        foreach (string dll in Directory.EnumerateFiles(cpp2IlOutDir, "*.dll"))
        {
            using var module = ModuleDefinition.ReadModule(dll);
            foreach (TypeDefinition type in module.GetTypes())
            {
                AddMethods(offsets, type);
            }
        }
        return new Cpp2IlAddressIndex(offsets);
    }

    public bool TryGetOffsets(
        string interopTypeFullName,
        string method,
        out IReadOnlyList<long> fileOffsets
    )
    {
        string key = Key(Cpp2IlNames.FromInteropType(interopTypeFullName), method);
        bool found = _offsets.TryGetValue(key, out List<long> list);
        fileOffsets = list;
        return found;
    }

    private static void AddMethods(Dictionary<string, List<long>> offsets, TypeDefinition type)
    {
        foreach (MethodDefinition method in type.Methods)
        {
            string key = Key(type.FullName, Cpp2IlNames.ToInteropMethod(method.Name));
            if (!offsets.TryGetValue(key, out List<long> list))
            {
                offsets[key] = list = new List<long>();
            }

            list.AddRange(OffsetsOf(method));
        }
    }

    private static IEnumerable<long> OffsetsOf(MethodDefinition method) =>
        method
            .CustomAttributes.Where(a => a.AttributeType.Name == AddressAttribute)
            .SelectMany(a => a.Fields)
            .Where(f => f.Name == OffsetField && f.Argument.Value is string)
            .Select(f =>
                long.Parse(
                    ((string)f.Argument.Value).Replace("0x", ""),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                )
            );

    private static string Key(string type, string method) => type + "::" + method;
}
