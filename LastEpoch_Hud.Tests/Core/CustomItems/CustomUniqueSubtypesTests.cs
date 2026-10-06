using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomUniqueSubtypesTests
{
    [Fact]
    public void Get_NotSet_ReturnsNone()
    {
        var table = new CustomUniqueSubtypes();

        Assert.Equal(CustomItemIds.None, table.Get(503));
    }

    [Fact]
    public void Set_ThenGet_ReturnsValue()
    {
        var table = new CustomUniqueSubtypes();

        table.Set(503, 40);

        Assert.Equal(40, table.Get(503));
    }

    [Fact]
    public void Set_Twice_ReturnsLast()
    {
        var table = new CustomUniqueSubtypes();

        table.Set(503, 40);
        table.Set(503, 41);

        Assert.Equal(41, table.Get(503));
    }

    [Fact]
    public void Set_OtherUnique_DoesNotLeak()
    {
        var table = new CustomUniqueSubtypes();

        table.Set(502, 40);

        Assert.Equal(CustomItemIds.None, table.Get(503));
    }
}
