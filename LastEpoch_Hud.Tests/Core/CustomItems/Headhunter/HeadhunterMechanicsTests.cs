using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterMechanicsTests
{
    [Fact]
    public void Create_ReturnsRareMods_ForRareModsId()
    {
        var problems = new List<HeadhunterConfigProblem>();

        IHeadhunterMechanic mechanic = HeadhunterMechanics.Create(
            ResolvedWithMechanic(HeadhunterMechanics.RareModsId),
            new HeadhunterStackState(0),
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
            problems
        );

        Assert.IsType<RareModsMechanic>(mechanic);
        Assert.Equal(["mechanic"], problems.Select(problem => problem.Path));
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
