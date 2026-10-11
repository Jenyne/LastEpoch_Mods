using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Weaver tree settings have one write path; no legacy Slider hooks.</summary>
public sealed class WeaverWritePathTests
{
    private const string PageType = "LastEpoch_Hud.Scripts.ModUI.Pages.UtilitiesCharacterPage";
    private const string SaveManagerType = "LastEpoch_Hud.Scripts.Save_Manager";
    private const string WovenType = "LastEpoch_Hud.Scripts.Save_Manager/Data/TheWoven";

    private static readonly string[] _wovenFields =
    {
        "Enable_TreePoints",
        "TreePoints",
        "Enable_PointMultiplier",
        "PointMultiplier",
    };

    [Fact]
    public void Slider_IsPatchedOnlyBySliderHook()
    {
        GameEnvironment.SkipWithoutModBuild();
        using var module = ModuleDefinition.ReadModule(GameEnvironment.ModDll);

        PatchDeclaration sliderPatch = Assert.Single(
            PatchScanner.Scan(module),
            patch => patch.Target.FullName == "UnityEngine.UI.Slider"
        );

        Assert.Equal(
            "LastEpoch_Hud.Scripts.ModUI.Settings.SliderHook+SliderSetValuePatch",
            sliderPatch.PatchClass
        );
    }

    [Fact]
    public void WovenTreeSettings_AreWrittenOnlyByCharacterPage()
    {
        GameEnvironment.SkipWithoutModBuild();
        using var module = ModuleDefinition.ReadModule(GameEnvironment.ModDll);

        var writers = module
            .GetTypes()
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody && WritesWovenField(method))
            .Select(method => OutermostType(method.DeclaringType).FullName)
            .ToHashSet();

        Assert.Contains(PageType, writers);
        Assert.Subset(new HashSet<string> { PageType, SaveManagerType }, writers);
    }

    private static bool WritesWovenField(MethodDefinition method)
    {
        return method.Body.Instructions.Any(instruction =>
            instruction.OpCode == OpCodes.Stfld
            && instruction.Operand is FieldReference field
            && field.DeclaringType.FullName == WovenType
            && _wovenFields.Contains(field.Name)
        );
    }

    private static TypeDefinition OutermostType(TypeDefinition type)
    {
        while (type.DeclaringType != null)
        {
            type = type.DeclaringType;
        }
        return type;
    }
}
