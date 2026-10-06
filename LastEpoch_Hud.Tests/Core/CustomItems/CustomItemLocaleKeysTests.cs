using System.Reflection;
using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomItemLocaleKeysTests
{
    [Fact]
    public void All_ListsEveryConstKeyOnce()
    {
        var constants = typeof(CustomItemLocaleKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue())
            .Order()
            .ToList();

        Assert.Equal(constants, CustomItemLocaleKeys.All.Order().ToList());
        Assert.Equal(constants.Count, constants.Distinct().Count());
    }
}
