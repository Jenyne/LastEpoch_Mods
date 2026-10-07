using System;
using System.Text.RegularExpressions;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

public sealed class MaxrollLink
{
    public string BuildId { get; }
    public string Fragment { get; }
    public Uri CanonicalUri { get; }
    public Uri ProfileEndpoint => new Uri("https://planners.maxroll.gg/profiles/le/" + BuildId);
    public Uri LoaderEndpoint =>
        new Uri(
            "https://maxroll.gg/last-epoch/planner/" + BuildId + "?_data=last-epoch-planner-by-id"
        );

    private MaxrollLink(string id, string fragment)
    {
        BuildId = id;
        Fragment = fragment;
        CanonicalUri = new Uri(
            "https://maxroll.gg/last-epoch/planner/"
                + id
                + (fragment.Length == 0 ? "" : "#" + Uri.EscapeDataString(fragment))
        );
    }

    public static MaxrollLink Parse(string input)
    {
        if (
            string.IsNullOrWhiteSpace(input)
            || input.Length > 2048
            || !Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || (uri.Host != "maxroll.gg" && uri.Host != "www.maxroll.gg")
            || !uri.IsDefaultPort
            || uri.UserInfo.Length != 0
        )
            throw new FormatException("Use an HTTPS Maxroll Last Epoch planner link.");
        var match = Regex.Match(
            uri.AbsolutePath,
            "^/last-epoch/planner/([A-Za-z0-9]{1,64})/?$",
            RegexOptions.CultureInvariant
        );
        if (!match.Success)
            throw new FormatException(
                "Open the guide's Last Epoch planner and copy its build link."
            );
        string fragment = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
        if (fragment.Length > 256 || Regex.IsMatch(fragment, "[\\x00-\\x1f\\x7f]"))
            throw new FormatException("The link's gear variant is invalid.");
        return new MaxrollLink(match.Groups[1].Value, fragment);
    }
}
