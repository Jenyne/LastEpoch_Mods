using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using LastEpoch_Hud.Scripts.Core.BuildImport;

if (args.Length == 0 || args[0] == "--help")
{
    Console.WriteLine("Read-only Maxroll build inspection; no game installation required.");
    Console.WriteLine(
        "dotnet run --project LastEpoch_Maxroll -- <planner-url> [--variant 1] [--output folder]"
    );
    Console.WriteLine(
        "dotnet run --project LastEpoch_Maxroll -- --file clipboard-or-response.json [--variant 1] [--output folder]"
    );
    return args.Length == 0 ? 1 : 0;
}

try
{
    int offset = args[0] == "--file" ? 2 : 1;
    if (args.Length < offset)
        throw new ArgumentException("Supply a JSON file after --file.");
    int? variant = null;
    string folder = null;
    for (int i = offset; i < args.Length; i += 2)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException("Missing option value.");
        if (args[i] == "--variant" && int.TryParse(args[i + 1], out int n) && n > 0)
            variant = n - 1;
        else if (args[i] == "--output")
            folder = args[i + 1];
        else
            throw new ArgumentException("Unknown option or invalid variant: " + args[i]);
    }
    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellation.Cancel();
    };
    MaxrollBuild build;
    if (args[0] == "--file")
    {
        if (new FileInfo(args[1]).Length > MaxrollBuildParser.MaximumBytes)
            throw new FormatException("The input file exceeds the 4 MiB limit.");
        build = MaxrollBuildParser.Parse(await File.ReadAllTextAsync(args[1], cancellation.Token));
    }
    else
    {
        using var client = new MaxrollBuildClient();
        build = await client.RetrieveAsync(args[0], cancellation.Token);
    }
    if (variant.HasValue)
        build.SelectVariant(variant.Value);
    Console.WriteLine(build.Name);
    for (int i = 0; i < build.Variants.Count; i++)
    {
        var v = build.Variants[i];
        Console.WriteLine(
            $"{(i == build.SelectedVariantIndex ? "*" : " ")} {i + 1}: {v.Name} ({v.Kind}; {v.Placements.Count(p => p.Item.HasValue)} items; {v.Issues.Count} issues)"
        );
    }
    var selected = build.SelectedVariant;
    var treeIssues = selected == null ? Array.Empty<string>() : TreeIssues(selected.Trees);
    if (selected != null)
    {
        foreach (var p in selected.Placements.Where(p => !p.IsEmpty))
        {
            if (!p.Item.HasValue)
            {
                Console.WriteLine($"  {p.Section}/{p.Slot}: UNRESOLVED reference {p.Reference}");
                continue;
            }
            var item = p.Item.Value;
            string Field(string key) =>
                item.TryGetProperty(key, out var value) ? value.GetRawText() : "absent";
            Console.WriteLine(
                $"  {p.Section}/{p.Slot}: base={Field("itemType")} subtype={Field("subType")} unique={Field("uniqueID")}"
            );
        }
        if (selected.Trees.Passives != null)
            PrintTree(selected.Trees.Passives);
        foreach (var tree in selected.Trees.Skills)
            PrintTree(tree);
        Console.WriteLine(
            "  Active skill IDs: " + JsonSerializer.Serialize(selected.Trees.ActiveSkills)
        );
        Console.WriteLine(
            "  Specialized skill IDs: " + JsonSerializer.Serialize(selected.Trees.SpecializedSkills)
        );
    }
    folder ??= Path.Combine("BuildImports", build.Link?.BuildId ?? "clipboard");
    Directory.CreateDirectory(folder);
    var options = new JsonSerializerOptions { WriteIndented = true };
    await File.WriteAllTextAsync(
        Path.Combine(folder, "response.json"),
        build.ResponseJson,
        cancellation.Token
    );
    await File.WriteAllTextAsync(
        Path.Combine(folder, "build.json"),
        JsonSerializer.Serialize(build.Data, options),
        cancellation.Token
    );
    var report = new
    {
        schemaVersion = 1,
        capturedUtc = DateTime.UtcNow,
        source = build.Link?.CanonicalUri.ToString(),
        endpoint = build.SourceEndpoint?.ToString(),
        build.Name,
        build.SelectedVariantIndex,
        build.SelectionIssue,
        variants = build.Variants.Select(v => new
        {
            v.Name,
            v.Kind,
            v.SourceIndex,
            v.EmbedId,
            itemCount = v.Placements.Count(p => p.Item.HasValue),
            v.Issues,
            treeIssues = TreeIssues(v.Trees),
        }),
        selected,
        notes = new[]
        {
            "Read-only planner snapshot, not a game item or legal-item verdict.",
            "Normalized rolls are preserved as 0..1. No rounding, byte conversion or defaults are applied.",
            "Tree ranks are decoded at the saved history cursor. Numeric steps increment; object steps overwrite only the listed ranks.",
            "Planner tree/node IDs are not validated against game nodes, prerequisites, point caps or an executable leveling order.",
            "Full histories, future steps, notes and unknown fields remain in build.json and variant Data.",
            "Absent LP, Weaver's Will, forging potential or other game metadata remains absent.",
        },
    };
    await File.WriteAllTextAsync(
        Path.Combine(folder, "preview.json"),
        JsonSerializer.Serialize(report, options),
        cancellation.Token
    );
    if (build.SelectionIssue != null)
        Console.WriteLine(build.SelectionIssue);
    if (selected != null)
        foreach (string issue in selected.Issues)
            Console.WriteLine("Issue: " + issue);
    foreach (string issue in treeIssues)
        Console.WriteLine("Tree issue: " + issue);
    Console.WriteLine("Saved snapshot and preview to " + Path.GetFullPath(folder));
    return selected == null || selected.Issues.Count > 0 || treeIssues.Length > 0 ? 2 : 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Retrieval cancelled.");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static void PrintTree(MaxrollTreeSnapshot tree) =>
    Console.WriteLine(
        $"  Planner tree {tree.PlannerId}: cursor={tree.Position}/{tree.HistoryLength}; "
            + (
                tree.IsDecoded
                    ? $"{tree.TotalPoints} points in {tree.Ranks.Count} nodes"
                    : "history could not be decoded"
            )
    );

static string[] TreeIssues(MaxrollTreePreview preview) =>
    preview
        .Issues.Concat(
            preview.Passives?.Issues.Select(issue => "passives: " + issue) ?? Array.Empty<string>()
        )
        .Concat(
            preview.Skills.SelectMany(tree =>
                tree.Issues.Select(issue => tree.PlannerId + ": " + issue)
            )
        )
        .ToArray();
