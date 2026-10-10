using LastEpoch_Hud.Scripts.Core.Skills;

namespace LastEpoch_Hud.Tests.Core.Skills;

public sealed class FormDrainGateTests
{
    private const string Scene = "scene";
    private const int Id = 7;

    [Fact]
    public void ShouldSkip_SkipVerdict_SkipsAndClassifiesOnce()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Skip);

        bool first = gate.ShouldSkip(Scene, Id, 42, classifier.Classify);
        bool second = gate.ShouldSkip(Scene, Id, 42, classifier.Classify);

        Assert.True(first);
        Assert.True(second);
        Assert.Equal(1, classifier.Calls);
        Assert.Equal(42, classifier.LastSource);
    }

    [Fact]
    public void ShouldSkip_KeepVerdict_DoesNotSkipAndClassifiesOnce()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Keep);

        bool first = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);
        bool second = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);

        Assert.False(first);
        Assert.False(second);
        Assert.Equal(1, classifier.Calls);
    }

    [Fact]
    public void ShouldSkip_NotReadyVerdict_DoesNotSkipAndIsNotCached()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.NotReady);

        bool first = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);
        bool second = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);

        Assert.False(first);
        Assert.False(second);
        Assert.Equal(2, classifier.Calls);
    }

    [Fact]
    public void ShouldSkip_NotReadyThenSkip_SkipsAndCaches()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.NotReady, FormDrainVerdict.Skip);

        bool notReady = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);
        bool skipped = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);
        bool cached = gate.ShouldSkip(Scene, Id, 0, classifier.Classify);

        Assert.False(notReady);
        Assert.True(skipped);
        Assert.True(cached);
        Assert.Equal(2, classifier.Calls);
    }

    [Fact]
    public void ShouldSkip_DifferentIds_ClassifiedSeparately()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Skip);

        gate.ShouldSkip(Scene, 1, 0, classifier.Classify);
        gate.ShouldSkip(Scene, 2, 0, classifier.Classify);

        Assert.Equal(2, classifier.Calls);
    }

    [Fact]
    public void Reset_ClearsCache()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Skip);
        gate.ShouldSkip(Scene, Id, 0, classifier.Classify);

        gate.Reset();
        gate.ShouldSkip(Scene, Id, 0, classifier.Classify);

        Assert.Equal(2, classifier.Calls);
    }

    [Fact]
    public void ShouldSkip_NewScene_ClearsCache()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Skip);
        gate.ShouldSkip("a", Id, 0, classifier.Classify);

        gate.ShouldSkip("b", Id, 0, classifier.Classify);

        Assert.Equal(2, classifier.Calls);
    }

    [Fact]
    public void ShouldSkip_RepeatCallInNewScene_UsesCache()
    {
        var gate = new FormDrainGate();
        var classifier = new FakeFormClassifier(FormDrainVerdict.Skip);
        gate.ShouldSkip("a", Id, 0, classifier.Classify);
        gate.ShouldSkip("b", Id, 0, classifier.Classify);

        gate.ShouldSkip("b", Id, 0, classifier.Classify);

        Assert.Equal(2, classifier.Calls);
    }
}
