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

        // ---- PASS 1: compute final length (with trim after transform) ----
        var outLen = 0;
        var i = 0;

        var seenNonWs = false;
        var pendingWs = 0; // whitespace collected between non-whitespace runs

        while (i < s.Length)
        {
            char c = s[i];

            // remove "{{" and "}}"
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

            // replacements
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
                // skip leading whitespace entirely
                if (isWs)
                {
                    i++;
                    continue;
                }

                seenNonWs = true;
                outLen++; // first non-ws char
            }
            else
            {
                if (isWs)
                {
                    pendingWs++; // don’t add yet; might be trimmed at end
                }
                else
                {
                    outLen += pendingWs; // flush pending ws (now it's not trailing)
                    pendingWs = 0;
                    outLen++;
                }
            }

            i++;
        }

        // trailing whitespace is excluded (don’t add pendingWs)
        if (outLen == 0) 
            return "";

        // ---- PASS 2: write directly ----
        return string.Create(outLen, input, static (dst, srcStr) =>
        {
            ReadOnlySpan<char> s = srcStr;

            var w = 0;
            var i = 0;

            // skip leading whitespace after transform
            var leadingDone = false;

            while (i < s.Length)
            {
                char c = s[i];

                // remove "{{" and "}}"
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

                // map char
                char mapped = c switch
                {
                    '"' => '\'',
                    '\\' => '/',
                    '\r' => ' ',
                    '\n' => ' ',
                    _ => c
                };

                bool isWs = char.IsWhiteSpace(mapped);

                if (!leadingDone)
                {
                    if (isWs)
                    {
                        i++;
                        continue;
                    } // drop leading ws

                    leadingDone = true;
                    dst[w++] = mapped;
                    i++;
                    continue;
                }

                // write, but avoid trailing ws: look-ahead and only write ws if a future non-ws exists
                if (isWs)
                {
                    // peek ahead to see if any non-ws remains
                    int j = i + 1;
                    var hasMoreNonWs = false;
                    while (j < s.Length)
                    {
                        char cj = s[j];
                        if (cj == '{' && j + 1 < s.Length && s[j + 1] == '{')
                        {
                            j += 2;
                            continue;
                        }

                        if (cj == '}' && j + 1 < s.Length && s[j + 1] == '}')
                        {
                            j += 2;
                            continue;
                        }

                        char mj = cj switch
                        {
                            '"' => '\'',
                            '\\' => '/',
                            '\r' => ' ',
                            '\n' => ' ',
                            _ => cj
                        };

                        if (!char.IsWhiteSpace(mj))
                        {
                            hasMoreNonWs = true;
                            break;
                        }

                        j++;
                    }

                    if (!hasMoreNonWs) break; // drop trailing ws entirely
                }

                dst[w++] = mapped;
                i++;
            }
        });
    }
}