using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class EnumIdMapTests
{
    [Fact]
    public void Build_ByteBackedEnum_MapsEveryNameToItsValue()
    {
        Dictionary<string, int> ids = EnumIdMap.Build(typeof(FakeByteStat));

        Assert.Equal(2, ids.Count);
        Assert.Equal(1, ids["FakeA"]);
        Assert.Equal(200, ids["FakeB"]);
    }

    [Fact]
    public void Build_IntEnum_MapsValues()
    {
        Dictionary<string, int> ids = EnumIdMap.Build(typeof(FakeIntStat));

        Assert.Equal(2, ids.Count);
        Assert.Equal(7, ids["FakeA"]);
        Assert.Equal(70000, ids["FakeB"]);
    }

    [Fact]
    public void Build_Alias_MapsBothNamesToSameId()
    {
        Dictionary<string, int> ids = EnumIdMap.Build(typeof(FakeAliasStat));

        Assert.Equal(2, ids.Count);
        Assert.Equal(5, ids["FakeFirst"]);
        Assert.Equal(5, ids["FakeSecond"]);
    }
}
