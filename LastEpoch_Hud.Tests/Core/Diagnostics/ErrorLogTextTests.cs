using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

/// <summary>The lines the error log writes.</summary>
public sealed class ErrorLogTextTests
{
    [Fact]
    public void Occurrence_WithDetails_JoinsContextAndDetails()
    {
        Assert.Equal(
            "Refs_Manager.Tick: System.Exception: boom",
            ErrorLogText.Occurrence("Refs_Manager.Tick", "System.Exception: boom")
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Occurrence_NullOrEmptyDetails_UsesPlaceholder(string details)
    {
        Assert.Equal(
            "Refs_Manager.Tick: (no details)",
            ErrorLogText.Occurrence("Refs_Manager.Tick", details)
        );
    }

    [Fact]
    public void Repeated_FormatsCount()
    {
        Assert.Equal(
            "Refs_Manager.Tick: previous error repeated 41 times (suppressed)",
            ErrorLogText.Repeated("Refs_Manager.Tick", 41)
        );
    }
}
