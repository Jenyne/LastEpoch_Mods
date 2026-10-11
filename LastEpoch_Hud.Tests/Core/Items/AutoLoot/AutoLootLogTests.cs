using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class AutoLootLogTests
{
    private const int FakeType = 4242;

    [Theory]
    [InlineData(AutoLootAction.Stored, "action=stored")]
    [InlineData(AutoLootAction.Picked, "action=picked")]
    [InlineData(AutoLootAction.Kept, "action=kept")]
    public void Direct_HasTypeKindAndAction(AutoLootAction action, string expected)
    {
        string line = AutoLootLog.Direct(FakeType, LootKind.Key, DropOrigin.Game, action);

        Assert.Contains("AutoLoot:", line);
        Assert.Contains("type=4242", line);
        Assert.Contains("kind=Key", line);
        Assert.Contains(expected, line);
        Assert.DoesNotContain("filter=", line);
    }

    [Theory]
    [InlineData(LootVisibility.Hidden, "filter=hidden")]
    [InlineData(LootVisibility.Shown, "filter=shown")]
    [InlineData(LootVisibility.Unfiltered, "filter=none")]
    public void Filter_HasVisibility(LootVisibility visibility, string expected)
    {
        string line = AutoLootLog.Filter(
            FakeType,
            LootKind.Gear,
            DropOrigin.Game,
            visibility,
            AutoLootAction.Kept
        );

        Assert.Contains(expected, line);
        Assert.Contains("kind=Gear", line);
        Assert.Contains("type=4242", line);
    }

    [Theory]
    [InlineData(AutoLootAction.Shattered, "action=shattered")]
    [InlineData(AutoLootAction.Sold, "action=sold")]
    [InlineData(AutoLootAction.Picked, "action=picked")]
    [InlineData(AutoLootAction.Kept, "action=kept")]
    public void Filter_HasAction(AutoLootAction action, string expected)
    {
        string line = AutoLootLog.Filter(
            FakeType,
            LootKind.Gear,
            DropOrigin.Game,
            LootVisibility.Hidden,
            action
        );

        Assert.Contains(expected, line);
    }

    [Fact]
    public void Direct_ModOrigin_Marked()
    {
        string line = AutoLootLog.Direct(
            FakeType,
            LootKind.Key,
            DropOrigin.Mod,
            AutoLootAction.Kept
        );

        Assert.EndsWith(" origin=mod", line);
    }

    [Fact]
    public void Filter_ModOrigin_Marked()
    {
        string line = AutoLootLog.Filter(
            FakeType,
            LootKind.Gear,
            DropOrigin.Mod,
            LootVisibility.Hidden,
            AutoLootAction.Sold
        );

        Assert.EndsWith(" origin=mod", line);
    }

    [Fact]
    public void Direct_ModOrigin_ExactLine()
    {
        string line = AutoLootLog.Direct(4242, LootKind.Key, DropOrigin.Mod, AutoLootAction.Stored);

        Assert.Equal("AutoLoot: type=4242 kind=Key action=stored origin=mod", line);
    }

    [Fact]
    public void Filter_GameOrigin_ExactLine()
    {
        string line = AutoLootLog.Filter(
            4242,
            LootKind.Gear,
            DropOrigin.Game,
            LootVisibility.Hidden,
            AutoLootAction.Shattered
        );

        Assert.Equal("AutoLoot: type=4242 kind=Gear filter=hidden action=shattered", line);
    }

    [Fact]
    public void Direct_GameOrigin_NotMarked()
    {
        string line = AutoLootLog.Direct(
            FakeType,
            LootKind.Key,
            DropOrigin.Game,
            AutoLootAction.Kept
        );

        Assert.DoesNotContain("origin=", line);
    }

    [Fact]
    public void Filter_GameOrigin_NotMarked()
    {
        string line = AutoLootLog.Filter(
            FakeType,
            LootKind.Gear,
            DropOrigin.Game,
            LootVisibility.Hidden,
            AutoLootAction.Sold
        );

        Assert.DoesNotContain("origin=", line);
    }

    [Fact]
    public void Direct_EveryAction_DistinctKnownTokens()
    {
        var lines = Enum.GetValues<AutoLootAction>()
            .Select(action => AutoLootLog.Direct(FakeType, LootKind.Key, DropOrigin.Game, action))
            .ToList();

        Assert.DoesNotContain(lines, line => line.Contains("unknown"));
        Assert.Equal(lines.Count, lines.Distinct().Count());
    }

    [Fact]
    public void Filter_EveryVisibility_DistinctKnownTokens()
    {
        var lines = Enum.GetValues<LootVisibility>()
            .Select(visibility =>
                AutoLootLog.Filter(
                    FakeType,
                    LootKind.Gear,
                    DropOrigin.Game,
                    visibility,
                    AutoLootAction.Kept
                )
            )
            .ToList();

        Assert.DoesNotContain(lines, line => line.Contains("unknown"));
        Assert.Equal(lines.Count, lines.Distinct().Count());
    }
}
