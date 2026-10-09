using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

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
            new KillInfo(KillKind.Normal, false, HeadhunterTestData.Mods(1)),
            Active(1)
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_MagicKill_ActsLikeRare()
    {
        BuffAction[] magic = CreateMechanic(HeadhunterTestData.AllTriggers)
            .OnKill(new KillInfo(KillKind.Magic, false, HeadhunterTestData.Mods(1, 2)), Active(1))
            .ToArray();
        BuffAction[] rare = CreateMechanic(HeadhunterTestData.AllTriggers)
            .OnKill(new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 2)), Active(1))
            .ToArray();

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
            new KillInfo(kind, false, HeadhunterTestData.Mods(1)),
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
            new KillInfo(KillKind.Rare, true, HeadhunterTestData.Mods(1)),
            Active(1)
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_TreatsMinionKillLikePlayerKill_WhenMinionTriggerOn()
    {
        BuffAction[] player = CreateMechanic(HeadhunterTestData.AllTriggers)
            .OnKill(new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 2)), Active(1))
            .ToArray();

        BuffAction[] minion = CreateMechanic(HeadhunterTestData.AllTriggers)
            .OnKill(new KillInfo(KillKind.Rare, true, HeadhunterTestData.Mods(1, 2)), Active(1))
            .ToArray();

        Assert.NotEmpty(player);
        Assert.Equal(player, minion);
    }

    [Fact]
    public void OnKill_AddsKnownStats_SkipsUnknown()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 99, 2)),
            Active()
        );

        Assert.Equal(
            [(BuffActionKind.Add, 1), (BuffActionKind.Add, 2)],
            actions.Select(action => (action.Kind, action.StatId))
        );
    }

    [Fact]
    public void OnKill_LiveStatInKill_AddsStack()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            Active(0)
        );

        BuffAction action = Assert.Single(actions);
        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(1, action.StatId);
        Assert.Equal(2, action.Stacks);
    }

    [Fact]
    public void OnKill_EmitsOneAction_ForDuplicateIds()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 1)),
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
        RareModsMechanic mechanic = HeadhunterTestData.Mechanic(config);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(2)),
            Active()
        );

        Assert.Empty(actions);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnKill_RefreshesAllActive_WhenKillHasNoMods(bool useNull)
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        IReadOnlyList<HeadhunterStatKey> mods = useNull ? null : [];

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, mods),
            Active(2, 0)
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
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(2)),
            Active(0, 99)
        );

        Assert.Equal(
            [(BuffActionKind.Add, 2), (BuffActionKind.Refresh, 1)],
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
            HeadhunterTestData.TagIds,
            new List<HeadhunterConfigProblem>()
        );
        RareModsMechanic mechanic = HeadhunterTestData.Mechanic(config);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, []),
            Active(0)
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
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            Active()
        );

        Assert.Equal(
            new BuffAction(
                BuffActionKind.Add,
                "HH_FakeA",
                1,
                5f,
                0.1f,
                HeadhunterTestData.Duration,
                1
            ),
            Assert.Single(actions)
        );
    }

    [Fact]
    public void OnKill_ClearsPreviousResult_OnNextCall()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        mechanic.OnKill(new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)), Active());

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
        mechanic.OnKill(new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)), Active());

        IReadOnlyList<BuffAction> second = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            Active()
        );

        Assert.Equal(
            [(BuffActionKind.Add, 1)],
            second.Select(action => (action.Kind, action.StatId))
        );
    }

    [Fact]
    public void OnKill_FirstKill_AddsOneStackBaseValue()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        BuffAction action = Assert.Single(Fire(mechanic, Active(), 1));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(1, action.Stacks);
        Assert.Equal(5f, action.Added);
        Assert.Equal(0.1f, action.Increased, 5);
        Assert.Equal(HeadhunterTestData.Duration, action.DurationSeconds);
    }

    [Fact]
    public void OnKill_SecondKill_AddsDoubleValueFullDuration()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1);

        BuffAction action = Assert.Single(Fire(mechanic, Active(0), 1));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(2, action.Stacks);
        Assert.Equal(10f, action.Added);
        Assert.Equal(0.2f, action.Increased, 5);
        Assert.Equal(HeadhunterTestData.Duration, action.DurationSeconds);
    }

    [Fact]
    public void OnKill_AtCap_RefreshesCappedValue()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1);
        Fire(mechanic, Active(0), 1);
        Fire(mechanic, Active(0), 1);

        BuffAction action = Assert.Single(Fire(mechanic, Active(0), 1));

        Assert.Equal(BuffActionKind.Refresh, action.Kind);
        Assert.Equal(HeadhunterTestData.MaxStacks, action.Stacks);
        Assert.Equal(15f, action.Added);
        Assert.Equal(0.3f, action.Increased, 5);
    }

    [Fact]
    public void OnKill_RowExpired_RestartsAtOne()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1);
        Fire(mechanic, Active(0), 1);

        BuffAction action = Assert.Single(Fire(mechanic, Active(), 1));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(1, action.Stacks);
    }

    [Fact]
    public void OnKill_LiveRowLostState_CountsAsOne()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);

        BuffAction action = Assert.Single(Fire(mechanic, Active(0), 1));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(2, action.Stacks);
    }

    [Fact]
    public void OnKill_RefreshOthers_KeepsStacksAndValue()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1);
        Fire(mechanic, Active(0), 1);

        BuffAction refresh = Fire(mechanic, Active(0), 2).Single(a => a.StatId == 1);

        Assert.Equal(BuffActionKind.Refresh, refresh.Kind);
        Assert.Equal(2, refresh.Stacks);
        Assert.Equal(10f, refresh.Added);
        Assert.Equal(0.2f, refresh.Increased, 5);
    }

    [Fact]
    public void OnKill_TotalStacksRise_OtherLiveStaysRefreshWithOwnValue()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1);
        Fire(mechanic, Active(0), 1);
        Fire(mechanic, Active(0), 1);

        BuffAction[] actions = Fire(mechanic, Active(0), 2);

        BuffAction b = Assert.Single(actions, action => action.StatId == 2);
        BuffAction a = Assert.Single(actions, action => action.StatId == 1);
        Assert.Equal(BuffActionKind.Add, b.Kind);
        Assert.Equal(1, b.Stacks);
        Assert.Equal(BuffActionKind.Refresh, a.Kind);
        Assert.Equal(3, a.Stacks);
        Assert.Equal(15f, a.Added);
        Assert.Equal(0.3f, a.Increased, 5);
    }

    [Fact]
    public void OnKill_SameStatTwice_OneStack()
    {
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers);
        Fire(mechanic, Active(), 1, 1);

        BuffAction action = Assert.Single(Fire(mechanic, Active(0), 1, 1));

        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(2, action.Stacks);
    }

    [Fact]
    public void OnKill_NonFiring_KeepsCounts()
    {
        var state = new HeadhunterStackState(3);
        RareModsMechanic mechanic = HeadhunterTestData.Mechanic(
            HeadhunterTestData.Resolve(
                HeadhunterTestData.Config(HeadhunterTestData.AllTriggers, _threeStats)
            ),
            state
        );
        Fire(mechanic, Active(), 1);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Normal, false, HeadhunterTestData.Mods(1)),
            Active()
        );

        Assert.Empty(actions);
        Assert.Equal(1, state.Get(0));
    }

    [Theory]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    public void OnKill_Boss_AddsPickedInactiveRow(KillKind kind)
    {
        var random = new FakeHeadhunterRandom(1);
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers, random);

        BuffAction[] actions = mechanic.OnKill(new KillInfo(kind, false, []), Active(0)).ToArray();

        Assert.Equal(
            [(BuffActionKind.Add, 3), (BuffActionKind.Refresh, 1)],
            actions.Select(action => (action.Kind, action.StatId))
        );
        Assert.Equal(1, actions[0].Stacks);
        Assert.Equal(2, random.LastMax);
    }

    [Fact]
    public void OnKill_Boss_AddCarriesBaseValueAndDuration()
    {
        RareModsMechanic mechanic = CreateMechanic(
            HeadhunterTestData.AllTriggers,
            new FakeHeadhunterRandom(0)
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Boss, false, []),
            Active()
        );

        Assert.Equal(
            new BuffAction(
                BuffActionKind.Add,
                "HH_FakeA",
                1,
                5f,
                0.1f,
                HeadhunterTestData.Duration,
                1
            ),
            Assert.Single(actions)
        );
    }

    [Theory]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    public void OnKill_Boss_IgnoresOwnMods(KillKind kind)
    {
        RareModsMechanic mechanic = CreateMechanic(
            HeadhunterTestData.AllTriggers,
            new FakeHeadhunterRandom(0)
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(kind, false, HeadhunterTestData.Mods(3)),
            Active()
        );

        Assert.Equal(1, Assert.Single(actions).StatId);
    }

    [Theory]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    public void OnKill_Boss_AllLive_RefreshesOnly(KillKind kind)
    {
        var random = new FakeHeadhunterRandom(0);
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers, random);

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(kind, false, HeadhunterTestData.Mods(1, 2)),
            Active(0, 1, 2)
        );

        Assert.Equal(
            [(BuffActionKind.Refresh, 1), (BuffActionKind.Refresh, 2), (BuffActionKind.Refresh, 3)],
            actions.Select(action => (action.Kind, action.StatId))
        );
        Assert.All(actions, action => Assert.Equal(1, action.Stacks));
        Assert.Equal(0, random.Calls);
    }

    [Theory]
    [InlineData(KillKind.Boss)]
    [InlineData(KillKind.Miniboss)]
    public void OnKill_Boss_EmptyTable_Nothing(KillKind kind)
    {
        var random = new FakeHeadhunterRandom(0);
        RareModsMechanic mechanic = HeadhunterTestData.Mechanic(
            HeadhunterTestData.Resolve(HeadhunterTestData.Config(HeadhunterTestData.AllTriggers)),
            new HeadhunterStackState(0),
            random
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(kind, false, HeadhunterTestData.Mods(1)),
            Active()
        );

        Assert.Empty(actions);
        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void OnKill_MinionBossKill_SameAsPlayer()
    {
        BuffAction[] player = CreateMechanic(
                HeadhunterTestData.AllTriggers,
                new FakeHeadhunterRandom(1)
            )
            .OnKill(new KillInfo(KillKind.Boss, false, []), Active(0))
            .ToArray();

        BuffAction[] minion = CreateMechanic(
                HeadhunterTestData.AllTriggers,
                new FakeHeadhunterRandom(1)
            )
            .OnKill(new KillInfo(KillKind.Boss, true, []), Active(0))
            .ToArray();

        Assert.NotEmpty(player);
        Assert.Equal(player, minion);
    }

    [Theory]
    [InlineData(KillKind.Rare)]
    [InlineData(KillKind.Magic)]
    public void OnKill_RareAndMagic_DoNotUseRandom(KillKind kind)
    {
        var random = new FakeHeadhunterRandom(0);
        RareModsMechanic mechanic = CreateMechanic(HeadhunterTestData.AllTriggers, random);

        mechanic.OnKill(new KillInfo(kind, false, HeadhunterTestData.Mods(1)), Active(1));

        Assert.Equal(0, random.Calls);
    }

    [Fact]
    public void OnKill_TaggedModStat_AddsTaggedRow()
    {
        RareModsMechanic mechanic = TaggedMechanic(
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Tagged("FakeA", "FakeTag")
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [new HeadhunterStatKey(1, 8)]),
            Active()
        );

        BuffAction action = Assert.Single(actions);
        Assert.Equal(BuffActionKind.Add, action.Kind);
        Assert.Equal(8, action.Tags);
    }

    [Fact]
    public void OnKill_UntaggedModStat_SkipsTaggedRow()
    {
        RareModsMechanic mechanic = TaggedMechanic(HeadhunterTestData.Tagged("FakeA", "FakeTag"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [new HeadhunterStatKey(1, 0)]),
            Active()
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_TaggedModStat_SkipsUntaggedRow()
    {
        RareModsMechanic mechanic = TaggedMechanic(HeadhunterTestData.Entry("FakeA"));

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(KillKind.Rare, false, [new HeadhunterStatKey(1, 8)]),
            Active()
        );

        Assert.Empty(actions);
    }

    [Fact]
    public void OnKill_SameStatTaggedAndUntaggedMods_AddsBothRows()
    {
        RareModsMechanic mechanic = TaggedMechanic(
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Tagged("FakeA", "FakeTag")
        );

        IReadOnlyList<BuffAction> actions = mechanic.OnKill(
            new KillInfo(
                KillKind.Rare,
                false,
                [new HeadhunterStatKey(1, 0), new HeadhunterStatKey(1, 8)]
            ),
            Active()
        );

        Assert.Equal(["HH_FakeA", "HH_FakeA_FakeTag"], actions.Select(action => action.BuffName));
        Assert.Equal([0, 8], actions.Select(action => action.Tags));
    }

    [Fact]
    public void Reset_ClearsAllStacks()
    {
        var state = new HeadhunterStackState(3);
        RareModsMechanic mechanic = HeadhunterTestData.Mechanic(
            HeadhunterTestData.Resolve(
                HeadhunterTestData.Config(HeadhunterTestData.AllTriggers, _threeStats)
            ),
            state
        );
        Fire(mechanic, Active(), 1, 2, 3);
        Fire(mechanic, Active(0, 1, 2), 1);
        Assert.Equal([2, 1, 1], Counts(state));

        mechanic.Reset();

        Assert.Equal([0, 0, 0], Counts(state));
    }

    private static int[] Counts(HeadhunterStackState state)
    {
        return [state.Get(0), state.Get(1), state.Get(2)];
    }

    private static BuffAction[] Fire(
        RareModsMechanic mechanic,
        HashSet<int> liveRows,
        params int[] statIds
    )
    {
        return mechanic
            .OnKill(new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(statIds)), liveRows)
            .ToArray();
    }

    private static RareModsMechanic CreateMechanic(HeadhunterTriggers triggers)
    {
        return CreateMechanic(triggers, new FakeHeadhunterRandom(0));
    }

    private static RareModsMechanic CreateMechanic(
        HeadhunterTriggers triggers,
        FakeHeadhunterRandom random
    )
    {
        HeadhunterResolvedConfig config = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(triggers, _threeStats)
        );
        return HeadhunterTestData.Mechanic(
            config,
            new HeadhunterStackState(config.Stats.Count),
            random
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

    private static RareModsMechanic TaggedMechanic(params HeadhunterStatEntry[] stats)
    {
        return HeadhunterTestData.Mechanic(
            HeadhunterTestData.Resolve(
                HeadhunterTestData.Config(HeadhunterTestData.AllTriggers, stats)
            )
        );
    }
}
