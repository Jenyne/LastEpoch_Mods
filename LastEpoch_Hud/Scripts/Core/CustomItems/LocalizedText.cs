using System;
using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>One text in English plus translations; other languages fall back to English.</summary>
public sealed class LocalizedText
{
    private const string EnglishCode = "en";

    private readonly Dictionary<string, string> _texts = new(StringComparer.Ordinal);

    public LocalizedText(string english, params (string Language, string Text)[] translations)
    {
        _texts[EnglishCode] = english;
        foreach ((string language, string text) in translations)
        {
            _texts[language] = text;
        }
    }

    public string For(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return "";
        }

        return _texts.TryGetValue(language, out string text) ? text : _texts[EnglishCode];
    }
}
