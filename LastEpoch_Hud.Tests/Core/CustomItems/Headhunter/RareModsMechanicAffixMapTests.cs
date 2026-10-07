using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class RareModsMechanicAffixMapTests
{
    private static readonly HeadhunterStatKey[] _ownA = HeadhunterTestData.Mods(1);

    [Fact]
    public void OnKill_MappedMod_AddsMappedRows_NotOwnStats()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB", "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Rare, (100, _ownA)),
            Live()
        );

        Assert.Equal(
            [(BuffActionKind.Add, "HH_FakeB"), (BuffActionKind.Add, "HH_FakeC")],
            actions.Select(action => (action.Kind, action.BuffName))
        );
    }

    [Fact]
    public void OnKill_UnmappedMod_UsesOwnStats()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB", "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Magic, (200, _ownA)),
            Live()
        );

        Assert.Equal(["HH_FakeA"], actions.Select(action => action.BuffName));
    }

    [Fact]
    public void OnKill_MixedMods_EachByOwnRule()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB", "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Rare, (100, _ownA), (200, _ownA)),
            Live()
        );

        Assert.Equal(
            ["HH_FakeB", "HH_FakeC", "HH_FakeA"],
            actions.Select(action => action.BuffName)
        );
    }

    [Fact]
    public void OnKill_MappedEmptyRows_GivesNothing()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Rare, (100, _ownA)),
            Live()
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_StatlessMappedMod_GivesRows()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB", "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Rare, (100, [])),
            Live()
        );

        Assert.Equal(["HH_FakeB", "HH_FakeC"], actions.Select(action => action.BuffName));
    }

    [Fact]
    public void OnKill_TwoModsSameMappedRow_OneStack()
    {
        RareModsMechanic mechanic = Create(
            HeadhunterTestData.Affix(100, "FakeB"),
            HeadhunterTestData.Affix(300, "FakeB")
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(KillKind.Rare, (100, []), (300, [])),
            Live()
        );

        BuffAction action = Assert.Single(actions);
        Assert.Equal(1, action.Stacks);
    }

    [Fact]
    public void OnKill_MappedRow_SecondKill_TwoStacks()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB"));
        KillInfo kill = HeadhunterTestData.Kill(KillKind.Rare, (100, []));

        mechanic.OnKill(kill, Live());
        BuffAction action = Assert.Single(mechanic.OnKill(kill, Live(1)));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(2, action.Stacks);
    }

    [Fact]
    public void OnKill_MappedRow_AtCap_Refresh()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeB"));
        KillInfo kill = HeadhunterTestData.Kill(KillKind.Rare, (100, []));
        for (int i = 0; i < HeadhunterTestData.MaxStacks; i++)
        {
            mechanic.OnKill(kill, Live(1));
        }

        BuffAction action = Assert.Single(mechanic.OnKill(kill, Live(1)));

        Assert.Equal(BuffActionKind.Refresh, action.Kind);
        Assert.Equal(HeadhunterTestData.MaxStacks, action.Stacks);
    }

    [Theory]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    public void OnKill_BossKinds_IgnoreMappedAndOwnStats(KillKind kind)
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            HeadhunterTestData.Kill(kind, (100, HeadhunterTestData.Mods(2))),
            Live()
        );

        Assert.Equal(["HH_FakeA"], actions.Select(action => action.BuffName));
    }

    [Theory]
    [InlineData(1, 10, "HH_FakeB")]
    [InlineData(-1, 2, "HH_FakeA")]
    [InlineData(5, 2, null)]
    public void OnKill_ModRangePastEnd_Clamped(int start, int count, string expected)
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeC"));
        var kill = new KillInfo(
            KillKind.Rare,
            false,
            HeadhunterTestData.Mods(1, 2),
            [new KillMod(200, start, count)]
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(kill, Live());

        Assert.Equal(
            expected == null ? [] : new[] { expected },
            actions.Select(action => action.BuffName)
        );
    }

    [Fact]
    public void OnKill_ModsNull_AllModStatsAreOwnStats()
    {
        RareModsMechanic mechanic = Create(HeadhunterTestData.Affix(100, "FakeC"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 2)),
            Live()
        );

        Assert.Equal(["HH_FakeA", "HH_FakeB"], actions.Select(action => action.BuffName));
    }

    private static RareModsMechanic Create(params HeadhunterAffixEntry[] map)
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.ConfigWithMap(
                map,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Entry("FakeB"),
                HeadhunterTestData.Entry("FakeC")
            )
        );
        return HeadhunterTestData.Mechanic(config);
    }

    private static HashSet<int> Live(params int[] rows)
    {
        return [.. rows];
    }
}
