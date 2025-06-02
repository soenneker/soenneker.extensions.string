using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

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

        var builder = new StringBuilder(input.Length);

        for (var i = 0; i < input.Length; i++)
        {
            char current = input[i];

            if (current == '{' && i + 1 < input.Length && input[i + 1] == '{')
            {
                i++; // skip the second '{'
                continue; // remove '{{'
            }

            if (current == '}' && i + 1 < input.Length && input[i + 1] == '}')
            {
                i++; // skip the second '}'
                continue; // remove '}}'
            }

            switch (current)
            {
                case '"':
                    builder.Append('\'');
                    break;
                case '\\':
                    builder.Append('/');
                    break;
                case '\r':
                case '\n':
                    builder.Append(' ');
                    break;
                default:
                    builder.Append(current);
                    break;
            }
        }

        return builder.ToString().Trim();
    }
}