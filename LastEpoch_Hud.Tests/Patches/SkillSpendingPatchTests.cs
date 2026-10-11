using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

public sealed class SkillSpendingPatchTests
{
    [Fact]
    public void NodeRequirementBypass_LeavesNativeSkillSpendingUnpatched()
    {
        GameEnvironment.SkipWithoutModBuild();
        using var module = ModuleDefinition.ReadModule(GameEnvironment.ModDll);
        var patches = PatchScanner.Scan(module);

        // Changing the spend boundary can bypass the native point budget and
        // trigger refunds when the temporary capacity is restored afterwards.
        Assert.DoesNotContain(
            patches,
            patch =>
                patch.Target.FullName == "Il2Cpp.LocalTreeData"
                && patch.Method == "tryToSpendSkillPoint"
        );

        var requirementPatch = Assert.Single(
            patches,
            patch =>
                patch.PatchClass
                == "LastEpoch_Hud.Scripts.Mods.Skills.Skills_Nodes_Req+LocalTreeData_FulfilledRequirementExists"
        );
        Assert.Equal("Il2Cpp.LocalTreeData", requirementPatch.Target.FullName);
        Assert.Equal("fulfilledRequirementExists", requirementPatch.Method);
        Assert.Equal(
            new[]
            {
                "Il2Cpp.GlobalTreeData/TreeData",
                "Il2Cpp.GlobalTreeData/NodeData",
                "Il2Cpp.LocalTreeData/TreeData",
                "Il2Cpp.LocalTreeData/NodeData",
            },
            requirementPatch.ArgumentTypes
        );
    }
}
