namespace LastEpoch_Hud.Scripts.Core.Diagnostics;

/// <summary>Builds the lines the error log writes.</summary>
public static class ErrorLogText
{
    public static string Occurrence(string context, string details)
    {
        string text = string.IsNullOrEmpty(details) ? "(no details)" : details;
        return context + ": " + text;
    }

    public static string Repeated(string context, int count)
    {
        return context + ": previous error repeated " + count + " times (suppressed)";
    }
}
