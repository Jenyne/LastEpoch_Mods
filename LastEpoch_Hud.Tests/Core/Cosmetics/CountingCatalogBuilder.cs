using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

/// <summary>Fake catalog builder that counts its calls.</summary>
public sealed class CountingCatalogBuilder
{
    private readonly int _keptIds;

    public CountingCatalogBuilder(int keptIds)
    {
        _keptIds = keptIds;
    }

    public int Calls { get; private set; }

    public CosmeticCatalog Build()
    {
        Calls++;
        var catalog = new CosmeticCatalog();
        for (int i = 0; i < _keptIds; i++)
        {
            catalog.Add("MTX_" + i, "Foo");
        }
        return catalog;
    }
}
