using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterStackStateTests
{
    [Fact]
    public void TryAdd_CountsUpToCap()
    {
        var state = new HeadhunterStackState(2);

        Assert.True(state.TryAdd(0, 3));
        Assert.True(state.TryAdd(0, 3));
        Assert.True(state.TryAdd(0, 3));
        Assert.False(state.TryAdd(0, 3));
        Assert.Equal(3, state.Get(0));
    }

    [Fact]
    public void Sync_DropsRowsNotLive()
    {
        var state = new HeadhunterStackState(3);
        state.TryAdd(0, 5);
        state.TryAdd(1, 5);
        state.TryAdd(1, 5);

        state.Sync(new HashSet<int> { 1 });

        Assert.Equal(0, state.Get(0));
        Assert.Equal(2, state.Get(1));
    }

    [Fact]
    public void Sync_LiftsLiveZeroToOne()
    {
        var state = new HeadhunterStackState(3);

        state.Sync(new HashSet<int> { 2 });

        Assert.Equal(1, state.Get(2));
        Assert.Equal(0, state.Get(0));
    }

    [Fact]
    public void Sync_KeepsLiveCount()
    {
        var state = new HeadhunterStackState(2);
        state.TryAdd(0, 5);
        state.TryAdd(0, 5);
        state.TryAdd(0, 5);

        state.Sync(new HashSet<int> { 0 });

        Assert.Equal(3, state.Get(0));
    }

    [Fact]
    public void Sync_IgnoresOutOfRangeRows()
    {
        var state = new HeadhunterStackState(2);

        state.Sync(new HashSet<int> { 0, 99 });

        Assert.Equal(1, state.Get(0));
        Assert.Equal(1, state.Total);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void Get_OutOfRange_Zero(int row)
    {
        var state = new HeadhunterStackState(2);

        Assert.Equal(0, state.Get(row));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void TryAdd_OutOfRange_False(int row)
    {
        var state = new HeadhunterStackState(2);

        Assert.False(state.TryAdd(row, 3));
    }

    [Fact]
    public void Total_SumsRows()
    {
        var state = new HeadhunterStackState(3);
        state.TryAdd(0, 5);
        state.TryAdd(0, 5);
        state.TryAdd(2, 5);

        Assert.Equal(3, state.Total);
    }

    [Fact]
    public void Reset_ZerosAll()
    {
        var state = new HeadhunterStackState(2);
        state.TryAdd(0, 5);
        state.TryAdd(1, 5);

        state.Reset();

        Assert.Equal(0, state.Get(0));
        Assert.Equal(0, state.Get(1));
        Assert.Equal(0, state.Total);
    }
}
