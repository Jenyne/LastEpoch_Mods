using System.Text;

namespace LastEpoch_Hud.Scripts.Core.ForceDrop;

// Input.inputString may contain edit/navigation control characters. Only
// printable characters are transferred when a search field lacks focus.
// Once focused, TMP_InputField handles Backspace, Enter, IME and selection.
public static class ForceDropSearchTyping
{
    public static string Printable(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        var result = new StringBuilder(input.Length);
        foreach (char ch in input)
            if (!char.IsControl(ch))
                result.Append(ch);
        return result.ToString();
    }
}
