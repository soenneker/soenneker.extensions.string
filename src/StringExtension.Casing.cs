using Soenneker.Extensions.Char;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Converts the first character of the string to lowercase if the string is not null or white-space.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string with the first character converted to lowercase, or the original string if it is null or white-space.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToLowerFirstChar(this string? value)
    {
        int len = value?.Length ?? 0;

        if (len == 0)
            return value;

        char c0 = value![0];
        char lc = c0.ToLowerInvariant();

        if (c0 == lc)
            return value;

        if (len == 1)
            return lc.ToString();

        return string.Create(len, value, static (dst, src) =>
        {
            dst[0] = src[0].ToLowerInvariant();
            src.AsSpan(1).CopyTo(dst[1..]);
        });
    }

    /// <summary>
    /// Converts the first character of the string to uppercase if the string is not null or white-space.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string with the first character converted to uppercase, or the original string if it is null or white-space.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToUpperFirstChar(this string? value)
    {
        int len = value?.Length ?? 0;
        if (len == 0)
            return value;

        char c0 = value![0];
        char uc = c0.ToUpperInvariant();

        if (c0 == uc)
            return value;

        if (len == 1)
            return uc.ToString();

        return string.Create(len, value, static (dst, src) =>
        {
            dst[0] = src[0].ToUpperInvariant();
            src.AsSpan(1).CopyTo(dst[1..]);
        });
    }

    /// <summary>
    /// Converts each character in the current <see cref="string"/> to its lowercase invariant equivalent.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A new string in which each character has been converted to its lowercase invariant equivalent.</returns>
    /// <remarks>This method is similar to <see cref="string.ToLowerInvariant"/> but operates on each character individually.</remarks>
    [Pure]
    public static string ToLowerInvariantFast(this string str)
    {
        ReadOnlySpan<char> s = str;
        var i = 0;
        for (; i < s.Length; i++)
        {
            char c = s[i];

            if ((uint) (c - 'A') <= 'Z' - 'A' || c.IsUpperFast())
                break;
        }

        if (i == s.Length)
            return str; // nothing to change

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start).CopyTo(dst);
            ReadOnlySpan<char> ss = src.AsSpan();

            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                dst[j] = (uint) (c - 'A') <= 'Z' - 'A' ? (char) (c + 32) : c.IsUpperFast() ? c.ToLowerInvariant() : c;
            }
        });
    }

    /// <summary>
    /// Converts each character in the current <see cref="string"/> to its uppercase invariant equivalent.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A new string in which each character has been converted to its uppercase invariant equivalent.</returns>
    /// <remarks>This method is similar to <see cref="string.ToUpperInvariant"/> but operates on each character individually.</remarks>
    [Pure]
    public static string ToUpperInvariantFast(this string str)
    {
        ReadOnlySpan<char> s = str;
        var i = 0;

        for (; i < s.Length; i++)
        {
            char c = s[i];

            if ((uint) (c - 'a') <= 'z' - 'a' || c.IsLowerFast())
                break;
        }

        if (i == s.Length)
            return str;

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start).CopyTo(dst);
            ReadOnlySpan<char> ss = src.AsSpan();
            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                dst[j] = (uint) (c - 'a') <= 'z' - 'a' ? (char) (c - 32) : c.IsLowerFast() ? c.ToUpperInvariant() : c;
            }
        });
    }

    /// <summary>
    /// Converts all uppercase ASCII letters ('A'-'Z') in the specified string to lowercase ('a'-'z') 
    /// using ordinal casing rules. Non-ASCII characters are left unchanged.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>
    /// A new string where all ASCII uppercase letters have been converted to lowercase.
    /// If the input string is empty, an empty string is returned.
    /// </returns>
    /// <remarks>
    /// This method is optimized for performance and only affects ASCII characters. 
    /// It does not perform culture-aware casing like <see cref="string.ToLower()"/>.
    /// </remarks>
    [Pure]
    public static string ToLowerOrdinal(this string str)
    {
        ReadOnlySpan<char> s = str;
        var i = 0;
        for (; i < s.Length; i++)
        {
            char c = s[i];
            if ((uint) (c - 'A') <= 'Z' - 'A') break;
        }

        if (i == s.Length) return str;

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start).CopyTo(dst);
            ReadOnlySpan<char> ss = src.AsSpan();
            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                dst[j] = (uint) (c - 'A') <= 'Z' - 'A' ? (char) (c + 32) : c;
            }
        });
    }

    /// <summary>
    /// Converts all lowercase ASCII letters ('a'-'z') in the specified string to uppercase ('A'-'Z') 
    /// using ordinal casing rules. Non-ASCII characters are left unchanged.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>
    /// A new string where all ASCII lowercase letters have been converted to uppercase.
    /// If the input string is empty, an empty string is returned.
    /// </returns>
    /// <remarks>
    /// This method is optimized for performance and only affects ASCII characters. 
    /// It does not perform culture-aware casing like <see cref="string.ToUpper()"/>.
    /// </remarks>
    [Pure]
    public static string ToUpperOrdinal(this string str)
    {
        ReadOnlySpan<char> s = str;
        var i = 0;

        for (; i < s.Length; i++)
        {
            char c = s[i];

            if ((uint) (c - 'a') <= 'z' - 'a')
                break;
        }

        if (i == s.Length)
            return str;

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start).CopyTo(dst);
            ReadOnlySpan<char> ss = src.AsSpan();

            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                dst[j] = (uint) (c - 'a') <= 'z' - 'a' ? (char) (c - 32) : c;
            }
        });
    }

    /// <summary>
    /// Converts the specified string to title case (each word capitalized), using spaces to determine word boundaries.
    /// </summary>
    /// <param name="str">The string to convert to title case.</param>
    /// <returns>A string converted to title case where each word is capitalized.</returns>
    /// <remarks>
    /// This method uses the current culture's <see cref="TextInfo"/> to perform the conversion.
    /// If the input string is null or empty, it returns the original string.
    /// </remarks>
    [Pure]
    public static string ToTitleCaseViaSpaces(this string str)
    {
        if (str.IsNullOrEmpty())
            return str;

        return string.Create(str.Length, str, static (dst, src) =>
        {
            var newWord = true;

            for (var i = 0; i < src.Length; i++)
            {
                char c = src[i];
                if (char.IsWhiteSpace(c))
                {
                    newWord = true;
                    dst[i] = c;
                }
                else if (newWord)
                {
                    dst[i] = c.ToUpperInvariant();
                    newWord = false;
                }
                else
                {
                    dst[i] = c.ToLowerInvariant();
                }
            }
        });
    }

    /// <summary>
    /// Converts the input <see cref="string"/> from PascalCase to snake_case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A new string in snake_case format.</returns>
    /// <remarks>
    /// This method converts a PascalCase string to snake_case format.
    /// For example, "PascalCaseString" will be converted to "pascal_case_string".
    /// </remarks>
    [Pure]
    public static string ToSnakeCaseFromPascal(this string input)
    {
        int len = input?.Length ?? 0;

        if (len == 0)
            return input;

        var underscores = 0;
        for (var i = 1; i < len; i++)
        {
            if (input[i].IsUpperFast())
                underscores++;
        }

        if (underscores == 0)
            return input.ToLowerInvariant(); // nothing to split; just lower

        int outLen = len + underscores;

        return string.Create(outLen, input, static (dst, src) =>
        {
            var w = 0;
            for (var i = 0; i < src.Length; i++)
            {
                char c = src[i];

                if (i > 0 && c.IsUpperFast()) 
                    dst[w++] = '_';

                // lower ASCII fast path
                dst[w++] = (uint) (c - 'A') <= 'Z' - 'A' ? (char) (c + 32) : c.ToLowerInvariant();
            }
        });
    }
}