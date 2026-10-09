using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Pause;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

public sealed class HeadhunterTimerFreezeTests
{
    private const float Held = HeadhunterTimerFreeze.HeldSeconds;
    private const float Full = HeadhunterTestData.Duration;
    private static readonly float[] _live = [5.5f, 0f, 3f];

    private readonly HeadhunterResolvedConfig _config = PauseTestData.Config(3);
    private readonly HeadhunterTimerFreeze _freeze = new();

    [Fact]
    public void Request_RunningToPaused_ReturnsPaused()
    {
        Assert.Equal(HeadhunterPauseChange.Paused, _freeze.Request(true));
    }

    [Fact]
    public void Request_PausedToRunning_ReturnsResumed()
    {
        _freeze.Request(true);

        Assert.Equal(HeadhunterPauseChange.Resumed, _freeze.Request(false));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Request_Same_ReturnsNone(bool paused)
    {
        _freeze.Request(paused);

        Assert.Equal(HeadhunterPauseChange.None, _freeze.Request(paused));
    }

    [Fact]
    public void Request_Paused_IsApplyPending()
    {
        _freeze.Request(true);

        Assert.True(_freeze.IsPaused);
        Assert.True(_freeze.IsApplyPending);
    }

    [Fact]
    public void Request_PauseThenResumeBeforeApply_NotPending()
    {
        _freeze.Request(true);
        _freeze.Request(false);

        Assert.False(_freeze.IsApplyPending);
    }

    [Fact]
    public void Apply_Pause_EmitsHeldForLiveRowsOnly()
    {
        _freeze.Request(true);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(_config, _live);

        Assert.Equal(
            [
                PauseTestData.SetRemaining(_config, 0, Held),
                PauseTestData.SetRemaining(_config, 2, Held),
            ],
            actions
        );
        Assert.True(_freeze.IsHolding);
        Assert.False(_freeze.IsApplyPending);
    }

    [Fact]
    public void Apply_Pause_StrayPlaceholder_KeepsFullDuration()
    {
        _freeze.Request(true);
        _freeze.Apply(_config, [Held, 0f, 0f]);
        _freeze.Request(false);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(_config, [Held, 0f, 0f]);

        Assert.Equal([PauseTestData.SetRemaining(_config, 0, Full)], actions);
    }

    [Fact]
    public void Apply_Pause_LiveShorterThanConfig_NoThrow()
    {
        _freeze.Request(true);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(_config, [5.5f]);

        Assert.Equal([PauseTestData.SetRemaining(_config, 0, Held)], actions);
    }

    [Fact]
    public void Apply_Resume_RestoresKeptSecondsExactly()
    {
        Pause();
        _freeze.Request(false);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(_config, [Held, 0f, Held]);

        Assert.Equal(
            [
                PauseTestData.SetRemaining(_config, 0, 5.5f),
                PauseTestData.SetRemaining(_config, 2, 3f),
            ],
            actions
        );
        Assert.False(_freeze.IsHolding);
    }

    [Fact]
    public void ShowFrozen_NotHolding_Untouched()
    {
        float[] shown = [Held, 0f, Held];

        _freeze.ShowFrozen(shown);

        Assert.Equal([Held, 0f, Held], shown);
    }

    [Fact]
    public void ShowFrozen_AfterResume_Untouched()
    {
        Pause();
        _freeze.Request(false);
        _freeze.Apply(_config, [Held, 0f, Held]);
        float[] shown = [Held, 0f, Held];

        _freeze.ShowFrozen(shown);

        Assert.Equal([Held, 0f, Held], shown);
    }

    [Fact]
    public void Apply_Resume_SmallerConfig_NoThrow()
    {
        Pause();
        _freeze.Request(false);
        HeadhunterResolvedConfig small = PauseTestData.Config(2);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(small, [Held, 0f]);

        Assert.Equal([PauseTestData.SetRemaining(small, 0, 5.5f)], actions);
    }

    [Fact]
    public void Hold_Holding_SetRemaining_Unchanged()
    {
        Pause();
        BuffAction set = PauseTestData.SetRemaining(_config, 0, 4f);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [set]);

        Assert.Equal([set], held);
        float[] shown = [Held, 0f, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([5.5f, 0f, 3f], shown);
    }

    [Fact]
    public void Hold_HoldingWithoutApply_SizesRows()
    {
        _freeze.Request(true);
        _freeze.ClearTimers();
        BuffAction add = PauseTestData.Action(_config, 2, BuffActionKind.Add, Full);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [add]);

        Assert.Equal([add with { DurationSeconds = Held }], held);
        float[] shown = [0f, 0f, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([0f, 0f, Full], shown);
    }

    [Fact]
    public void Hold_BiggerConfigThanKept_UsesNewRows()
    {
        HeadhunterResolvedConfig small = PauseTestData.Config(2);
        _freeze.Request(true);
        _freeze.Apply(small, [5f, 4f]);
        BuffAction add = PauseTestData.Action(_config, 2, BuffActionKind.Add, Full);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [add]);

        Assert.Equal([add with { DurationSeconds = Held }], held);
        float[] shown = [0f, 0f, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([0f, 0f, Full], shown);
    }

    [Fact]
    public void Apply_NotPending_ReturnsEmpty()
    {
        Assert.Empty(_freeze.Apply(_config, _live));
    }

    [Fact]
    public void Hold_Holding_AddAndRefresh_FrozenAtFullDuration()
    {
        Pause();
        BuffAction add = PauseTestData.Action(_config, 0, BuffActionKind.Add, Full);
        BuffAction refresh = PauseTestData.Action(_config, 1, BuffActionKind.Refresh, Full);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [add, refresh]);

        Assert.Equal(
            [add with { DurationSeconds = Held }, refresh with { DurationSeconds = Held }],
            held
        );
        float[] shown = [Held, Held, 0f];
        _freeze.ShowFrozen(shown);
        Assert.Equal([Full, Full, 0f], shown);
    }

    [Fact]
    public void Hold_Holding_Remove_DropsRow()
    {
        Pause();
        BuffAction remove = PauseTestData.Action(_config, 0, BuffActionKind.Remove, 0f);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [remove]);

        Assert.Equal([remove], held);
        float[] shown = [Held, 0f, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([Held, 0f, 3f], shown);
    }

    [Fact]
    public void Hold_Holding_UnknownRow_Unchanged()
    {
        Pause();
        BuffAction unknown = new(BuffActionKind.Add, "FakeX", 99, 0f, 0f, Full, 1);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [unknown]);

        Assert.Equal([unknown], held);
    }

    [Fact]
    public void Hold_NotHolding_ReturnsSameInstance()
    {
        IReadOnlyList<BuffAction> actions =
        [
            PauseTestData.Action(_config, 0, BuffActionKind.Add, Full),
        ];

        Assert.Same(actions, _freeze.Hold(_config, actions));
    }

    [Fact]
    public void ShowFrozen_Holding_ShowsKeptNeverHeld()
    {
        Pause();
        float[] shown = [Held, 0f, Held];

        _freeze.ShowFrozen(shown);

        Assert.Equal([5.5f, 0f, 3f], shown);
    }

    [Fact]
    public void ShowFrozen_KeptButDead_StaysZero()
    {
        Pause();
        float[] shown = [0f, 0f, 0f];

        _freeze.ShowFrozen(shown);

        Assert.Equal([0f, 0f, 0f], shown);
    }

    [Fact]
    public void ShowFrozen_RowWithoutKept_Untouched()
    {
        Pause();
        float[] shown = [Held, 4f, Held];

        _freeze.ShowFrozen(shown);

        Assert.Equal(4f, shown[1]);
    }

    [Fact]
    public void ShowFrozen_ShorterThanKept_ShowsFirstRowOnly()
    {
        Pause();
        float[] shown = [Held];

        _freeze.ShowFrozen(shown);

        Assert.Equal([5.5f], shown);
    }

    [Fact]
    public void ShowFrozen_LongerThanKept_LeavesExtraRowsUntouched()
    {
        Pause();
        float[] shown = [Held, 0f, Held, 9f, 9f];

        _freeze.ShowFrozen(shown);

        Assert.Equal([5.5f, 0f, 3f, 9f, 9f], shown);
    }

    [Fact]
    public void ClearTimers_WhilePaused_HoldingNothingNotPending()
    {
        Pause();

        _freeze.ClearTimers();

        Assert.True(_freeze.IsHolding);
        Assert.False(_freeze.IsApplyPending);
    }

    [Fact]
    public void ClearTimers_WhilePaused_NextKillIsHeld()
    {
        Pause();
        _freeze.ClearTimers();
        BuffAction add = PauseTestData.Action(_config, 0, BuffActionKind.Add, Full);

        IReadOnlyList<BuffAction> held = _freeze.Hold(_config, [add]);

        Assert.Equal([add with { DurationSeconds = Held }], held);
    }

    [Fact]
    public void ClearTimers_WhilePaused_DropsKeptSeconds()
    {
        Pause();

        _freeze.ClearTimers();

        float[] shown = [Held, 0f, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([Held, 0f, Held], shown);
    }

    [Fact]
    public void ClearTimers_WhileRunning_NotHolding()
    {
        Pause();
        _freeze.Request(false);

        _freeze.ClearTimers();

        Assert.False(_freeze.IsHolding);
        Assert.False(_freeze.IsApplyPending);
    }

    [Fact]
    public void Apply_BiggerConfigAfterClear_UsesNewRows()
    {
        HeadhunterResolvedConfig small = PauseTestData.Config(2);
        _freeze.Request(true);
        _freeze.Apply(small, [5f, 4f]);
        _freeze.ClearTimers();
        _freeze.Request(false);
        _freeze.ClearTimers();
        _freeze.Request(true);

        IReadOnlyList<BuffAction> actions = _freeze.Apply(_config, [1f, 2f, 3f]);

        Assert.Equal(
            [
                PauseTestData.SetRemaining(_config, 0, Held),
                PauseTestData.SetRemaining(_config, 1, Held),
                PauseTestData.SetRemaining(_config, 2, Held),
            ],
            actions
        );
        float[] shown = [Held, Held, Held];
        _freeze.ShowFrozen(shown);
        Assert.Equal([1f, 2f, 3f], shown);
    }

    private void Pause()
    {
        _freeze.Request(true);
        _freeze.Apply(_config, _live);
    }
}
