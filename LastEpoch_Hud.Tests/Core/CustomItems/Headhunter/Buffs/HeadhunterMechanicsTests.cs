using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterMechanicsTests
{
    [Fact]
    public void Create_ReturnsRareMods_ForRareModsId()
    {
        var problems = new List<HeadhunterConfigProblem>();

        IHeadhunterMechanic mechanic = HeadhunterMechanics.Create(
            ResolvedWithMechanic(HeadhunterMechanics.RareModsId),
            new HeadhunterStackState(0),
            new FakeHeadhunterRandom(0),
            problems
        );

        Assert.IsType<RareModsMechanic>(mechanic);
        Assert.Empty(problems);
    }

    [Fact]
    public void Create_FallsBackToRareModsWithProblem_ForUnknownId()
    {
        var problems = new List<HeadhunterConfigProblem>();

        IHeadhunterMechanic mechanic = HeadhunterMechanics.Create(
            ResolvedWithMechanic("fake_id"),
            new HeadhunterStackState(0),
            new FakeHeadhunterRandom(0),
            problems
        );

        Assert.IsType<RareModsMechanic>(mechanic);
        Assert.Equal(["mechanic"], problems.Select(problem => problem.Path));
    }

    [Fact]
    public void Create_MechanicReset_ClearsPassedStacks()
    {
        HeadhunterResolvedConfig table = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA", 5f, 10f)
            )
        );
        HeadhunterResolvedConfig resolved = new(
            HeadhunterMechanics.RareModsId,
            HeadhunterTestData.Duration,
            HeadhunterTestData.MaxStacks,
            HeadhunterTestData.AllTriggers,
            table.Stats
        );
        var state = new HeadhunterStackState(table.Stats.Count);
        IHeadhunterMechanic mechanic = HeadhunterMechanics.Create(
            resolved,
            state,
            new FakeHeadhunterRandom(0),
            new List<HeadhunterConfigProblem>()
        );
        mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            new HashSet<int>()
        );
        Assert.True(state.Total > 0);

        mechanic.Reset();

        Assert.Equal(0, state.Total);
    }

    private static HeadhunterResolvedConfig ResolvedWithMechanic(string mechanic)
    {
        return new HeadhunterResolvedConfig(
            mechanic,
            HeadhunterTestData.Duration,
            HeadhunterTestData.MaxStacks,
            HeadhunterTestData.AllTriggers,
            []
        );
    }
}
