using LastEpoch_Hud.Scripts.Core.Login;

namespace LastEpoch_Hud.Tests.Core.Login;

public sealed class ClientStartupReadinessTests
{
    [Fact]
    public void ReproducedSlowStartupCannotDispatchUntilApplicationLoginCompletes()
    {
        var client = new ClientStartupReadiness();
        var attempt = new OfflineStartupAttempt();
        client.Observe("ClientStateManager: Application state changed to SystemLoading.");
        foreach (
            string message in new[]
            {
                "ShellSceneManager: State transition SplashScreen -> Login",
                "Scene CharacterSelectScene load started: load mode: Additive",
                "ShellSceneManager: Scene Login loaded.",
                "ShellSceneManager: Scene CharacterSelectScene loaded.",
            }
        )
        {
            client.Observe(message);
            Assert.False(attempt.TryBegin(client.ReadyForOfflineClick, 20));
        }
        Assert.True(client.Observe("ClientStateManager: Application state changed to Login."));
        Assert.False(attempt.TryBegin(client.ReadyForOfflineClick, 24.682));
        Assert.True(attempt.TryBegin(client.ReadyForOfflineClick, 24.932));
        Assert.False(attempt.TryBegin(client.ReadyForOfflineClick, 40));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ShellSceneManager: State transition SplashScreen -> Login")]
    [InlineData("Successfully authenticated with the login service")]
    [InlineData("ClientStateManager: Application state changed to Login")]
    [InlineData("[Offline] ClientStateManager: Application state changed to Login.")]
    [InlineData("ClientStateManager: Application state changed to Login.\nExtra.")]
    public void MissingOrUnrelatedSignalsNeverPermitOfflineDispatch(string message)
    {
        var client = new ClientStartupReadiness();
        client.Observe(message);
        Assert.False(client.ReadyForOfflineClick);
    }

    [Theory]
    [InlineData("SystemLoading")]
    [InlineData("CharacterSelect")]
    [InlineData("InGame")]
    [InlineData("FutureState")]
    public void LeavingLoginClosesTheGate(string state)
    {
        var client = new ClientStartupReadiness();
        client.Observe("ClientStateManager: Application state changed to Login.");
        Assert.True(client.ReadyForOfflineClick);
        client.Observe("ClientStateManager: Application state changed to " + state + ".");
        Assert.False(client.ReadyForOfflineClick);
    }

    [Fact]
    public void NewListenerMustObserveFreshLoginCompletion()
    {
        var client = new ClientStartupReadiness();
        client.Observe("ClientStateManager: Application state changed to Login.");
        client.Reset();
        Assert.False(client.ReadyForOfflineClick);
        client.Observe("ClientStateManager: Application state changed to Login.");
        Assert.True(client.ReadyForOfflineClick);
    }
}
