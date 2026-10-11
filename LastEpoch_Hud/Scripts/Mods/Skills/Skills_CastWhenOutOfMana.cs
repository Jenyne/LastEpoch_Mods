using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Skills;

/// <summary>
/// Two ways to cast without enough mana, both hooked only at InitialiseAbilityUse and UseAbility:
/// - When mana is short, the mod runs the game's own cooldown and remaining checks
///   (canUseAbilityChecksAfterManaAndCooldown) and, if they pass, starts the cast with the game's
///   ignoreCanUseAbilityChecks, skipping the mana refusal.
/// - That skips the game's pacing too (Heartseeker cast ~1.8x faster than attack speed), so the
///   mod waits until the previous cast's animation has ended before starting the next one. A cast
///   is recorded at its release point, so that is castDuration minus the measured wind-up.
///   With movement held it also waits until the character has actually moved, so holding both
///   alternates cast, move, cast like the game does with mana.
/// - The forced cast's spend is skipped (useMana = false); "Allow Negative Mana" charges the cost
///   afterwards through the game's reduceMana, with canGoNegative on. Casts the game allows by
///   itself (e.g. gear that lets mana go negative) are never changed.
/// What is deliberately not done, each of which broke casting in testing:
/// - patching the cast checks (ClientUsingAbilityState.CanUseAbility, UsingAbilityPlayer.canUseAbility):
///   they take the target as ref Vector3, and patching them broke targeting even with the modes off;
/// - changing current mana during a check, or letting the game spend mana it doesn't have:
///   both froze projectiles;
/// - patching getters such as getFreeWhenOutOfMana: tiny IL2CPP getters can share native code;
/// - setting freeWhenOutOfMana on the server-side CastingData: the server's check ignores it.
/// </summary>
internal static class Skills_CastWhenOutOfMana
{
    // The player mana whose canGoNegative this mod manages, and the character's own value to
    // restore once negative mode is off.
    private static System.IntPtr _managedMana = System.IntPtr.Zero;
    private static bool _originalCanGoNegative = false;

    // The ability whose short cast the mod started, until that cast's spend runs. The game can
    // report the start as failed and still cast a moment later, so it lives a few seconds.
    private static System.IntPtr _overriddenAbility = System.IntPtr.Zero;
    private static float _overrideExpires = 0f;

    // When the player's last cast ran and how long it lasts, to pace forced casts.
    private static float _lastCastTime = -100f;
    private static float _lastCastDuration = 0f;

    // A cast is recorded at its release point, partway into the animation, so the next forced
    // cast starts that wind-up earlier than a full castDuration after it. The wind-up is measured
    // from each forced start to its release, as a fraction of the cast's duration, per skill.
    private static float _overrideStarted = 0f;
    private static readonly Dictionary<System.IntPtr, float> _windupFraction = new();
    private const float MaxWindupFraction = 0.8f;

    // Where the player was at the last cast, and how far they must move (with movement held)
    // before the next forced cast.
    private static Vector3 _lastCastPosition = Vector3.zero;
    private const float MinMoveBetweenCasts = 0.5f;

    private static bool AnyMode() =>
        ModSettings.SkillsMana.CastWhenOutOfMana.Value
        || ModSettings.SkillsMana.AllowNegativeMana.Value;

    private static BaseMana PlayerMana() =>
        Refs_Manager.player_actor.IsNullOrDestroyed() ? null : Refs_Manager.player_actor.mana;

    /// <summary>Whether the player is giving movement input (keyboard, mouse or stick).</summary>
    private static bool MovementHeld()
    {
        EpochInputManager input = Refs_Manager.epoch_input_manager;
        if (input.IsNullOrDestroyed())
        {
            return false;
        }
        return input.isKeyboardMovementButtonHeld
            || input.isMouseMovementButtonHeld
            || input.input.sqrMagnitude > 0.01f;
    }

    private static float WindupFraction(Ability ability) =>
        _windupFraction.TryGetValue(ability.Pointer, out float fraction) ? fraction : 0f;

    private static bool IsShort(BaseMana mana, float cost) =>
        !mana.IsNullOrDestroyed() && mana.currentMana < cost;

    /// <summary>The cast's real mana cost, from the game's own BaseMana.getManaCost.</summary>
    private static float CostOf(BaseMana mana, Ability ability)
    {
        try
        {
            return mana.IsNullOrDestroyed() || ability.IsNullOrDestroyed()
                ? 0f
                : mana.getManaCost(ability);
        }
        catch (System.Exception)
        {
            return 0f;
        }
    }

    private static bool IsPlayer(UsingAbility usingAbility)
    {
        BaseMana player = PlayerMana();
        BaseMana mana = usingAbility.baseMana;
        return !player.IsNullOrDestroyed()
            && !mana.IsNullOrDestroyed()
            && player.Pointer == mana.Pointer;
    }

    [HarmonyPatch(typeof(UsingAbilityPlayer), nameof(UsingAbilityPlayer.InitialiseAbilityUse))]
    public class UsingAbilityPlayer_InitialiseAbilityUse
    {
        [HarmonyPrefix]
        static void Prefix(UsingAbilityPlayer __instance, Ability __0, Vector3 __1, ref bool __2)
        {
            try
            {
                BaseMana mana = __instance.baseMana;
                if (mana.IsNullOrDestroyed())
                {
                    return;
                }
                ApplyCanGoNegative(mana);
                if (__2 || !AnyMode() || __0.IsNullOrDestroyed())
                {
                    return;
                }
                float cost = CostOf(mana, __0);
                if (!IsShort(mana, cost))
                {
                    return;
                }
                ChargeManager charges = __instance.chargeManager;
                if (!charges.IsNullOrDestroyed() && charges.onCoooldown(__0))
                {
                    return;
                }
                // The game's own check, which this bypasses, lets a held movement key win between
                // casts. While movement is held, the next forced cast waits until the character
                // has actually moved, so holding both alternates cast, move, cast at the game's
                // own pace. If movement is held but blocked (e.g. a wall), two cast lengths do.
                float sinceCast = Time.time - _lastCastTime;
                float untilNext = _lastCastDuration * (1f - WindupFraction(__0));
                bool ready = !__instance.midAbilityAnimation && sinceCast >= untilNext;
                bool moving = MovementHeld();
                if (ready && moving)
                {
                    bool hasMoved =
                        (__instance.transform.position - _lastCastPosition).sqrMagnitude
                        >= MinMoveBetweenCasts * MinMoveBetweenCasts;
                    ready = hasMoved || sinceCast >= _lastCastDuration * 2f;
                }
                if (!ready)
                {
                    return;
                }
                AbilityInfo info = __instance.mutatorManager?.getMutatorOrAbility(__0);
                if (
                    info == null
                    || !__instance.canUseAbilityChecksAfterManaAndCooldown(__0, info, __1)
                )
                {
                    return;
                }
                __2 = true;
                _overriddenAbility = __0.Pointer;
                _overrideStarted = Time.time;
                _overrideExpires = Time.time + 3f;
            }
            catch (System.Exception ex)
            {
                ErrorLog.Report(ex, "Skills_CastWhenOutOfMana");
            }
        }

        /// <summary>
        /// "Allow Negative Mana" turns the game's canGoNegative on for the player's mana; otherwise
        /// the character's own value (gear can grant it) is put back and left alone.
        /// </summary>
        private static void ApplyCanGoNegative(BaseMana mana)
        {
            if (ModSettings.SkillsMana.AllowNegativeMana.Value)
            {
                if (_managedMana != mana.Pointer)
                {
                    _managedMana = mana.Pointer;
                    _originalCanGoNegative = mana.canGoNegative;
                }
                mana.canGoNegative = true;
            }
            else if (_managedMana == mana.Pointer)
            {
                mana.canGoNegative = _originalCanGoNegative;
                _managedMana = System.IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Every player cast passes here: its time and duration pace later forced casts. For a cast
    /// the mod forced, the game's spend is skipped (useMana = false), and negative mode charges
    /// the cost afterwards through the game's reduceMana.
    /// </summary>
    [HarmonyPatch(typeof(UsingAbility), nameof(UsingAbility.UseAbility))]
    public class UsingAbility_UseAbility
    {
        [HarmonyPrefix]
        static void Prefix(UsingAbility __instance, Ability __0, ref bool __2, out float __state)
        {
            // The cost negative mode still has to charge after the cast; 0 = nothing to charge.
            __state = 0f;
            if (__0.IsNullOrDestroyed() || !IsPlayer(__instance))
            {
                return;
            }
            bool forced = _overriddenAbility == __0.Pointer && Time.time <= _overrideExpires;
            if (!forced)
            {
                return;
            }
            MeasureWindup(__0);
            if (!__2 || !AnyMode())
            {
                return;
            }
            _overriddenAbility = System.IntPtr.Zero;
            __2 = false;
            if (ModSettings.SkillsMana.AllowNegativeMana.Value)
            {
                __state = CostOf(__instance.baseMana, __0);
            }
        }

        /// <summary>
        /// The time from the forced start to this release is the cast's wind-up. Values at or
        /// above the cast's duration aren't a wind-up (the cast was held back), so they're ignored.
        /// </summary>
        private static void MeasureWindup(Ability ability)
        {
            float windup = Time.time - _overrideStarted;
            if (_lastCastDuration > 0f && windup >= 0f && windup < _lastCastDuration)
            {
                _windupFraction[ability.Pointer] = Mathf.Min(
                    windup / _lastCastDuration,
                    MaxWindupFraction
                );
            }
        }

        [HarmonyPostfix]
        static void Postfix(UsingAbility __instance, Ability __0, float __state)
        {
            if (__0.IsNullOrDestroyed() || !IsPlayer(__instance))
            {
                return;
            }
            // The cast's duration is set once it has started; pace the next forced cast by it.
            _lastCastTime = Time.time;
            _lastCastDuration = __instance.castDuration;
            _lastCastPosition = __instance.transform.position;
            if (__state <= 0f)
            {
                return;
            }
            try
            {
                BaseMana mana = __instance.baseMana;
                if (!mana.IsNullOrDestroyed())
                {
                    mana.reduceMana(__state);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLog.Report(ex, "Skills_CastWhenOutOfMana");
            }
        }
    }
}
