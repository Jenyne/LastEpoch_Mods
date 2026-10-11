namespace LastEpoch_Hud.Scripts;

/// <summary>Clamps legacy x1–x10 point multipliers.</summary>
public static class SettingRow
{
    public static int Clamp(float value)
    {
        int multiplier = (int)value;
        if (multiplier < 1)
        {
            return 2;
        }
        if (multiplier > 10)
        {
            return 10;
        }
        return multiplier;
    }
}
