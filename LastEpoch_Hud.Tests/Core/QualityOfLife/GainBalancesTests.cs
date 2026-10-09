using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class GainBalancesTests
{
    [Fact]
    public void ExistingWalletIsNotIncomeAndMissingNativeEventsAreRecovered()
    {
        var session = new SessionGains();
        var balances = new GainBalances(session);
        balances.Observe(1, 100, GainCurrency.Favour, 500, true);
        balances.Observe(1, 100, GainCurrency.Favour, 550, true);
        Assert.Equal(50, session.Total(GainCurrency.Favour));
    }

    [Fact]
    public void HookAndPollingDoNotDoubleCountAndSpendingDoesNotSubtract()
    {
        var session = new SessionGains();
        var balances = new GainBalances(session);
        balances.Observe(1, 100, GainCurrency.Favour, 500, true);
        balances.Observe(1, 100, GainCurrency.Favour, 520, true);
        // Event prefix catches any pending wallet change, then postfix observes its own gain.
        balances.Observe(1, 100, GainCurrency.Favour, 530, true);
        balances.Observe(1, 100, GainCurrency.Favour, 560, true);
        balances.Observe(1, 100, GainCurrency.Favour, 560, true);
        balances.Observe(1, 100, GainCurrency.Favour, 100, true);
        balances.Observe(1, 100, GainCurrency.Favour, 110, true);
        Assert.Equal(70, session.Total(GainCurrency.Favour));
    }

    [Fact]
    public void ManualGrantPauseAndLoadingDoNotBecomeDeferredIncome()
    {
        var session = new SessionGains();
        var balances = new GainBalances(session);
        balances.Observe(1, 100, GainCurrency.Favour, 100, true);
        balances.Observe(1, 100, GainCurrency.Favour, 10000, false);
        session.Paused = true;
        balances.Observe(1, 100, GainCurrency.Favour, 10100, true);
        session.Paused = false;
        balances.Observe(1, 100, GainCurrency.Favour, 10105, true);
        Assert.Equal(5, session.Total(GainCurrency.Favour));
    }

    [Fact]
    public void ReplacementFactionAndSessionResetReseedTheWallet()
    {
        var session = new SessionGains();
        var balances = new GainBalances(session);
        balances.Observe(1, 100, GainCurrency.Favour, 100, true);
        balances.Observe(1, 200, GainCurrency.Favour, 1000, true);
        balances.Observe(1, 200, GainCurrency.Favour, 1005, true);
        Assert.Equal(5, session.Total(GainCurrency.Favour));
        session.Reset();
        balances.Clear();
        balances.Observe(1, 200, GainCurrency.Favour, 1010, true);
        balances.Observe(1, 200, GainCurrency.Favour, 1017, true);
        Assert.Equal(7, session.Total(GainCurrency.Favour));
    }

    [Fact]
    public void FactionsRemainSeparateAndInvalidOrRemovedWalletReseeds()
    {
        var session = new SessionGains();
        var balances = new GainBalances(session);
        balances.Observe(1, 100, GainCurrency.Favour, 100, true);
        balances.Observe(2, 200, GainCurrency.MemoryAmber, 100, true);
        balances.Observe(1, 100, GainCurrency.Favour, 115, true);
        balances.Observe(2, 200, GainCurrency.MemoryAmber, 125, true);
        balances.Observe(1, 100, GainCurrency.Favour, -1, true);
        balances.Observe(1, 100, GainCurrency.Favour, 1000, true);
        balances.Forget(2);
        balances.Observe(2, 200, GainCurrency.MemoryAmber, 1000, true);
        Assert.Equal(15, session.Total(GainCurrency.Favour));
        Assert.Equal(25, session.Total(GainCurrency.MemoryAmber));
    }
}
