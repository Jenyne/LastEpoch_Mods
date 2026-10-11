using Iced.Intel;

namespace LastEpoch_Hud.Tests.Patches;

public sealed class HookSafetyTests
{
    public static TheoryData<byte[]> ShortBranches =>
        new() { Pad(0xEB, 0x02), Pad(0xE2, 0xFE), Pad(0xE3, 0x02), Pad(0x67, 0xE3, 0x02) };

    [Fact]
    public void ShortJccInDetour_Flagged()
    {
        byte[] code = { 0x83, 0x79, 0x40, 0x01, 0x75, 0x04, 0xC6, 0x41, 0x50, 0x01, 0xC3 };

        bool found = HookSafety.TryFindShortBranch(code, out Instruction branch);

        Assert.True(found);
        Assert.Equal(4UL, branch.IP);
        Assert.Equal(Mnemonic.Jne, branch.Mnemonic);
    }

    [Fact]
    public void NearJccInDetour_NotFlagged()
    {
        byte[] code = { 0x48, 0x85, 0xC9, 0x0F, 0x85, 0x10, 0x00, 0x00, 0x00, 0xC3 };

        Assert.False(HookSafety.TryFindShortBranch(code, out _));
    }

    [Theory]
    [MemberData(nameof(ShortBranches))]
    public void ShortBranchKinds_Flagged(byte[] code)
    {
        bool found = HookSafety.TryFindShortBranch(code, out Instruction branch);

        Assert.True(found);
        Assert.Equal(0UL, branch.IP);
    }

    [Fact]
    public void ShortBranchAtLastDetourByte_Flagged()
    {
        byte[] code = { 0x48, 0x89, 0x5C, 0x24, 0x08, 0x75, 0x02, 0x90, 0x90 };

        bool found = HookSafety.TryFindShortBranch(code, out Instruction branch);

        Assert.True(found);
        Assert.Equal(5UL, branch.IP);
        Assert.Equal(Mnemonic.Jne, branch.Mnemonic);
    }

    [Fact]
    public void ShortBranchAfterDetour_NotFlagged()
    {
        byte[] code = { 0x48, 0x89, 0x5C, 0x24, 0x08, 0x90, 0x75, 0x02, 0x90, 0x90 };

        Assert.False(HookSafety.TryFindShortBranch(code, out _));
    }

    [Fact]
    public void TruncatedCode_NotFlagged()
    {
        byte[] code = { 0xC3 };

        Assert.False(HookSafety.TryFindShortBranch(code, out _));
    }

    private static byte[] Pad(params byte[] head)
    {
        byte[] code = Enumerable.Repeat((byte)0x90, 8).ToArray();
        head.CopyTo(code, 0);
        return code;
    }
}
