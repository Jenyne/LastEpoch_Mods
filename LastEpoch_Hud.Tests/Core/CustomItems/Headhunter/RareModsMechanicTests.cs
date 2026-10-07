using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class RareModsMechanicTests
{
    private static readonly HeadhunterStatEntry[] _threeStats =
    [
        HeadhunterTestData.Entry("FakeA", 5f, 10f),
        HeadhunterTestData.Entry("FakeB"),
        HeadhunterTestData.Entry("FakeC"),
    ];

    [Fact]
    public void OnKill_ReturnsNothing_ForNormal()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Normal, false, [1]),
            Active(1)
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_MagicKill_ActsLikeRare()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        var magic = mechanic
            .OnKill(new KillInfo(KillKind.Magic, false, [1, 2]), Active(1))
            .Select(action => (action.Kind, action.StatId))
            .ToList();
        var rare = mechanic
            .OnKill(new KillInfo(KillKind.Rare, false, [1, 2]), Active(1))
            .Select(action => (action.Kind, action.StatId))
            .ToList();

        Assert.NotEmpty(magic);
        Assert.Equal(rare, magic);
    }

    [Theory]
    [InlineData(KillKind.Rare)]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    [InlineData(KillKind.Magic)]
    public void OnKill_ReturnsNothing_WhenKindTriggerOff(KillKind kind)
    {
        RareModsMechanic mechanic = CreateMechanic(TriggersWithout(kind));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(kind, false, [1]),
            Active(1)
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_ReturnsNothing_ForMinionKill_WhenMinionTriggerOff()
    {
        RareModsMechanic mechanic = CreateMechanic(
            new HeadhunterTriggers(true, true, true, false, true)
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, true, [1]),
            Active(1)
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_TreatsMinionKillLikePlayerKill_WhenMinionTriggerOn()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        BuffAction[] player = mechanic
            .OnKill(new KillInfo(KillKind.Rare, false, [1, 2]), Active(1))
            .ToArray();

        BuffAction[] minion = mechanic
            .OnKill(new KillInfo(KillKind.Rare, true, [1, 2]), Active(1))
            .ToArray();

        Assert.NotEmpty(player);
        Assert.Equal(player, minion);
    }

    [Fact]
    public void OnKill_AddsKnownStats_SkipsUnknown()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [1, 99, 2]),
            Active()
        );

        Assert.Equal(
            [(BuffActionKind.Add, 1), (BuffActionKind.Add, 2)],
            actions.Select(action => (action.Kind, action.StatId))
        );
    }

    [Fact]
    public void OnKill_RefreshesStat_WhenAlreadyActive()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [1]),
            Active(1)
        );

        BuffAction action = Assert.Single(actions);
        Assert.Equal(BuffActionKind.Refresh, action.Kind);
        Assert.Equal(1, action.StatId);
    }

    [Fact]
    public void OnKill_EmitsOneAction_ForDuplicateIds()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [1, 1]),
            Active()
        );

        Assert.Equal(1, Assert.Single(actions).StatId);
    }

    [Fact]
    public void OnKill_SkipsDisabledStat()
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Disabled("FakeB")
            )
        );
        var mechanic = new RareModsMechanic(config);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [2]),
            Active()
        );

        Assert.Empty(actions);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnKill_RefreshesAllActive_WhenBossHasNoMods(bool useNull)
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        IReadOnlyList<int> mods = useNull ? null : [];

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Boss, false, mods),
            Active(3, 1)
        );

        Assert.Equal(
            [(BuffActionKind.Refresh, 1), (BuffActionKind.Refresh, 3)],
            actions.Select(action => (action.Kind, action.StatId))
        );
    }

    [Fact]
    public void OnKill_RefreshesOtherActive_AfterKillStats()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [2]),
            Active(1, 2, 99)
        );

        Assert.Equal(
            [(BuffActionKind.Refresh, 2), (BuffActionKind.Refresh, 1)],
            actions.Select(action => (action.Kind, action.StatId))
        );
    }

    [Fact]
    public void OnKill_RefreshesOnce_WhenTwoNamesShareId()
    {
        var sharedIds = new Dictionary<string, int> { ["FakeA"] = 1, ["FakeB"] = 1 };
        HeadhunterResolvedConfig config = HeadhunterConfigResolver.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Entry("FakeB")
            ),
            sharedIds,
            new List<HeadhunterConfigProblem>()
        );
        var mechanic = new RareModsMechanic(config);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Boss, false, []),
            Active(1)
        );

        BuffAction action = Assert.Single(actions);
        Assert.Equal(BuffActionKind.Refresh, action.Kind);
        Assert.Equal("HH_FakeA", action.BuffName);
    }

    [Fact]
    public void OnKill_ActionCarriesStatValuesAndDuration()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [1]),
            Active()
        );

        Assert.Equal(
            new BuffAction(
                BuffActionKind.Add,
                "HH_FakeA",
                1,
                5f,
                0.1f,
                HeadhunterTestData.Duration
            ),
            Assert.Single(actions)
        );
    }

    [Fact]
    public void OnKill_ClearsPreviousResult_OnNextCall()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        mechanic.OnKill(new KillInfo(KillKind.Rare, false, [1]), Active());

        IReadOnlyList<BuffAction> second = mechanic.OnKill(
            new KillInfo(KillKind.Normal, false, []),
            Active()
        );

        Assert.Empty(second);
    }

    [Fact]
    public void OnKill_StartsFresh_OnRepeatedFiringKill()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        mechanic.OnKill(new KillInfo(KillKind.Rare, false, [1]), Active());

        IReadOnlyList<BuffAction> second = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [1]),
            Active()
        );

        Assert.Equal(
            [(BuffActionKind.Add, 1)],
            second.Select(action => (action.Kind, action.StatId))
        );
    }

    private static RareModsMechanic CreateMechanic(HeadhunterTriggers triggers)
    {
        return new RareModsMechanic(
            HeadhunterTestData.Resolve(HeadhunterTestData.Config(triggers, _threeStats))
        );
    }

    private static HashSet<int> Active(params int[] ids)
    {
        return [.. ids];
    }

    private static HeadhunterTriggers TriggersWithout(KillKind kind)
    {
        return kind switch
        {
            KillKind.Rare => new HeadhunterTriggers(false, true, true, true, true),
            KillKind.Boss => new HeadhunterTriggers(true, false, true, true, true),
            KillKind.Miniboss => new HeadhunterTriggers(true, true, false, true, true),
            _ => new HeadhunterTriggers(true, true, true, true, false),
        };
    }
}
