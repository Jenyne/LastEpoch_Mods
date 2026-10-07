using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterRunResetTests
{
    [Fact]
    public void IsCharacterExit_CharacterSelect_True()
    {
        Assert.True(HeadhunterRunReset.IsCharacterExit(HeadhunterRunReset.CharacterSelectScene));
    }

    [Fact]
    public void IsCharacterExit_Login_True()
    {
        Assert.True(HeadhunterRunReset.IsCharacterExit(HeadhunterRunReset.LoginScene));
    }

    [Theory]
    [InlineData("FakeZoneA")]
    [InlineData("FakeZoneB")]
    [InlineData(HeadhunterRunReset.CharacterSelectScene + "Fake")]
    [InlineData("Fake" + HeadhunterRunReset.LoginScene)]
    [InlineData("login")]
    public void IsCharacterExit_OtherScene_False(string sceneName)
    {
        Assert.False(HeadhunterRunReset.IsCharacterExit(sceneName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsCharacterExit_NullOrEmpty_False(string sceneName)
    {
        Assert.False(HeadhunterRunReset.IsCharacterExit(sceneName));
    }
}
