using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class AutoLootRulesTests
{
    private static readonly AutoLootSettings _allOn = new(true, true, true, true, true, true);

    [Theory]
    [InlineData(LootKind.Key, true, false, false)]
    [InlineData(LootKind.WovenEcho, false, true, false)]
    [InlineData(LootKind.Material, false, false, true)]
    public void Route_DirectKindToggleOn_PickUp(
        LootKind kind,
        bool keys,
        bool echoes,
        bool materials
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, false, false, false);
        Assert.Equal(LootRoute.PickUp, AutoLootRules.Route(kind, settings));
    }

    [Theory]
    [InlineData(LootKind.Key, false, true, true)]
    [InlineData(LootKind.WovenEcho, true, false, true)]
    [InlineData(LootKind.Material, true, true, false)]
    public void Route_DirectKindToggleOff_IgnoreEvenWithFilterToggles(
        LootKind kind,
        bool keys,
        bool echoes,
        bool materials
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, true, true, true);
        Assert.Equal(LootRoute.Ignore, AutoLootRules.Route(kind, settings));
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void Route_GearAnyFilterToggle_Filter(bool pickUp, bool sell, bool shatter)
    {
        var settings = new AutoLootSettings(false, false, false, pickUp, sell, shatter);
        Assert.Equal(LootRoute.Filter, AutoLootRules.Route(LootKind.Gear, settings));
    }

    [Fact]
    public void Route_GearFilterTogglesOff_Ignore()
    {
        var settings = new AutoLootSettings(true, true, true, false, false, false);
        Assert.Equal(LootRoute.Ignore, AutoLootRules.Route(LootKind.Gear, settings));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Route_Other_AlwaysIgnore(bool allOn)
    {
        AutoLootSettings settings = allOn ? _allOn : default;
        Assert.Equal(LootRoute.Ignore, AutoLootRules.Route(LootKind.Other, settings));
    }

    [Fact]
    public void ForFilter_ShownPickupOn_PickUpOnly()
    {
        Assert.Equal(
            new FilterLootPlan(true, false, false),
            AutoLootRules.ForFilter(LootVisibility.Shown, _allOn)
        );
    }

    [Fact]
    public void ForFilter_ShownPickupOff_Nothing()
    {
        var settings = new AutoLootSettings(false, false, false, false, true, true);
        Assert.Equal(
            default(FilterLootPlan),
            AutoLootRules.ForFilter(LootVisibility.Shown, settings)
        );
    }

    [Fact]
    public void ForFilter_HiddenShatterAndSell_ShatterThenSell()
    {
        Assert.Equal(
            new FilterLootPlan(false, true, true),
            AutoLootRules.ForFilter(LootVisibility.Hidden, _allOn)
        );
    }

    [Fact]
    public void ForFilter_HiddenSellOnly_Sell()
    {
        var settings = new AutoLootSettings(false, false, false, false, true, false);
        Assert.Equal(
            new FilterLootPlan(false, false, true),
            AutoLootRules.ForFilter(LootVisibility.Hidden, settings)
        );
    }

    [Fact]
    public void ForFilter_HiddenShatterOnly_Shatter()
    {
        var settings = new AutoLootSettings(false, false, false, false, false, true);
        Assert.Equal(
            new FilterLootPlan(false, true, false),
            AutoLootRules.ForFilter(LootVisibility.Hidden, settings)
        );
    }

    [Fact]
    public void ForFilter_HiddenPickupOnly_Nothing()
    {
        var settings = new AutoLootSettings(false, false, false, true, false, false);
        Assert.Equal(
            default(FilterLootPlan),
            AutoLootRules.ForFilter(LootVisibility.Hidden, settings)
        );
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, false, false, true, true, true)]
    [InlineData(false, false, false, true, false, false)]
    public void ForFilter_Unfiltered_Nothing(
        bool keys,
        bool echoes,
        bool materials,
        bool pickUp,
        bool sell,
        bool shatter
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, pickUp, sell, shatter);
        FilterLootPlan plan = AutoLootRules.ForFilter(LootVisibility.Unfiltered, settings);
        Assert.Equal(default(FilterLootPlan), plan);
        Assert.True(plan.IsEmpty);
    }

    [Fact]
    public void ForOrigin_Mod_DropsSellAndShatter()
    {
        Assert.Equal(
            new AutoLootSettings(true, true, true, true, false, false),
            AutoLootRules.ForOrigin(_allOn, DropOrigin.Mod)
        );
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void ForOrigin_Mod_KeepsPickupToggles(
        bool keys,
        bool echoes,
        bool materials,
        bool pickUp
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, pickUp, false, false);
        Assert.Equal(settings, AutoLootRules.ForOrigin(settings, DropOrigin.Mod));
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(false, false, false, false, true, true)]
    public void ForOrigin_Game_Unchanged(
        bool keys,
        bool echoes,
        bool materials,
        bool pickUp,
        bool sell,
        bool shatter
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, pickUp, sell, shatter);
        Assert.Equal(settings, AutoLootRules.ForOrigin(settings, DropOrigin.Game));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ForOrigin_ModSellShatterOnly_NotAnyEnabled(bool sell, bool shatter)
    {
        var settings = new AutoLootSettings(false, false, false, false, sell, shatter);
        Assert.False(AutoLootRules.ForOrigin(settings, DropOrigin.Mod).AnyEnabled);
    }

    [Fact]
    public void ForFilter_ModHidden_Nothing()
    {
        FilterLootPlan plan = AutoLootRules.ForFilter(
            LootVisibility.Hidden,
            AutoLootRules.ForOrigin(_allOn, DropOrigin.Mod)
        );
        Assert.True(plan.IsEmpty);
    }

    [Fact]
    public void ForFilter_ModShown_PickUp()
    {
        Assert.Equal(
            new FilterLootPlan(true, false, false),
            AutoLootRules.ForFilter(
                LootVisibility.Shown,
                AutoLootRules.ForOrigin(_allOn, DropOrigin.Mod)
            )
        );
    }

    [Theory]
    [InlineData(LootKind.Key, LootRoute.PickUp)]
    [InlineData(LootKind.WovenEcho, LootRoute.PickUp)]
    [InlineData(LootKind.Material, LootRoute.PickUp)]
    [InlineData(LootKind.Gear, LootRoute.Filter)]
    [InlineData(LootKind.Other, LootRoute.Ignore)]
    public void Route_ModOrigin_SameAsGame(LootKind kind, LootRoute expected)
    {
        Assert.Equal(
            expected,
            AutoLootRules.Route(kind, AutoLootRules.ForOrigin(_allOn, DropOrigin.Mod))
        );
    }
}
