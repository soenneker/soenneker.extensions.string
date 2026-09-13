using Soenneker.Extensions.Char;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

namespace Soenneker.Extensions.String;

/// <summary>
/// Represents the string extension.
/// </summary>
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
            char c = src[0];
            dst[0] = c.ToLowerInvariant();
            src.AsSpan(1)
               .CopyTo(dst[1..]);
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
            char c = src[0];
            dst[0] = c.ToUpperInvariant();
            src.AsSpan(1)
               .CopyTo(dst[1..]);
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
        if (s.Length >= 16 && Ascii.IsValid(s))
            return ToLowerOrdinal(str);

        int i = 0;

        for (; i < s.Length; i++)
        {
            char c = s[i];

            if ((uint)(c - 'A') <= ('Z' - 'A'))
                break;

            if (c > 127 && c.IsUpperFast())
                return str.ToLowerInvariant();
        }

        if (i == s.Length)
            return str;

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            ReadOnlySpan<char> ss = src;
            ss[..start].CopyTo(dst);

            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                dst[j] = (uint)(c - 'A') <= ('Z' - 'A')
                    ? (char)(c + 32)
                    : c > 127 && c.IsUpperFast() ? c.ToLowerInvariant() : c;
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
        if (s.Length >= 16 && Ascii.IsValid(s))
            return ToUpperOrdinal(str);

        var i = 0;
        for (; i < s.Length; i++)
        {
            char c = s[i];
            if ((uint)(c - 'a') <= 'z' - 'a' || c > 127 && c.IsLowerFast())
                break;
        }

        if (i == s.Length)
            return str;

        return string.Create(s.Length, (str, i), static (dst, st) =>
        {
            (string src, int start) = st;
            ReadOnlySpan<char> ss = src;
            ss[..start]
                .CopyTo(dst);

            for (int j = start; j < ss.Length; j++)
            {
                char c = ss[j];
                if ((uint)(c - 'a') <= 'z' - 'a')
                {
                    dst[j] = (char)(c - 32);
                }
                else
                {
                    dst[j] = c.IsLowerFast() ? c.ToUpperInvariant() : c;
                }
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
        ReadOnlySpan<char> source = str;
        int first = source.IndexOfAnyInRange('A', 'Z');
        if (first < 0)
            return str;

        return string.Create(source.Length, (str, first), static (destination, state) =>
        {
            state.str.AsSpan(0, state.first).CopyTo(destination);
            ChangeOrdinalCase(state.str.AsSpan(state.first), destination[state.first..], false);
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
        ReadOnlySpan<char> source = str;
        int first = source.IndexOfAnyInRange('a', 'z');
        if (first < 0)
            return str;

        return string.Create(source.Length, (str, first), static (destination, state) =>
        {
            state.str.AsSpan(0, state.first).CopyTo(destination);
            ChangeOrdinalCase(state.str.AsSpan(state.first), destination[state.first..], true);
        });
    }

    private static void ChangeOrdinalCase(ReadOnlySpan<char> source, Span<char> destination, bool upper)
    {
        int position = 0;
        uint first = upper ? 'a' : 'A';
        if (Vector.IsHardwareAccelerated && source.Length >= Vector<ushort>.Count)
        {
            ReadOnlySpan<ushort> input = MemoryMarshal.Cast<char, ushort>(source);
            Span<ushort> output = MemoryMarshal.Cast<char, ushort>(destination);
            var start = new Vector<ushort>((ushort)first);
            var width = new Vector<ushort>(25);
            var bit = new Vector<ushort>(32);
            for (; position <= input.Length - Vector<ushort>.Count; position += Vector<ushort>.Count)
            {
                var chars = new Vector<ushort>(input[position..]);
                Vector<ushort> mask = Vector.LessThanOrEqual(chars - start, width) & bit;
                (chars ^ mask).CopyTo(output[position..]);
            }
        }

        for (; position < source.Length; position++)
        {
            char c = source[position];
            destination[position] = (uint)c - first <= 25 ? (char)(c ^ 32) : c;
        }
    }

}
