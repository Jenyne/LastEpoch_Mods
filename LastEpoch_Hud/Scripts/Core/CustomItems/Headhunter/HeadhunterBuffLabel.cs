using System;
using System.Globalization;
using System.Text;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Tooltip text for one buff.</summary>
public static class HeadhunterBuffLabel
{
    public static string Format(
        string gameName,
        string enumName,
        float added,
        float increased,
        bool addedAsPercent,
        int stacks
    )
    {
        string name = string.IsNullOrEmpty(gameName) ? StatNameWords.Split(enumName) : gameName;
        StringBuilder text = new(name);
        AppendValue(text, addedAsPercent ? added * 100f : added, addedAsPercent);
        AppendValue(text, increased * 100f, true);
        if (stacks > 1)
        {
            text.Append(" (x").Append(stacks).Append(')');
        }

        return text.ToString();
    }

    private static void AppendValue(StringBuilder text, float value, bool percent)
    {
        if (value == 0f)
        {
            return;
        }

        text.Append(' ').Append(value < 0f ? '-' : '+');
        text.Append(Math.Abs(value).ToString("0.##", CultureInfo.InvariantCulture));
        if (percent)
        {
            text.Append('%');
        }
    }
}
