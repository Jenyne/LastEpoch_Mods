namespace LastEpoch_Hud.Tests.Patches;

public sealed class Cpp2IlNamesTests
{
    [Theory]
    [InlineData("Il2Cpp.Fake", "Fake")]
    [InlineData("Il2CppLE.Ns.Fake", "LE.Ns.Fake")]
    [InlineData("Il2Cpp.Outer/Inner", "Outer/Inner")]
    [InlineData("Il2CppSystem.Fake", "System.Fake")]
    [InlineData("UnityEngine.Fake", "UnityEngine.Fake")]
    public void FromInteropType_MapsName(string interop, string expected) =>
        Assert.Equal(expected, Cpp2IlNames.FromInteropType(interop));

    [Theory]
    [InlineData("A.B.IFake.Do", "A_B_IFake_Do")]
    [InlineData("Do", "Do")]
    public void ToInteropMethod_MapsName(string cpp2Il, string expected) =>
        Assert.Equal(expected, Cpp2IlNames.ToInteropMethod(cpp2Il));
}
