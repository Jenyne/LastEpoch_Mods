using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterIconLoadGateTests
{
    private static readonly HeadhunterStatKey _key = new(1, 0);
    private static readonly HeadhunterStatKey _other = new(2, 0);

    [Fact]
    public void Next_NewKey_Start()
    {
        Assert.Equal(
            HeadhunterIconStep.Start,
            new HeadhunterIconLoadGate().Next(_key, HeadhunterIconLoadStatus.None)
        );
    }

    [Fact]
    public void Next_Loading_Wait()
    {
        Assert.Equal(
            HeadhunterIconStep.Wait,
            new HeadhunterIconLoadGate().Next(_key, HeadhunterIconLoadStatus.Loading)
        );
    }

    [Fact]
    public void Next_Loaded_Use()
    {
        Assert.Equal(
            HeadhunterIconStep.Use,
            new HeadhunterIconLoadGate().Next(_key, HeadhunterIconLoadStatus.Loaded)
        );
    }

    [Fact]
    public void Next_Failed_DropThenSkip()
    {
        var gate = new HeadhunterIconLoadGate();

        HeadhunterIconStep drop = gate.Next(_key, HeadhunterIconLoadStatus.Failed);
        HeadhunterIconStep first = gate.Next(_key, HeadhunterIconLoadStatus.None);
        HeadhunterIconStep second = gate.Next(_key, HeadhunterIconLoadStatus.None);

        Assert.Equal(HeadhunterIconStep.Drop, drop);
        Assert.Equal(HeadhunterIconStep.Skip, first);
        Assert.Equal(HeadhunterIconStep.Skip, second);
    }

    [Fact]
    public void Next_FailedKey_OtherKeyStillStarts()
    {
        var gate = new HeadhunterIconLoadGate();
        gate.Next(_key, HeadhunterIconLoadStatus.Failed);

        Assert.Equal(HeadhunterIconStep.Start, gate.Next(_other, HeadhunterIconLoadStatus.None));
    }

    [Fact]
    public void MarkFailed_ThenNone_Skip()
    {
        var gate = new HeadhunterIconLoadGate();
        gate.MarkFailed(_key);

        Assert.Equal(HeadhunterIconStep.Skip, gate.Next(_key, HeadhunterIconLoadStatus.None));
    }

    [Fact]
    public void AllowRetry_FailedKeyStartsOnce()
    {
        var gate = new HeadhunterIconLoadGate();
        gate.Next(_key, HeadhunterIconLoadStatus.Failed);
        gate.AllowRetry();

        HeadhunterIconStep retry = gate.Next(_key, HeadhunterIconLoadStatus.None);
        HeadhunterIconStep drop = gate.Next(_key, HeadhunterIconLoadStatus.Failed);
        HeadhunterIconStep after = gate.Next(_key, HeadhunterIconLoadStatus.None);

        Assert.Equal(HeadhunterIconStep.Start, retry);
        Assert.Equal(HeadhunterIconStep.Drop, drop);
        Assert.Equal(HeadhunterIconStep.Skip, after);
    }
}
