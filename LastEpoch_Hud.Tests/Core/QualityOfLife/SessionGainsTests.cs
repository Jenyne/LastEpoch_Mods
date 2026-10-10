using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Tests.Core.QualityOfLife;

public sealed class SessionGainsTests
{
    [Fact]
    public void SpendingAndNegativeBalancesDoNotCountAsIncome()
    {
        var session = new SessionGains();
        session.RecordIncrease(GainCurrency.Favour, 100, 150);
        session.RecordIncrease(GainCurrency.Favour, 150, 10);
        session.RecordIncrease(GainCurrency.Favour, 10, 80);
        session.RecordIncrease(GainCurrency.Favour, 80, 80);
        session.RecordIncrease(GainCurrency.Favour, -1, 80);
        Assert.Equal(120, session.Total(GainCurrency.Favour));
    }

    [Fact]
    public void IncomeAndRatesRemainSeparateForEachCurrency()
    {
        var session = new SessionGains();
        session.Advance(900, true);
        session.RecordIncrease(GainCurrency.Experience, 10000, 11000);
        session.RecordIncrease(GainCurrency.Favour, 10, 60);
        session.RecordIncrease(GainCurrency.MemoryAmber, 20, 220);
        Assert.Equal(4000, session.PerHour(GainCurrency.Experience));
        Assert.Equal(200, session.PerHour(GainCurrency.Favour));
        Assert.Equal(800, session.PerHour(GainCurrency.MemoryAmber));
    }

    [Fact]
    public void PauseFreezesBothTimeAndGainsAndResumePreservesTheSession()
    {
        var session = new SessionGains();
        session.Advance(60, true);
        session.RecordIncrease(GainCurrency.MemoryAmber, 0, 10);
        session.Paused = true;
        session.Advance(120, true);
        session.RecordIncrease(GainCurrency.MemoryAmber, 10, 30);
        session.Paused = false;
        session.Advance(60, true);
        session.RecordIncrease(GainCurrency.MemoryAmber, 30, 40);
        Assert.Equal(120, session.ActiveSeconds);
        Assert.Equal(20, session.Total(GainCurrency.MemoryAmber));
        Assert.Equal(600, session.PerHour(GainCurrency.MemoryAmber));
    }

    [Fact]
    public void LoadingOrMenuTimeDoesNotDiluteRates()
    {
        var session = new SessionGains();
        session.Advance(60, true);
        session.Advance(600, false);
        session.RecordIncrease(GainCurrency.Experience, 0, 100);
        session.Advance(60, true);
        Assert.Equal(120, session.ActiveSeconds);
        Assert.Equal(3000, session.PerHour(GainCurrency.Experience));
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidClockSamplesAreIgnored(double seconds)
    {
        var session = new SessionGains();
        session.Advance(seconds, true);
        Assert.Equal(0, session.ActiveSeconds);
        Assert.Equal(0, session.PerHour(GainCurrency.Experience));
    }

    [Fact]
    public void LargeTotalsSaturateInsteadOfWrapping()
    {
        var session = new SessionGains();
        session.RecordIncrease(GainCurrency.Experience, 0, long.MaxValue);
        session.RecordIncrease(GainCurrency.Experience, 0, 10);
        Assert.Equal(long.MaxValue, session.Total(GainCurrency.Experience));
        session.Advance(3600, true);
        Assert.Equal((double)long.MaxValue, session.PerHour(GainCurrency.Experience));
    }

    [Fact]
    public void ResetClearsAllCurrenciesTimeAndPause()
    {
        var session = new SessionGains();
        foreach (var currency in Enum.GetValues<GainCurrency>())
            session.RecordIncrease(currency, 0, 100);
        session.Advance(60, true);
        session.Paused = true;
        session.Reset();
        Assert.False(session.Paused);
        Assert.Equal(0, session.ActiveSeconds);
        foreach (var currency in Enum.GetValues<GainCurrency>())
            Assert.Equal(0, session.Total(currency));
    }
}
