using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Probe;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Probe;

public sealed class HeadhunterProbeLogTests
{
    private const string FakeScene = "FakeScene";
    private const string FakeActor = "FakeActor";

    public static TheoryData<string> AllLines =>
        new()
        {
            HeadhunterProbeLog.Flags(FakeScene, default, 1),
            HeadhunterProbeLog.EmergeStart(FakeScene, FakeActor, KillKind.Boss, 3f),
            HeadhunterProbeLog.EmergeEnd(FakeScene, FakeActor, KillKind.Boss, 2f, 3f, false),
            HeadhunterProbeLog.Cinematic(FakeScene, "src", 5f, true, false, false),
            HeadhunterProbeLog.Cutscene(FakeScene, "id", 12f),
            HeadhunterProbeLog.BossEngaged(FakeScene, FakeActor, false, false),
        };

    [Theory]
    [MemberData(nameof(AllLines))]
    public void AllLines_HavePrefixSceneOneLine(string line)
    {
        Assert.StartsWith(HeadhunterProbeLog.Prefix, line);
        Assert.Contains($"scene={FakeScene}", line);
        Assert.DoesNotContain('\n', line);
        Assert.DoesNotContain('\r', line);
    }

    [Theory]
    [InlineData(
        true,
        false,
        false,
        false,
        "cinematic=yes interruptible=no inputOff=no subtitles=no"
    )]
    [InlineData(
        false,
        true,
        false,
        false,
        "cinematic=no interruptible=yes inputOff=no subtitles=no"
    )]
    [InlineData(
        false,
        false,
        true,
        false,
        "cinematic=no interruptible=no inputOff=yes subtitles=no"
    )]
    [InlineData(
        false,
        false,
        false,
        true,
        "cinematic=no interruptible=no inputOff=no subtitles=yes"
    )]
    public void Flags_MapsEachFlagToItsLabel(
        bool cinematic,
        bool interruptible,
        bool inputOff,
        bool subtitles,
        string expected
    )
    {
        string line = HeadhunterProbeLog.Flags(
            FakeScene,
            new HeadhunterProbeFlags(cinematic, interruptible, inputOff, subtitles),
            2.5
        );

        Assert.Contains($"{expected} prevHeld=2.50s", line);
    }

    [Fact]
    public void EmergeStart_HasActorKindDuration()
    {
        string line = HeadhunterProbeLog.EmergeStart(FakeScene, FakeActor, KillKind.Boss, 3f);

        Assert.Contains($"actor={FakeActor} kind=boss duration=3.00s", line);
    }

    [Fact]
    public void EmergeEnd_HasSpentDurationSkipped()
    {
        string line = HeadhunterProbeLog.EmergeEnd(
            FakeScene,
            FakeActor,
            KillKind.Miniboss,
            2.9f,
            3f,
            true
        );

        Assert.Contains("kind=miniboss spent=2.90s duration=3.00s skipped=yes", line);
    }

    [Fact]
    public void Cinematic_InvertsGameNoFlags()
    {
        string line = HeadhunterProbeLog.Cinematic(FakeScene, "src", 5f, true, false, true);

        Assert.Contains("duration=5.00s interruptible=yes moveLock=yes invincible=no", line);
    }

    [Fact]
    public void Cutscene_HasIdDuration()
    {
        string line = HeadhunterProbeLog.Cutscene(FakeScene, "id1", 12f);

        Assert.Contains("id=id1 duration=12.00s", line);
    }

    [Fact]
    public void BossEngaged_HasActorAndFlags()
    {
        string line = HeadhunterProbeLog.BossEngaged(FakeScene, FakeActor, true, false);

        Assert.Contains($"actor={FakeActor} calledToArms=yes diedBefore=no", line);
    }

    [Fact]
    public void EmptyText_ShowsQuestionMark()
    {
        string line = HeadhunterProbeLog.BossEngaged("", null, false, false);

        Assert.Contains("scene=?", line);
        Assert.Contains("actor=?", line);
    }

    [Fact]
    public void Seconds_CommaCulture_Invariant()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            string line = HeadhunterProbeLog.Flags(FakeScene, default, 2.5);

            Assert.Contains("2.50", line);
            Assert.DoesNotContain("2,50", line);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
