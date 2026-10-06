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
    public void Summary_FormatsCount()
    {
        Assert.Equal(
            "Refs_Manager.Tick: System.Exception: boom (3 times since last report)",
            ErrorLogText.Summary("Refs_Manager.Tick", "System.Exception: boom", 3)
        );
    }

    [Fact]
    public void Summary_CountOne_Singular()
    {
        Assert.Equal(
            "Refs_Manager.Tick: System.Exception: boom (1 time since last report)",
            ErrorLogText.Summary("Refs_Manager.Tick", "System.Exception: boom", 1)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Summary_NullOrEmptyDetails_UsesPlaceholder(string details)
    {
        Assert.Equal(
            "Refs_Manager.Tick: (no details) (2 times since last report)",
            ErrorLogText.Summary("Refs_Manager.Tick", details, 2)
        );
    }

    [Theory]
    [InlineData("System.NullReferenceException: x\n  at A()\n  at B()")]
    [InlineData("System.NullReferenceException: x\r\n  at A()\r\n  at B()")]
    public void ExceptionSummary_MultiLineMessage_KeepsFirstLine(string message)
    {
        Assert.Equal(
            "Il2CppInterop.Runtime.Il2CppException: System.NullReferenceException: x",
            ErrorLogText.ExceptionSummary("Il2CppInterop.Runtime.Il2CppException", message)
        );
    }

    [Fact]
    public void ExceptionSummary_NullTypeAndMessage_Empty()
    {
        Assert.Equal("", ErrorLogText.ExceptionSummary(null, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExceptionSummary_EmptyMessage_TypeOnly(string message)
    {
        Assert.Equal(
            "System.Exception",
            ErrorLogText.ExceptionSummary("System.Exception", message)
        );
    }

    [Fact]
    public void ExceptionSummary_NullType_LineOnly()
    {
        Assert.Equal("boom", ErrorLogText.ExceptionSummary(null, "boom"));
    }

    [Fact]
    public void Line_ZeroRepeats_UsesFullDetails()
    {
        Assert.Equal(
            "Refs_Manager.Tick: System.Exception: boom\n  at A()",
            ErrorLogText.Line(
                "Refs_Manager.Tick",
                0,
                "System.Exception: boom\n  at A()",
                "System.Exception: boom"
            )
        );
    }

    [Fact]
    public void Line_Repeats_UsesSummaryDetailsAndCount()
    {
        Assert.Equal(
            "Refs_Manager.Tick: System.Exception: boom (3 times since last report)",
            ErrorLogText.Line(
                "Refs_Manager.Tick",
                3,
                "System.Exception: boom\n  at A()",
                "System.Exception: boom"
            )
        );
    }
}
