using LastEpoch_Hud.Tests.Support;
using Mono.Cecil;

namespace LastEpoch_Hud.Tests.Patches;

public class ProphecyPatchTests
{
    [Fact]
    public void Trigger_UsesVerifiedPrimitiveAndReferenceSignature()
    {
        GameEnvironment.SkipWithoutGame();
        using var game = ModuleDefinition.ReadModule(
            Path.Combine(GameEnvironment.Il2CppDir, "Il2CppLE.dll")
        );
        var slot = game.Types.Single(t => t.FullName == "Il2CppLE.Factions.ProphecySlot");
        var trigger = slot.Methods.Single(m => m.Name == "TryTriggerReward");
        Assert.False(trigger.IsStatic);
        Assert.Equal("System.Boolean", trigger.ReturnType.FullName);
        Assert.Equal(
            new[]
            {
                "Il2CppLE.Factions.ProphecyTargetType",
                "System.Boolean",
                "Il2Cpp.Actor",
                "System.Int32&",
            },
            trigger.Parameters.Select(p => p.ParameterType.FullName)
        );
        Assert.True(trigger.Parameters[3].IsOut);
        Assert.DoesNotContain(
            trigger.Parameters,
            p => p.ParameterType.FullName.Contains("Nullable")
        );
        var reward = game.Types.Single(t => t.FullName == "Il2CppLE.Factions.ProphecySlotReward");
        Assert.Equal(
            "System.Int32",
            reward.Properties.Single(p => p.Name == "itemsDropped").PropertyType.FullName
        );
    }

    [Fact]
    public void BuiltProphecyPatches_LeaveNullableSpawnMethodUntouched()
    {
        GameEnvironment.SkipWithoutGame();
        GameEnvironment.SkipWithoutModBuild();
        using var mod = ModuleDefinition.ReadModule(GameEnvironment.ModDll);
        var patches = PatchScanner.Scan(mod).Where(p => p.Id.Contains("ProphecyRewards")).ToArray();
        Assert.Single(patches);
        Assert.Equal("TryTriggerReward", patches[0].Method);
    }
}
