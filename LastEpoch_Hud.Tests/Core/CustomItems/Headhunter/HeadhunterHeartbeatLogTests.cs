using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterHeartbeatLogTests
{
    [Fact]
    public void Format_HasFrameScaleScene()
    {
        string line = HeadhunterHeartbeatLog.Format(12345, 0.25f, "FakeScene");

        Assert.StartsWith("HH heartbeat", line);
        Assert.Contains("frame=12345", line);
        Assert.Contains("timeScale=0.25", line);
        Assert.Contains("scene=FakeScene", line);
    }

    [Fact]
    public void Format_CommaCulture_InvariantDecimal()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            string line = HeadhunterHeartbeatLog.Format(1, 0.5f, "FakeScene");

            Assert.Contains("0.5", line);
            Assert.DoesNotContain("0,5", line);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
