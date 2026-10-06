using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // Public API consumed by feature code (e.g. Skills_AutoCast.IsModifierHeld).
    // Reads the tagged-string binding produced by KeybindSetting and tests whether
    // the corresponding input is currently held this frame
    public static class KeybindMatcher
    {
        public static bool IsHeld(string binding)
        {
            if (string.IsNullOrEmpty(binding)) return false;
            if (binding.StartsWith("kb:"))
            {
                foreach (var part in binding.Substring(3).Split('+'))
                    if (!System.Enum.TryParse(part, out KeyCode key) || !Input.GetKey(key))
                        return false;
                return true;
            }
            if (binding.StartsWith("gp:"))
                return KeybindRewired.ButtonHeldByName(binding.Substring(3));
            return false;
        }
    }
}
