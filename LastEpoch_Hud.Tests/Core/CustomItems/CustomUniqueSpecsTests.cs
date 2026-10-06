using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomUniqueSpecsTests
{
    [Theory]
    [InlineData("Headhunter", 500, 2, -1, true, 40, true, 0)]
    [InlineData("Mjolner", 501, 7, 10, false, 78, false, 60)]
    [InlineData("Sands of Silk", 502, 1, 71, true, 16, true, 0)]
    [InlineData("Essentia Sanguis", 503, 4, 15, true, 52, true, 0)]
    public void Spec_HoldsTodaysValues(
        string name,
        int uniqueId,
        int baseType,
        int baseId,
        bool addsBase,
        int level,
        bool overrideLevel,
        int effectiveLevel
    )
    {
        CustomUniqueSpec spec = CustomUniqueSpecs.All.Single(s => s.Name == name);

        Assert.Equal((ushort)uniqueId, spec.UniqueId);
        Assert.Equal((byte)baseType, spec.BaseType);
        Assert.Equal(baseId, spec.BaseId);
        Assert.Equal(addsBase, spec.AddsBase);
        Assert.Equal(level, spec.LevelRequirement);
        Assert.Equal(overrideLevel, spec.OverrideLevelRequirement);
        Assert.Equal(effectiveLevel, spec.EffectiveLevelForLegendaryPotential);
    }

    [Fact]
    public void Fields_AreInAll()
    {
        Assert.Equal(
            new[]
            {
                CustomUniqueSpecs.Headhunter,
                CustomUniqueSpecs.Mjolner,
                CustomUniqueSpecs.SandsOfSilk,
                CustomUniqueSpecs.EssentiaSanguis,
            },
            CustomUniqueSpecs.All
        );
    }

    [Fact]
    public void UniqueIds_AreDistinct()
    {
        var ids = CustomUniqueSpecs.All.Select(s => s.UniqueId).ToList();

        Assert.Equal(CustomUniqueSpecs.All.Count, ids.Distinct().Count());
    }

    [Fact]
    public void FixedBases_AreDistinct()
    {
        var bases = CustomUniqueSpecs
            .All.Where(s => s.BaseId >= 0)
            .Select(s => (s.BaseType, s.BaseId))
            .ToList();

        Assert.Equal(bases.Count, bases.Distinct().Count());
    }
}
