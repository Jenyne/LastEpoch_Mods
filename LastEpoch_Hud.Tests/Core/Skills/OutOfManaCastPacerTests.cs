using LastEpoch_Hud.Scripts.Core.Skills;

namespace LastEpoch_Hud.Tests.Core.Skills;

public sealed class OutOfManaCastPacerTests
{
    private const long Heartseeker = 1;
    private const long ShadowRend = 2;

    // Times and durations are exact in binary floating point, so ready/not-ready edges are exact.
    private const float CastDuration = 0.25f;

    [Fact]
    public void IsReady_NoCastYet_IsReady()
    {
        var pacer = new OutOfManaCastPacer();

        Assert.True(pacer.IsReady(Heartseeker, 5f, false, false, 0f));
    }

    [Fact]
    public void IsReady_NoWindupMeasured_WaitsAFullCastDuration()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);

        Assert.False(pacer.IsReady(Heartseeker, 10.125f, false, false, 0f));
        Assert.True(pacer.IsReady(Heartseeker, 10.25f, false, false, 0f));
    }

    [Fact]
    public void IsReady_MidAnimation_IsNeverReady()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);

        Assert.False(pacer.IsReady(Heartseeker, 20f, true, false, 0f));
    }

    [Fact]
    public void ForcedCasts_KeepTheNormalCastCadence()
    {
        // Regression: pacing a full castDuration from each release added the wind-up to every
        // cast (Heartseeker 291 ms apart instead of 208 ms). With the wind-up measured, forced
        // casts release exactly one castDuration apart, like the game's own casts.
        var pacer = new OutOfManaCastPacer();
        const float windup = 0.125f;
        float release = 10f;
        pacer.CastReleased(release, CastDuration);

        for (int i = 0; i < 5; i++)
        {
            float start = FirstReadyTime(pacer, Heartseeker, release);
            pacer.ForcedCastStarted(start);
            float next = start + windup;
            pacer.ForcedCastReleased(Heartseeker, next);
            pacer.CastReleased(next, CastDuration);

            if (i > 0)
            {
                Assert.Equal(CastDuration, next - release, 3);
            }
            release = next;
        }
        Assert.Equal(0.5f, pacer.WindupFraction(Heartseeker));
    }

    [Fact]
    public void ForcedCastReleased_MeasuresWindupAsFractionOfPreviousCast()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);
        pacer.ForcedCastStarted(10.25f);

        pacer.ForcedCastReleased(Heartseeker, 10.3125f);

        Assert.Equal(0.25f, pacer.WindupFraction(Heartseeker));
        // The next cast may start a quarter of the cast earlier: 0.1875 s after the release.
        pacer.CastReleased(10.3125f, CastDuration);
        Assert.False(pacer.IsReady(Heartseeker, 10.484375f, false, false, 0f));
        Assert.True(pacer.IsReady(Heartseeker, 10.5f, false, false, 0f));
    }

    [Fact]
    public void ForcedCastReleased_DelayAtOrAboveCastDuration_IsIgnored()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);
        pacer.ForcedCastStarted(10.25f);
        pacer.ForcedCastReleased(Heartseeker, 10.375f);

        // The cast was held back for a full cast length: not a wind-up, keep the last one.
        pacer.ForcedCastStarted(11f);
        pacer.ForcedCastReleased(Heartseeker, 11.25f);

        Assert.Equal(0.5f, pacer.WindupFraction(Heartseeker));
    }

    [Fact]
    public void ForcedCastReleased_NoPreviousCast_MeasuresNothing()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.ForcedCastStarted(10f);

        pacer.ForcedCastReleased(Heartseeker, 10.125f);

        Assert.Equal(0f, pacer.WindupFraction(Heartseeker));
    }

    [Fact]
    public void ForcedCastReleased_LongWindup_IsCapped()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, 1f);
        pacer.ForcedCastStarted(10f);

        pacer.ForcedCastReleased(Heartseeker, 10.9375f);

        Assert.Equal(OutOfManaCastPacer.MaxWindupFraction, pacer.WindupFraction(Heartseeker));
    }

    [Fact]
    public void WindupFraction_IsPerSkill()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);
        pacer.ForcedCastStarted(10.25f);
        pacer.ForcedCastReleased(Heartseeker, 10.375f);
        pacer.CastReleased(10.375f, CastDuration);

        Assert.Equal(0f, pacer.WindupFraction(ShadowRend));
        Assert.True(pacer.IsReady(Heartseeker, 10.5f, false, false, 0f));
        Assert.False(pacer.IsReady(ShadowRend, 10.5f, false, false, 0f));
    }

    [Fact]
    public void IsReady_MovementHeldWithoutMoving_WaitsTwoCastLengths()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);

        Assert.False(pacer.IsReady(Heartseeker, 10.25f, false, true, 0f));
        Assert.False(pacer.IsReady(Heartseeker, 10.375f, false, true, 0.25f));
        Assert.True(pacer.IsReady(Heartseeker, 10.5f, false, true, 0f));
    }

    [Fact]
    public void IsReady_MovementHeldAndMoved_CastsAtTheNormalTime()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);

        bool moved = pacer.IsReady(
            Heartseeker,
            10.25f,
            false,
            true,
            OutOfManaCastPacer.MinMoveBetweenCasts
        );

        Assert.True(moved);
    }

    [Fact]
    public void IsReady_MovementHeldAndMoved_StillWaitsForTheCast()
    {
        var pacer = new OutOfManaCastPacer();
        pacer.CastReleased(10f, CastDuration);

        Assert.False(pacer.IsReady(Heartseeker, 10.125f, false, true, 5f));
    }

    private static float FirstReadyTime(OutOfManaCastPacer pacer, long ability, float from)
    {
        const float step = 1f / 1024f;
        float now = from;
        while (!pacer.IsReady(ability, now, false, false, 0f))
        {
            now += step;
        }
        return now;
    }
}
