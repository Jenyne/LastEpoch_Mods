using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.UI;

internal static class SessionGainCounters
{
    public static SessionGains Session { get; } = new();

    [ThreadStatic]
    static int xpDepth;

    [ThreadStatic]
    static int favourDepth;

    [ThreadStatic]
    static int amberDepth;

    [ThreadStatic]
    static int manualGrants;
    static string display = "";
    static float nextDisplay;
    static GUIStyle style;
    static readonly FactionID[] trackedFactions =
    {
        FactionID.CircleOfFortune,
        FactionID.MerchantsGuild,
        FactionID.TheWeaver,
    };

    public static bool Active =>
        !SaveManager.instance.IsNullOrDestroyed()
        && SaveManager.instance.initialized
        && Scenes.IsGameScene()
        && !Refs_Manager.player_actor.IsNullOrDestroyed()
        && Refs_Manager.player_actor.gameObject.activeInHierarchy
        && !Session.Paused
        && Time.timeScale > 0;

    public static void Tick()
    {
        try
        {
            if (
                Scenes.IsCharacterSelection()
                || Scenes.SceneName == "Login"
                || Scenes.SceneName == "ClientSplash"
            )
                Session.Reset();
            // A long loading frame is not active play time.
            float delta = Time.unscaledDeltaTime;
            Session.Advance(delta, Active && delta <= 1);
            if (Time.unscaledTime >= nextDisplay)
            {
                nextDisplay = Time.unscaledTime + .25f;
                display = Format();
                SessionStatsControls.Refresh();
            }
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Session gain counters");
        }
    }

    public static string Format()
    {
        string state = LocaleRegistry.Translate(Session.Paused ? "Paused" : "Session");
        long seconds = (long)Math.Min(Session.ActiveSeconds, long.MaxValue);
        return state
            + " "
            + (seconds / 3600).ToString("00")
            + ":"
            + ((seconds / 60) % 60).ToString("00")
            + ":"
            + (seconds % 60).ToString("00")
            + "\n"
            + Line("XP", GainCurrency.Experience)
            + "\n"
            + Line("Favour", GainCurrency.Favour)
            + "\n"
            + Line("Memory Amber", GainCurrency.MemoryAmber);
    }

    static string Line(string label, GainCurrency currency) =>
        LocaleRegistry.Translate(label)
        + ": "
        + Session.Total(currency).ToString("N0")
        + "  |  "
        + Session.PerHour(currency).ToString("N0")
        + "/h";

    public static void Draw()
    {
        if (
            !ModSettings.SessionStats.ShowOverlay.Value
            || !Scenes.IsGameScene()
            || Refs_Manager.player_actor.IsNullOrDestroyed()
        )
            return;
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.richText = false;
            style.alignment = TextAnchor.UpperRight;
        }
        // Labels only: no full-screen canvas, raycast surface or interactive GUI controls.
        var rect = new Rect(Screen.width - 370, 90, 350, 92);
        var previous = GUI.color;
        try
        {
            GUI.color = Color.black;
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), display, style);
            GUI.color = new Color(.93f, .84f, .65f);
            GUI.Label(rect, display, style);
        }
        finally
        {
            GUI.color = previous;
        }
    }

    public static IDisposable SuppressManualGrants() => new ManualGrant();

    sealed class ManualGrant : IDisposable
    {
        bool disposed;

        public ManualGrant() => manualGrants++;

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            manualGrants--;
        }
    }

    public sealed class Observation
    {
        public GainCurrency Currency;
        public long Before;
        public bool Outer;
        public bool Complete;
    }

    static Observation BeginXp(ExperienceTracker tracker)
    {
        if (
            !Active
            || manualGrants != 0
            || tracker.IsNullOrDestroyed()
            || Refs_Manager.exp_tracker.IsNullOrDestroyed()
            || tracker.Pointer != Refs_Manager.exp_tracker.Pointer
        )
            return null;
        var state = new Observation
        {
            Currency = GainCurrency.Experience,
            Before = tracker.CurrentExperience,
            Outer = xpDepth == 0,
        };
        xpDepth++;
        return state;
    }

    static bool CurrencyOf(Faction faction, out GainCurrency currency)
    {
        currency = GainCurrency.Favour;
        var tracker = Refs_Manager.faction_tracker;
        if (tracker.IsNullOrDestroyed() || tracker.factions == null)
            return false;
        foreach (var id in trackedFactions)
            if (
                tracker.factions.TryGetValue(id, out Faction local)
                && !local.IsNullOrDestroyed()
                && local.Pointer == faction.Pointer
            )
            {
                currency =
                    id == FactionID.TheWeaver ? GainCurrency.MemoryAmber : GainCurrency.Favour;
                return true;
            }
        return false;
    }

    static Observation BeginFavour(Faction faction)
    {
        if (
            !Active
            || manualGrants != 0
            || faction.IsNullOrDestroyed()
            || !CurrencyOf(faction, out var currency)
        )
            return null;
        bool outer = currency == GainCurrency.MemoryAmber ? amberDepth == 0 : favourDepth == 0;
        var state = new Observation
        {
            Currency = currency,
            Before = faction.Favor,
            Outer = outer,
        };
        if (currency == GainCurrency.MemoryAmber)
            amberDepth++;
        else
            favourDepth++;
        return state;
    }

    static void End(Observation state, long after, bool record)
    {
        if (state == null || state.Complete)
            return;
        state.Complete = true;
        if (state.Currency == GainCurrency.Experience)
            xpDepth--;
        else if (state.Currency == GainCurrency.MemoryAmber)
            amberDepth--;
        else
            favourDepth--;
        if (record && state.Outer)
            Session.RecordIncrease(state.Currency, state.Before, after);
    }

    static void XpPrefix(ExperienceTracker tracker, out Observation state)
    {
        state = null;
        try
        {
            state = BeginXp(tracker);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Session XP observation");
        }
    }

    static void XpPostfix(ExperienceTracker tracker, Observation state)
    {
        if (state == null)
            return;
        try
        {
            End(state, tracker.CurrentExperience, true);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Session XP total");
        }
        finally
        {
            End(state, 0, false);
        }
    }

    [HarmonyPatch(typeof(ExperienceTracker), "GainExp")]
    public class GainExpPatch
    {
        [HarmonyPrefix]
        static void Prefix(ExperienceTracker __instance, out Observation __state) =>
            XpPrefix(__instance, out __state);

        [HarmonyPostfix]
        static void Postfix(ExperienceTracker __instance, Observation __state) =>
            XpPostfix(__instance, __state);

        [HarmonyFinalizer]
        static void Finalizer(Observation __state) => End(__state, 0, false);
    }

    [HarmonyPatch(typeof(ExperienceTracker), "GainExpDirect")]
    public class GainExpDirectPatch
    {
        [HarmonyPrefix]
        static void Prefix(ExperienceTracker __instance, out Observation __state) =>
            XpPrefix(__instance, out __state);

        [HarmonyPostfix]
        static void Postfix(ExperienceTracker __instance, Observation __state) =>
            XpPostfix(__instance, __state);

        [HarmonyFinalizer]
        static void Finalizer(Observation __state) => End(__state, 0, false);
    }

    [HarmonyPatch(typeof(ExperienceTracker), "GainExpFromEnemyOrMote")]
    public class GainExpFromEnemyPatch
    {
        [HarmonyPrefix]
        static void Prefix(ExperienceTracker __instance, out Observation __state) =>
            XpPrefix(__instance, out __state);

        [HarmonyPostfix]
        static void Postfix(ExperienceTracker __instance, Observation __state) =>
            XpPostfix(__instance, __state);

        [HarmonyFinalizer]
        static void Finalizer(Observation __state) => End(__state, 0, false);
    }

    [HarmonyPatch(typeof(Faction), "GainFavor")]
    public class GainFavorPatch
    {
        [HarmonyPrefix]
        static void Prefix(Faction __instance, out Observation __state)
        {
            __state = null;
            try
            {
                __state = BeginFavour(__instance);
            }
            catch (Exception ex)
            {
                ErrorLog.Report(ex, "Session Favour observation");
            }
        }

        [HarmonyPostfix]
        static void Postfix(Faction __instance, Observation __state)
        {
            if (__state == null)
                return;
            try
            {
                End(__state, __instance.Favor, true);
            }
            catch (Exception ex)
            {
                ErrorLog.Report(ex, "Session Favour total");
            }
            finally
            {
                End(__state, 0, false);
            }
        }

        [HarmonyFinalizer]
        static void Finalizer(Observation __state) => End(__state, 0, false);
    }
}
