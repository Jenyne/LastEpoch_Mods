using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterKillLogTests
{
    private static readonly Func<int, string> _statName = id => "Fake" + id;
    private static readonly Func<int, string> _tagName = id => "T" + id;

    [Fact]
    public void Format_IsOneLine()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1, 2)),
            [new BuffAction(BuffActionKind.Add, "HH_FakeA", 1, 0f, 0f, 1f, 1)],
            _statName,
            _tagName
        );

        Assert.DoesNotContain('\n', line);
        Assert.DoesNotContain('\r', line);
    }

    [Theory]
    [InlineData(true, "True", "False")]
    [InlineData(false, "False", "True")]
    public void Format_ContainsKindAndByMinion(bool byMinion, string expected, string opposite)
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Boss, byMinion, HeadhunterTestData.Mods(1)),
            [],
            _statName,
            _tagName
        );

        Assert.Contains("Boss", line);
        Assert.Contains(expected, line);
        Assert.DoesNotContain(opposite, line);
    }

    [Fact]
    public void Format_ListsModNamesInKillOrder()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(3, 1, 2)),
            [],
            _statName,
            _tagName
        );

        AssertInOrder(line, "Fake3", "Fake1", "Fake2");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_HandlesNullOrEmptyMods(bool useNull)
    {
        HeadhunterStatKey[] mods = useNull ? null : [];

        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, mods),
            [],
            _statName,
            _tagName
        );

        Assert.DoesNotContain("Fake", line);
    }

    [Fact]
    public void Format_ListsActionsInOrder()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            [
                new BuffAction(BuffActionKind.Add, "HH_FakeA", 1, 0f, 0f, 1f, 1),
                new BuffAction(BuffActionKind.Refresh, "HH_FakeB", 2, 0f, 0f, 1f, 1),
                new BuffAction(BuffActionKind.Add, "HH_FakeC", 3, 0f, 0f, 1f, 1),
            ],
            _statName,
            _tagName
        );

        AssertInOrder(line, "Add HH_FakeA", "Refresh HH_FakeB", "Add HH_FakeC");
    }

    [Fact]
    public void Format_HandlesEmptyActions()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Miniboss, false, HeadhunterTestData.Mods(1)),
            [],
            _statName,
            _tagName
        );

        Assert.Contains("Miniboss", line);
    }

    [Fact]
    public void Format_ShowsStacks_AboveOne()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            [new BuffAction(BuffActionKind.Add, "HH_FakeA", 1, 0f, 0f, 1f, 2)],
            _statName,
            _tagName
        );

        Assert.Contains("Add HH_FakeA x2", line);
    }

    [Fact]
    public void Format_NoStacks_AtOne()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, HeadhunterTestData.Mods(1)),
            [new BuffAction(BuffActionKind.Add, "HH_FakeA", 1, 0f, 0f, 1f, 1)],
            _statName,
            _tagName
        );

        Assert.DoesNotContain(" x1", line);
    }

    [Fact]
    public void Format_TaggedMod_AppendsTagName()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, [new HeadhunterStatKey(1, 8)]),
            [],
            id => "S" + id,
            _tagName
        );

        Assert.Contains("mods=[S1[T8]]", line);
    }

    [Fact]
    public void Format_UntaggedMod_NoTag()
    {
        string line = HeadhunterKillLog.Format(
            new KillInfo(KillKind.Rare, false, [new HeadhunterStatKey(1, 0)]),
            [],
            id => "S" + id,
            _tagName
        );

        Assert.Contains("mods=[S1]", line);
        Assert.DoesNotContain("S1[", line);
    }

    private static void AssertInOrder(string line, params string[] parts)
    {
        int previous = -1;
        foreach (string part in parts)
        {
            int index = line.IndexOf(part, StringComparison.Ordinal);
            Assert.True(index > previous, $"'{part}' missing or out of order");
            previous = index;
        }
    }
}
