using Iced.Intel;

namespace LastEpoch_Hud.Tests.Patches;

/// <summary>Finds code that MelonLoader's detour would break.</summary>
internal static class HookSafety
{
    public const int DetourSize = 6;
    public const int CodeBytes = 32;

    public static bool TryFindShortBranch(byte[] code, out Instruction branch)
    {
        var decoder = Decoder.Create(64, new ByteArrayCodeReader(code), 0UL);
        while (decoder.IP < DetourSize)
        {
            decoder.Decode(out Instruction instruction);
            if (instruction.Code == Code.INVALID)
            {
                break;
            }

            if (IsShortBranch(instruction))
            {
                branch = instruction;
                return true;
            }
        }
        branch = default;
        return false;
    }

    private static bool IsShortBranch(Instruction instruction) =>
        instruction.IsJccShort
        || instruction.IsJmpShort
        || instruction.IsLoop
        || instruction.IsLoopcc
        || instruction.IsJcxShort;
}
