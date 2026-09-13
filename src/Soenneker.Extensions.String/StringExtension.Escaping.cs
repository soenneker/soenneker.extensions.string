using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Soenneker.Extensions.String;

/// <summary>
/// Represents the string extension.
/// </summary>
public static partial class StringExtension
{
    private static readonly SearchValues<char> _scribanChanges =
        SearchValues.Create("{}\"\\\t\n\v\f\r\u0085\u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000");

    /// <summary>
    /// Use whenever a URL needs to be encoded etc.
    /// Utilizes Uri.EscapeDataString
    /// </summary>
    /// <remarks>https://stackoverflow.com/questions/602642/server-urlencode-vs-httputility-urlencode/1148326#1148326</remarks>
    /// <returns>Use whenever a URL needs to be encoded etc. Utilizes Uri.EscapeDataString.</returns>
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
    /// <returns>Utilizes Uri.UnescapeDataString.</returns>
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
        if (input.IsNullOrEmpty())
            return "";

        ReadOnlySpan<char> s = input;
        if (!char.IsWhiteSpace(s[0]) && !char.IsWhiteSpace(s[^1]) && !s.ContainsAny(_scribanChanges))
            return input;

        return EscapeScribanCore(input);
    }

    private static string EscapeScribanCore(string input)
    {
        ReadOnlySpan<char> source = input;
        char[]? rented = null;
        Span<char> destination = source.Length <= _largeStackAllocThreshold
            ? stackalloc char[source.Length]
            : (rented = ArrayPool<char>.Shared.Rent(source.Length));

        try
        {
            int pendingWhitespace = 0;
            int written = 0;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if ((c == '{' || c == '}') && i + 1 < source.Length && source[i + 1] == c)
                {
                    i++;
                    continue;
                }

                char mapped = c switch
                {
                    '"' => '\'',
                    '\\' => '/',
                    '\r' or '\n' => ' ',
                    _ => c
                };

                // Preserve the existing normalization of internal Unicode whitespace to spaces.
                if (char.IsWhiteSpace(mapped))
                {
                    if (written != 0)
                        pendingWhitespace++;
                    continue;
                }

                if (pendingWhitespace != 0)
                {
                    destination.Slice(written, pendingWhitespace).Fill(' ');
                    written += pendingWhitespace;
                    pendingWhitespace = 0;
                }

                destination[written++] = mapped;
            }

            return destination[..written].SequenceEqual(source) ? input : new string(destination[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }
}
