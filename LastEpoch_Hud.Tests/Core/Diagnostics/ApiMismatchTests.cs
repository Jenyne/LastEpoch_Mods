using LastEpoch_Hud.Scripts.Core.Diagnostics;

namespace LastEpoch_Hud.Tests.Core.Diagnostics;

/// <summary>Tells a permanent game API mismatch from a transient failure.</summary>
public sealed class ApiMismatchTests
{
    [Fact]
    public void Is_MissingMethod_True()
    {
        Assert.True(ApiMismatch.Is(new MissingMethodException()));
    }

    [Fact]
    public void Is_MissingField_True()
    {
        Assert.True(ApiMismatch.Is(new MissingFieldException()));
    }

    [Fact]
    public void Is_MissingMember_True()
    {
        Assert.True(ApiMismatch.Is(new MissingMemberException()));
    }

    [Fact]
    public void Is_TypeLoad_True()
    {
        Assert.True(ApiMismatch.Is(new TypeLoadException()));
    }

    [Fact]
    public void Is_InvalidOperation_False()
    {
        Assert.False(ApiMismatch.Is(new InvalidOperationException()));
    }

    [Fact]
    public void Is_NullReference_False()
    {
#pragma warning disable CA2201 // The runtime's own exception type is the case under test.
        Assert.False(ApiMismatch.Is(new NullReferenceException()));
#pragma warning restore CA2201
    }
}
