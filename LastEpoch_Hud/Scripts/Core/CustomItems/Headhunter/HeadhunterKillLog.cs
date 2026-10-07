using System;
using System.Collections.Generic;
using System.Text;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Builds the one-line debug text for a Headhunter kill.</summary>
public static class HeadhunterKillLog
{
    public static string Format(
        KillInfo kill,
        IReadOnlyList<BuffAction> actions,
        Func<int, string> statName
    )
    {
        var text = new StringBuilder("Headhunter kill: kind=");
        text.Append(kill.Kind).Append(" byMinion=").Append(kill.ByMinion).Append(" mods=[");
        AppendMods(text, kill.ModStatIds, statName);
        text.Append("] actions=[");
        AppendActions(text, actions);
        return text.Append(']').ToString();
    }

    private static void AppendMods(
        StringBuilder text,
        IReadOnlyList<int> ids,
        Func<int, string> statName
    )
    {
        if (ids == null)
        {
            return;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            AppendSeparator(text, i);
            text.Append(statName(ids[i]));
        }
    }

    private static void AppendActions(StringBuilder text, IReadOnlyList<BuffAction> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            AppendSeparator(text, i);
            text.Append(actions[i].Kind).Append(' ').Append(actions[i].BuffName);
        }
    }

    private static void AppendSeparator(StringBuilder text, int index)
    {
        if (index > 0)
        {
            text.Append(", ");
        }
    }
}
