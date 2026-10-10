using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Probe;

/// <summary>Debug lines for the cutscene/boss-intro probe.</summary>
public static class HeadhunterProbeLog
{
    public const string Prefix = "HH probe: ";

    public static string Flags(string scene, HeadhunterProbeFlags flags, double prevHeldSeconds)
    {
        return $"{Prefix}flags scene={Text(scene)} cinematic={YesNo(flags.Cinematic)} "
            + $"interruptible={YesNo(flags.InterruptibleCinematic)} inputOff={YesNo(flags.InputDisabled)} "
            + $"subtitles={YesNo(flags.Subtitles)} prevHeld={Seconds(prevHeldSeconds)}s";
    }

    public static string EmergeStart(
        string scene,
        string actor,
        KillKind kind,
        float durationSeconds
    )
    {
        return $"{Prefix}emerge start scene={Text(scene)} actor={Text(actor)} kind={Kind(kind)} "
            + $"duration={Seconds(durationSeconds)}s";
    }

    public static string EmergeEnd(
        string scene,
        string actor,
        KillKind kind,
        float spentSeconds,
        float durationSeconds,
        bool skipped
    )
    {
        return $"{Prefix}emerge end scene={Text(scene)} actor={Text(actor)} kind={Kind(kind)} "
            + $"spent={Seconds(spentSeconds)}s duration={Seconds(durationSeconds)}s skipped={YesNo(skipped)}";
    }

    public static string Cinematic(
        string scene,
        string source,
        float durationSeconds,
        bool interruptible,
        bool noMovementLock,
        bool noPlayerInvincibility
    )
    {
        return $"{Prefix}cinematic start scene={Text(scene)} source={Text(source)} "
            + $"duration={Seconds(durationSeconds)}s interruptible={YesNo(interruptible)} "
            + $"moveLock={YesNo(!noMovementLock)} invincible={YesNo(!noPlayerInvincibility)}";
    }

    public static string Cutscene(string scene, string id, float durationSeconds)
    {
        return $"{Prefix}cutscene start scene={Text(scene)} id={Text(id)} "
            + $"duration={Seconds(durationSeconds)}s";
    }

    public static string BossEngaged(
        string scene,
        string actor,
        bool calledToArms,
        bool diedBeforeActivating
    )
    {
        return $"{Prefix}boss engaged scene={Text(scene)} actor={Text(actor)} "
            + $"calledToArms={YesNo(calledToArms)} diedBefore={YesNo(diedBeforeActivating)}";
    }

    private static string Seconds(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string YesNo(bool value)
    {
        return value ? "yes" : "no";
    }

    private static string Text(string value)
    {
        return string.IsNullOrEmpty(value) ? "?" : value;
    }

    private static string Kind(KillKind kind)
    {
        return kind.ToString().ToLowerInvariant();
    }
}
