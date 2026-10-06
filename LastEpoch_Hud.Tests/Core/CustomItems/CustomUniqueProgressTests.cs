using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomUniqueProgressTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Next_NotReady_Waits(bool addsBase)
    {
        var progress = new CustomUniqueProgress(addsBase);

        Assert.Equal(CustomUniqueStep.Wait, progress.Next(false));
    }

    [Fact]
    public void Next_WithBase_StartsWithAddBase()
    {
        var progress = new CustomUniqueProgress(true);

        Assert.Equal(CustomUniqueStep.AddBase, progress.Next(true));
    }

    [Fact]
    public void Next_WithoutBase_StartsWithAddUnique()
    {
        var progress = new CustomUniqueProgress(false);

        Assert.Equal(CustomUniqueStep.AddUnique, progress.Next(true));
    }

    [Fact]
    public void Next_WithoutCompleting_RepeatsSameStep()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Next(true);

        Assert.Equal(CustomUniqueStep.AddBase, progress.Next(true));
    }

    [Fact]
    public void Complete_WalksStepsInOrder()
    {
        var progress = new CustomUniqueProgress(true);
        var steps = new List<CustomUniqueStep>();

        for (int i = 0; i < 4; i++)
        {
            steps.Add(progress.Next(true));
            progress.Complete();
        }

        Assert.Equal(
            new[]
            {
                CustomUniqueStep.AddBase,
                CustomUniqueStep.AddUnique,
                CustomUniqueStep.AddToDictionary,
                CustomUniqueStep.Done,
            },
            steps
        );
    }

    [Fact]
    public void Complete_WithoutBase_SkipsAddBase()
    {
        var progress = new CustomUniqueProgress(false);
        progress.Complete();

        Assert.Equal(CustomUniqueStep.AddToDictionary, progress.Next(true));
    }

    [Fact]
    public void Next_AfterDone_StaysDoneWhenNotReady()
    {
        CustomUniqueProgress progress = Done();

        Assert.Equal(CustomUniqueStep.Done, progress.Next(false));
    }

    [Fact]
    public void Complete_AfterDone_StaysDone()
    {
        CustomUniqueProgress progress = Done();
        progress.Complete();

        Assert.Equal(CustomUniqueStep.Done, progress.Next(true));
    }

    [Fact]
    public void Fail_IsFinal()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Fail();
        progress.Complete();

        Assert.Equal(CustomUniqueStep.Failed, progress.Next(true));
    }

    [Fact]
    public void Next_AfterFailWhenNotReady_StaysFailed()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Fail();

        Assert.Equal(CustomUniqueStep.Failed, progress.Next(false));
    }

    [Fact]
    public void IsFinished_NewProgress_IsFalse()
    {
        Assert.False(new CustomUniqueProgress(true).IsFinished);
    }

    [Fact]
    public void IsFinished_MidWay_IsFalse()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Complete();

        Assert.False(progress.IsFinished);
    }

    [Fact]
    public void IsFinished_AfterDone_IsTrue()
    {
        CustomUniqueProgress progress = Done();

        Assert.True(progress.IsFinished);
    }

    [Fact]
    public void IsFinished_AfterFail_IsTrue()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Fail();

        Assert.True(progress.IsFinished);
    }

    [Fact]
    public void Next_ReadyAfterWait_ResumesPendingStep()
    {
        var progress = new CustomUniqueProgress(true);
        progress.Complete();

        Assert.Equal(CustomUniqueStep.Wait, progress.Next(false));
        Assert.Equal(CustomUniqueStep.AddUnique, progress.Next(true));
    }

    [Fact]
    public void Fail_AfterDone_StaysDone()
    {
        CustomUniqueProgress progress = Done();
        progress.Fail();

        Assert.Equal(CustomUniqueStep.Done, progress.Next(true));
    }

    [Fact]
    public void RunPending_StepDoesNotAdvance_StopsAfterOneCall()
    {
        var progress = new CustomUniqueProgress(true);
        int calls = 0;

        progress.RunPending(true, _ => calls++);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void RunPending_AllStepsComplete_RunsThreeStepsInOrder()
    {
        var progress = new CustomUniqueProgress(true);
        var steps = new List<CustomUniqueStep>();

        progress.RunPending(
            true,
            step =>
            {
                steps.Add(step);
                progress.Complete();
            }
        );

        Assert.Equal(
            new[]
            {
                CustomUniqueStep.AddBase,
                CustomUniqueStep.AddUnique,
                CustomUniqueStep.AddToDictionary,
            },
            steps
        );
        Assert.True(progress.IsFinished);
    }

    [Fact]
    public void RunPending_StepFails_StopsAtFailed()
    {
        var progress = new CustomUniqueProgress(true);
        int calls = 0;

        progress.RunPending(
            true,
            _ =>
            {
                calls++;
                progress.Fail();
            }
        );

        Assert.Equal(1, calls);
        Assert.Equal(CustomUniqueStep.Failed, progress.Next(true));
    }

    [Fact]
    public void RunPending_NotReady_RunsNothing()
    {
        var progress = new CustomUniqueProgress(true);
        int calls = 0;

        progress.RunPending(false, _ => calls++);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void RunPending_StepThrows_ExceptionPropagates()
    {
        var progress = new CustomUniqueProgress(true);

        Assert.Throws<InvalidOperationException>(() =>
            progress.RunPending(true, _ => throw new InvalidOperationException())
        );
    }

    private static CustomUniqueProgress Done()
    {
        var progress = new CustomUniqueProgress(false);
        progress.Complete();
        progress.Complete();
        return progress;
    }
}
