using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Use whenever a URL needs to be encoded etc.
    /// Utilizes Uri.EscapeDataString
    /// </summary>
    /// <remarks>https://stackoverflow.com/questions/602642/server-urlencode-vs-httputility-urlencode/1148326#1148326</remarks>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToEscaped(this string? value)
    {
        if (value is null)
            return null;

        if (value.Length == 0)
            return "";

        return Uri.EscapeDataString(value);
    }

    /// <summary>
    /// Utilizes Uri.UnescapeDataString
    /// </summary>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToUnescaped(this string? value)
    {
        if (value is null)
            return null;

        if (value.Length == 0)
            return "";

        return Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Escapes and sanitizes a string for safe use within Scriban templates.
    /// </summary>
    /// <param name="input">The input string to sanitize. If <c>null</c> or whitespace, returns an empty string.</param>
    /// <returns>
    /// A cleaned string with the following transformations:
    /// <list type="bullet">
    ///   <item><description>Double curly braces (<c>{{</c> and <c>}}</c>) are removed to prevent template injection.</description></item>
    ///   <item><description>Double quotes (<c>"</c>) are replaced with single quotes (<c>'</c>).</description></item>
    ///   <item><description>Backslashes (<c>\</c>) are replaced with forward slashes (<c>/</c>).</description></item>
    ///   <item><description>Carriage returns and newlines are replaced with spaces.</description></item>
    ///   <item><description>Leading and trailing whitespace is trimmed.</description></item>
    /// </list>
    /// </returns>
    [Pure]
    public static string ToEscapedForScriban(this string? input)
    {
        if (input.IsNullOrWhiteSpace())
            return "";

        ReadOnlySpan<char> s = input;

        // Pass 1:
        // - remove "{{" and "}}"
        // - map chars
        // - trim leading/trailing whitespace after mapping
        // - preserve internal whitespace exactly
        var outLen = 0;
        var i = 0;
        var seenNonWs = false;
        var pendingWs = 0;

        while (i < s.Length)
        {
            char c = s[i];

            if (c == '{' && i + 1 < s.Length && s[i + 1] == '{')
            {
                i += 2;
                continue;
            }

            if (c == '}' && i + 1 < s.Length && s[i + 1] == '}')
            {
                i += 2;
                continue;
            }

            char mapped = c switch
            {
                '"' => '\'',
                '\\' => '/',
                '\r' => ' ',
                '\n' => ' ',
                _ => c
            };

            bool isWs = char.IsWhiteSpace(mapped);

            if (!seenNonWs)
            {
                if (isWs)
                {
                    i++;
                    continue;
                }

                seenNonWs = true;
                outLen++;
                i++;
                continue;
            }

            if (isWs)
            {
                pendingWs++;
            }
            else
            {
                outLen += pendingWs + 1;
                pendingWs = 0;
            }

            i++;
        }

        if (outLen == 0)
            return "";

        // Pass 2:
        // Same state machine as pass 1, but write directly.
        // No lookahead. No rescanning.
        return string.Create(outLen, input, static (dst, src) =>
        {
            ReadOnlySpan<char> s = src;
            var i = 0;
            var w = 0;
            var seenNonWs = false;
            var pendingWs = 0;

            while (i < s.Length)
            {
                char c = s[i];

                if (c == '{' && i + 1 < s.Length && s[i + 1] == '{')
                {
                    i += 2;
                    continue;
                }

                if (c == '}' && i + 1 < s.Length && s[i + 1] == '}')
                {
                    i += 2;
                    continue;
                }

                char mapped = c switch
                {
                    '"' => '\'',
                    '\\' => '/',
                    '\r' => ' ',
                    '\n' => ' ',
                    _ => c
                };

                bool isWs = char.IsWhiteSpace(mapped);

                if (!seenNonWs)
                {
                    if (isWs)
                    {
                        i++;
                        continue;
                    }

                    seenNonWs = true;
                    dst[w++] = mapped;
                    i++;
                    continue;
                }

                if (isWs)
                {
                    pendingWs++;
                    i++;
                    continue;
                }

                while (pendingWs > 0)
                {
                    dst[w++] = ' ';
                    pendingWs--;
                }

                dst[w++] = mapped;
                i++;
            }
        });
    }
}