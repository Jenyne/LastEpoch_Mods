using LastEpoch_Hud.Scripts.Core.Skills;

namespace LastEpoch_Hud.Tests.Core.Skills;

public sealed class FakeFormClassifier
{
    private readonly FormDrainVerdict[] _verdicts;

    public FakeFormClassifier(params FormDrainVerdict[] verdicts)
    {
        _verdicts = verdicts;
    }

    public int Calls { get; private set; }

    public int LastSource { get; private set; }

    public FormDrainVerdict Classify(int source)
    {
        int index = Math.Min(Calls, _verdicts.Length - 1);
        Calls++;
        LastSource = source;
        return _verdicts[index];
    }
}
