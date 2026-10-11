using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class AutoLootRulesTests
{
    private static readonly AutoLootSettings _allOn = new(true, true, true, true, true, true);

    [Theory]
    [InlineData(LootKind.Key, true, false, false)]
    [InlineData(LootKind.WovenEcho, false, true, false)]
    [InlineData(LootKind.Material, false, false, true)]
    public void Route_DirectKindToggleOn_GoesToStorage_Store(
        LootKind kind,
        bool keys,
        bool echoes,
        bool materials
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, false, false, false);
        Assert.Equal(LootRoute.Store, AutoLootRules.Route(kind, true, settings));
    }

    [Theory]
    [InlineData(LootKind.Key, true, false, false)]
    [InlineData(LootKind.WovenEcho, false, true, false)]
    [InlineData(LootKind.Material, false, false, true)]
    public void Route_DirectKindToggleOn_NotToStorage_PickUp(
        LootKind kind,
        bool keys,
        bool echoes,
        bool materials
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, false, false, false);
        Assert.Equal(LootRoute.PickUp, AutoLootRules.Route(kind, false, settings));
    }

    [Theory]
    [InlineData(LootKind.Key, false, true, true, true)]
    [InlineData(LootKind.WovenEcho, true, false, true, true)]
    [InlineData(LootKind.Material, true, true, false, true)]
    [InlineData(LootKind.Key, false, true, true, false)]
    [InlineData(LootKind.WovenEcho, true, false, true, false)]
    [InlineData(LootKind.Material, true, true, false, false)]
    public void Route_DirectKindToggleOff_IgnoreWhateverStorage(
        LootKind kind,
        bool keys,
        bool echoes,
        bool materials,
        bool goesToStorage
    )
    {
        var settings = new AutoLootSettings(keys, echoes, materials, true, true, true);
        Assert.Equal(LootRoute.Ignore, AutoLootRules.Route(kind, goesToStorage, settings));
    }

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    public void Route_GearAnyFilterToggle_FilterWhateverStorage(
        bool pickUp,
        bool sell,
        bool shatter,
        bool goesToStorage
    )
    {
        var settings = new AutoLootSettings(false, false, false, pickUp, sell, shatter);
        Assert.Equal(LootRoute.Filter, AutoLootRules.Route(LootKind.Gear, goesToStorage, settings));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Route_GearFilterTogglesOff_Ignore(bool goesToStorage)
    {
        var settings = new AutoLootSettings(true, true, true, false, false, false);
        Assert.Equal(LootRoute.Ignore, AutoLootRules.Route(LootKind.Gear, goesToStorage, settings));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Route_Other_IgnoreWhateverStorage(bool allOn, bool goesToStorage)
    {
        AutoLootSettings settings = allOn ? _allOn : default;
        Assert.Equal(
            LootRoute.Ignore,
            AutoLootRules.Route(LootKind.Other, goesToStorage, settings)
        );
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
    [InlineData(LootKind.Key, false, LootRoute.PickUp)]
    [InlineData(LootKind.WovenEcho, false, LootRoute.PickUp)]
    [InlineData(LootKind.Material, false, LootRoute.PickUp)]
    [InlineData(LootKind.Gear, false, LootRoute.Filter)]
    [InlineData(LootKind.Other, false, LootRoute.Ignore)]
    [InlineData(LootKind.Key, true, LootRoute.Store)]
    [InlineData(LootKind.WovenEcho, true, LootRoute.Store)]
    [InlineData(LootKind.Material, true, LootRoute.Store)]
    [InlineData(LootKind.Gear, true, LootRoute.Filter)]
    [InlineData(LootKind.Other, true, LootRoute.Ignore)]
    public void Route_ModOrigin_SameAsGame(LootKind kind, bool goesToStorage, LootRoute expected)
    {
        Assert.Equal(
            expected,
            AutoLootRules.Route(
                kind,
                goesToStorage,
                AutoLootRules.ForOrigin(_allOn, DropOrigin.Mod)
            )
        );
    }
}
