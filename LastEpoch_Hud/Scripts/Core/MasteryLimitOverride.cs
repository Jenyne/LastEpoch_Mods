namespace LastEpoch_Hud.Scripts.Core;

// Own only the temporary cap we wrote; game reinitialization supplies a new baseline.
internal sealed class MasteryLimitOverride
{
    public bool Active { get; private set; }
    private byte original;
    private byte applied;

    public byte Sync(byte current, byte fullLimit, bool enabled)
    {
        if (!enabled || current == 0 || fullLimit == 0)
        {
            byte restored = Active && current == applied ? original : current;
            Active = false;
            return restored;
        }
        if (Active && current != applied)
            Active = false;
        if (!Active)
        {
            if (current >= fullLimit)
                return current;
            original = current;
            Active = true;
        }
        applied = fullLimit;
        return applied;
    }
}
