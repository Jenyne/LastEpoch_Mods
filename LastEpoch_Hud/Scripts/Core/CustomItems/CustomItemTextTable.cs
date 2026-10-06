using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Maps game localization keys to custom item text resolvers.</summary>
public sealed class CustomItemTextTable
{
    private readonly Dictionary<string, Func<string, string>> _resolvers = new(
        StringComparer.Ordinal
    );

    public void Register(string key, Func<string, string> resolve)
    {
        if (string.IsNullOrEmpty(key) || resolve == null)
        {
            return;
        }

        _resolvers[key] = resolve;
    }

    public void Register(string key, LocalizedText text)
    {
        if (text == null)
        {
            return;
        }

        Register(key, text.For);
    }

    public string Resolve(string key, string language)
    {
        if (key == null || !_resolvers.TryGetValue(key, out Func<string, string> resolve))
        {
            return null;
        }

        string text = resolve(language);
        return string.IsNullOrEmpty(text) ? null : text;
    }
}
