using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Defaults;

public sealed class HeadhunterAffixDefaultsTests
{
    private const int ExpectedCount = 38;

    [Fact]
    public void AffixDefaults_Are38_UniqueKeys()
    {
        var keys = HeadhunterAffixDefaults
            .VersionedAffixes.Select(row => row.Entry.ModKey)
            .ToList();

        Assert.Equal(ExpectedCount, keys.Count);
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void AffixDefaults_AllSinceCurrent()
    {
        Assert.Equal(6, HeadhunterAffixDefaults.Since);
        Assert.All(
            HeadhunterAffixDefaults.VersionedAffixes,
            row => Assert.Equal(HeadhunterAffixDefaults.Since, row.Since)
        );
        Assert.True(HeadhunterConfigDefaults.DefaultsVersion >= HeadhunterAffixDefaults.Since);
    }

    [Fact]
    public void AffixDefaults_HaveNoteAndRows()
    {
        Assert.All(
            HeadhunterAffixDefaults.VersionedAffixes,
            row =>
            {
                Assert.False(string.IsNullOrWhiteSpace(row.Entry.Note));
                Assert.NotEmpty(row.Entry.Rows);
            }
        );
    }

    [Fact]
    public void AffixDefaults_MapEqualsVersionedInOrder()
    {
        Assert.Equal(
            HeadhunterAffixDefaults.VersionedAffixes.Select(row => row.Entry.ModKey),
            HeadhunterAffixDefaults.AffixMap.Select(entry => entry.ModKey)
        );
    }

    [Fact]
    public void AffixDefaults_RowsResolveAgainstDefaultStats()
    {
        Dictionary<string, int> statIds = Ids(
            HeadhunterConfigDefaults.Stats.Select(entry => entry.Stat)
        );
        Dictionary<string, int> tagIds = Ids(
            HeadhunterConfigDefaults
                .Stats.Where(entry => entry.Tag != null)
                .Select(entry => entry.Tag)
        );
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig resolved = HeadhunterConfigResolver.Resolve(
            HeadhunterConfigDefaults.Config,
            statIds,
            tagIds,
            problems
        );

        Assert.Empty(problems);
        Assert.Equal(ExpectedCount, resolved.AffixCount);
        Assert.All(
            HeadhunterAffixDefaults.AffixMap,
            entry =>
            {
                Assert.True(resolved.TryGetAffixRows(entry.ModKey, out IReadOnlyList<int> rows));
                Assert.Equal(entry.Rows.Count, rows.Count);
            }
        );
    }

    [Fact]
    public void Config_AffixMap_IsDefaultMap()
    {
        Assert.Same(HeadhunterAffixDefaults.AffixMap, HeadhunterConfigDefaults.Config.AffixMap);
    }

    [Fact]
    public void MergeDefaults_Affixes_AreVersionedAffixes()
    {
        Assert.Same(
            HeadhunterAffixDefaults.VersionedAffixes,
            HeadhunterConfigDefaults.MergeDefaults.Affixes
        );
    }

    [Fact]
    public void AffixRows_AllSinceRowsSince()
    {
        Assert.Equal(8, HeadhunterAffixDefaults.RowsSince);
        Assert.True(HeadhunterConfigDefaults.DefaultsVersion >= HeadhunterAffixDefaults.RowsSince);
        Assert.Equal(8, HeadhunterAffixDefaults.VersionedRows.Count);
        Assert.All(
            HeadhunterAffixDefaults.VersionedRows,
            row => Assert.Equal(HeadhunterAffixDefaults.RowsSince, row.Since)
        );
    }

    [Fact]
    public void AffixRows_UniqueAndInTheirDefaultEntry()
    {
        var pairs = HeadhunterAffixDefaults
            .VersionedRows.Select(row => (row.ModKey, row.Row))
            .ToList();

        Assert.Equal(pairs.Count, pairs.Distinct().Count());
        Assert.All(
            HeadhunterAffixDefaults.VersionedRows,
            row =>
            {
                HeadhunterAffixEntry entry = HeadhunterAffixDefaults.AffixMap.Single(candidate =>
                    candidate.ModKey == row.ModKey
                );
                Assert.Contains(row.Row, entry.Rows);
            }
        );
    }

    [Fact]
    public void AffixRows_NameDefaultStatRows()
    {
        Assert.All(
            HeadhunterAffixDefaults.VersionedRows,
            row =>
                Assert.Contains(
                    HeadhunterConfigDefaults.Config.Stats,
                    stat => stat.RowText == row.Row
                )
        );
    }

    [Fact]
    public void MergeDefaults_AffixRows_AreVersionedRows()
    {
        Assert.Same(
            HeadhunterAffixDefaults.VersionedRows,
            HeadhunterConfigDefaults.MergeDefaults.AffixRows
        );
    }

    private static Dictionary<string, int> Ids(IEnumerable<string> names)
    {
        return names
            .Distinct(StringComparer.Ordinal)
            .Select((name, index) => (name, id: index + 1))
            .ToDictionary(pair => pair.name, pair => pair.id, StringComparer.Ordinal);
    }
}
