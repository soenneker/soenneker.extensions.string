using Soenneker.Extensions.Char;
using Soenneker.Extensions.Spans.Readonly.Chars;
using Soenneker.Utils.Random;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Soenneker.Extensions.String;

/// <summary>
/// A collection of useful string extension methods
/// </summary>
public static partial class StringExtension
{
    /// <summary>
    /// Safe on modern hardware (Intel Core 6th gen+ / AMD Ryzen 1st gen+ / ARM Cortex-A72+).
    /// </summary>
    private const int _stackallocThreshold = 256;

    private const int _largeStackAllocThreshold = 512;

    private static readonly SearchValues<char> _asciiWhiteSpaceSearchValues = SearchValues.Create(" \t\r\n\f\v");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int IndexOfWhiteSpaceFast(ReadOnlySpan<char> value)
    {
        int firstAscii = value.IndexOfAny(_asciiWhiteSpaceSearchValues);
        int scanLength = firstAscii < 0 ? value.Length : firstAscii;

        for (var i = 0; i < scanLength; i++)
        {
            char c = value[i];
            if (c > 127 && c.IsWhiteSpaceFast())
                return i;
        }

        return firstAscii;
    }

    /// <summary>
    /// Truncates a string to the specified length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="length">The maximum length of the truncated string.</param>
    /// <returns>The truncated string.</returns>
    public static string Truncate(this string value, int length)
    {
        if (value.IsNullOrEmpty())
            return "";

        // If the requested length >= the current length, just return the original (no new allocation).
        if (length >= value.Length)
            return value;

        if (length <= 0)
            return "";

        // Allocate a new string from the first 'length' characters.
        return value[..length];
    }

    /// <summary>
    /// Determines whether a string contains only alphanumeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is alphanumeric, otherwise false.</returns>
    [Pure]
    public static bool IsAlphaNumeric(this string? value)
    {
        if (value.IsNullOrEmpty())
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (!value[i]
                    .IsLetterOrDigitFast())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Removes all non-digit characters from the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>A new string that contains only the digit characters from the original string.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveNonDigits(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        ReadOnlySpan<char> s = value.AsSpan();

        int firstNonDigit = -1;
        for (int i = 0; i < s.Length; i++)
        {
            if (!s[i]
                    .IsDigitFast())
            {
                firstNonDigit = i;
                break;
            }
        }

        if (firstNonDigit < 0)
            return value;

        int outLen = firstNonDigit;
        for (int i = firstNonDigit + 1; i < s.Length; i++)
        {
            if (s[i]
                .IsDigitFast())
                outLen++;
        }

        if (outLen == 0)
            return string.Empty;

        return string.Create(outLen, (value, firstNonDigit), static (dst, state) =>
        {
            (string src, int firstBad) = state;
            ReadOnlySpan<char> sp = src.AsSpan();

            sp[..firstBad]
                .CopyTo(dst);
            int w = firstBad;

            for (int i = firstBad + 1; i < sp.Length; i++)
            {
                char c = sp[i];
                if (c.IsDigitFast())
                    dst[w++] = c;
            }
        });
    }

    /// <summary>
    /// Removes all white-space characters from the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>A new string that contains only the non-white-space characters from the original string.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveWhiteSpace(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        ReadOnlySpan<char> s = value;

        int first = IndexOfWhiteSpaceFast(s);

        if (first < 0)
            return value;

        int outLen = first;
        for (int i = first + 1; i < s.Length; i++)
            if (!s[i]
                    .IsWhiteSpaceFast())
                outLen++;

        if (outLen == 0)
            return string.Empty;

        return string.Create(outLen, (value, first), static (dst, st) =>
        {
            (string src, int start) = st;
            ReadOnlySpan<char> sp = src;

            sp[..start]
                .CopyTo(dst);
            int w = start;

            for (int i = start + 1; i < sp.Length; i++)
            {
                char c = sp[i];
                if (!c.IsWhiteSpaceFast())
                    dst[w++] = c;
            }
        });
    }

    /// <summary>
    /// Removes all occurrences of a specified character from the input string.
    /// </summary>
    /// <param name="value">The string to process. If <c>null</c> or empty, it is returned as-is.</param>
    /// <param name="removeChar">The character to remove from the string.</param>
    /// <returns>
    /// A new string with all instances of <paramref name="removeChar"/> removed,
    /// or the original string if no changes were necessary. Returns <c>null</c> if <paramref name="value"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method avoids unnecessary allocations by only creating a buffer if at least one character is removed.
    /// </remarks>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveAllChar(this string? value, char removeChar)
    {
        if (value.IsNullOrEmpty())
            return value;

        ReadOnlySpan<char> s = value;
        int first = s.IndexOf(removeChar);
        if (first < 0)
            return value;

        int outLen = first;
        for (int i = first + 1; i < s.Length; i++)
            if (s[i] != removeChar)
                outLen++;

        if (outLen == 0)
            return string.Empty;

        return string.Create(outLen, (value, first, removeChar), static (dst, st) =>
        {
            (string src, int start, char rm) = st;
            ReadOnlySpan<char> sp = src;
            sp[..start]
                .CopyTo(dst);
            int w = start;
            for (int i = start + 1; i < sp.Length; i++)
            {
                char c = sp[i];
                if (c != rm)
                    dst[w++] = c;
            }
        });
    }

    /// <summary>
    /// Removes all white-space characters from the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>A new string that contains only the non-white-space characters from the original string.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveDashes(this string? value)
    {
        return value.RemoveAllChar('-');
    }

    /// <summary>
    /// Determines whether the string ends with any of the specified suffixes.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="suffixes">An enumerable collection of suffixes to check against.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if the string ends with any of the specified suffixes; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool EndsWithAny(this string value, string[] suffixes, StringComparison comparison = StringComparison.Ordinal)
    {
        if (value.IsNullOrEmpty() || suffixes is null || suffixes.Length == 0)
            return false;

        for (var i = 0; i < suffixes.Length; i++)
        {
            string s = suffixes[i];
            if (!s.IsNullOrEmpty() && value.EndsWith(s, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the string starts with any of the specified prefixes.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="prefixes">An enumerable collection of prefixes to check against.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if the string starts with any of the specified prefixes; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool StartsWithAny(this string value, string[] prefixes, StringComparison comparison = StringComparison.Ordinal)
    {
        if (value.IsNullOrEmpty() || prefixes is null || prefixes.Length == 0)
            return false;

        for (var i = 0; i < prefixes.Length; i++)
        {
            string p = prefixes[i];

            if (!p.IsNullOrEmpty() && value.StartsWith(p, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the string contains any of the specified values.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="values">A list of values to search for.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if the string contains any of the specified values; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool ContainsAny(this string value, IList<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        if (values is null || values.Count == 0)
            return false;

        for (var i = 0; i < values.Count; i++)
        {
            string needle = values[i];

            if (needle.HasContent() && value.IndexOf(needle, comparison) >= 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the string contains any of the specified characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="characters">The characters to search for.</param>
    /// <returns><c>true</c> if the string contains any of the specified characters; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool ContainsAny(this string value, params char[]? characters)
    {
        if (value.IsNullOrEmpty() || characters is null || characters.Length == 0)
            return false;

        // Fast SIMD path in the runtime
        return value.AsSpan()
                    .IndexOfAny(characters) >= 0;
    }

    /// <summary>
    /// Determines whether the string contains at least one requested character.
    /// </summary>
    /// <param name="value">The string to inspect or convert.</param>
    /// <param name="searchValues">The optimized character set to search for.</param>
    /// <returns>True when any requested character occurs.</returns>
    [Pure]
    public static bool ContainsAny(this string value, SearchValues<char> searchValues)
    {
        if (value.IsNullOrEmpty())
            return false;

        return value.AsSpan()
                    .IndexOfAny(searchValues) >= 0;
    }

    /// <summary>
    /// Determines whether the string contains at least one requested character.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="characters">The candidate characters to search for.</param>
    /// <returns>True when any requested character occurs.</returns>
    [Pure]
    public static bool ContainsAny(this string value, ReadOnlySpan<char> characters)
    {
        if (value.IsNullOrEmpty() || characters.Length == 0)
            return false;

        return value.AsSpan()
                    .IndexOfAny(characters) >= 0;
    }

    /// <summary>
    /// Determines whether the string is equal to any of the specified strings using the specified comparison rules.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <param name="strings">The strings to compare against.</param>
    /// <returns><c>true</c> if the string is equal to any of the specified strings; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool EqualsAny(this string value, StringComparison comparison = StringComparison.Ordinal, params string[] strings)
    {
        if (strings is null || strings.Length == 0)
            return false;

        ReadOnlySpan<char> needle = value;

        for (var i = 0; i < strings.Length; i++)
        {
            if (needle.Equals(strings[i]
                    .AsSpan(), comparison))
                return true;
        }

        return false;
    }


    /// <summary>
    /// Determines whether the string is equal to any of the strings in the specified collection using the specified comparison rules.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="strings">The collection of strings to compare against.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if the string is equal to any of the strings in the collection; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool EqualsAny(this string value, IEnumerable<string> strings, StringComparison comparison = StringComparison.Ordinal)
    {
        // If the collection is null, there's nothing to compare.
        if (strings is null)
            return false;

        // Short-circuit on the first match.
        foreach (string candidate in strings)
        {
            if (string.Equals(value, candidate, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// From Date, with "dd/MM/yyyy" (assuming local)       
    /// </summary>
    /// <returns>From Date, with "dd/MM/yyyy" (assuming local).</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? ToDateTime(this string? date) =>
        DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime dt) ? dt : null;

    /// <summary>
    /// Parses a string with invariant culture and adjusts the result to UTC; invalid input produces null.
    /// </summary>
    /// <param name="value">The date and time text to parse.</param>
    /// <returns>The parsed UTC-adjusted value, or null when parsing fails.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? ToUtcDateTime(this string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dt) ? dt : null;

    /// <summary>
    /// Parses a string into a DateTimeOffset using invariant culture.
    /// If the input has no offset/zone, it is assumed to be local.
    /// </summary>
    /// <returns>Parses a string into a DateTimeOffset using invariant culture. If the input has no offset/zone, it is assumed to be local.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ToDateTimeOffset(this string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto) ? dto : null;

    /// <summary>
    /// Parses a string into a DateTimeOffset using invariant culture, then normalizes it to UTC (offset 00:00).
    /// If the input has no offset/zone, it is assumed to be local before conversion to UTC.
    /// </summary>
    /// <returns>Parses a string into a DateTimeOffset using invariant culture, then normalizes it to UTC (offset 00:00). If the input has no offset/zone, it is assumed to be local before conversion to UTC.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ToUtcDateTimeOffset(this string? value)
    {
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var dto))
            return null;

        // Ensure Offset == 00:00 (Normalize)
        return dto.ToUniversalTime();
    }

    private static readonly string[] _isoDateTimeOffsetFormats =
    [
        "O", // 2024-10-03T13:40:34.5299422Z or +00:00
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"
    ];

    /// <summary>
    /// Strict ISO-8601 parse (recommended for APIs). Handles "Z" and explicit offsets reliably.
    /// If no offset is present, it will assume local.
    /// </summary>
    /// <returns>Strict ISO-8601 parse (recommended for APIs). Handles "Z" and explicit offsets reliably. If no offset is present, it will assume local.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ToIsoDateTimeOffset(this string? value)
    {
        if (value.IsNullOrEmpty())
            return null;

        return DateTimeOffset.TryParseExact(value, _isoDateTimeOffsetFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset dto)
            ? dto
            : null;
    }

    /// <summary>
    /// Strict ISO-8601 parse, normalized to UTC (offset 00:00).
    /// If no offset is present, assumes local before conversion to UTC.
    /// </summary>
    /// <returns>Strict ISO-8601 parse, normalized to UTC (offset 00:00). If no offset is present, assumes local before conversion to UTC.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ToUtcIsoDateTimeOffset(this string? value)
    {
        var dto = value.ToIsoDateTimeOffset();
        return dto?.ToUniversalTime();
    }


    /// <summary>
    /// Replaces periods with dashes
    /// </summary>
    /// <returns>Replaces periods with dashes.</returns>
    [Pure]
    public static string ToDashesFromPeriods(this string value)
    {
        return value.Replace('.', '-');
    }

    /// <summary>
    /// Replaces whitespace with dashes
    /// </summary>
    /// <returns>Replaces whitespace with dashes.</returns>
    [Pure]
    public static string ToDashesFromWhiteSpace(this string value)
    {
        ReadOnlySpan<char> s = value.AsSpan();
        int first = IndexOfWhiteSpaceFast(s);

        if (first < 0)
            return value; // return original ref

        return string.Create(value.Length, (value, first), static (Span<char> dst, (string src, int start) st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start)
               .CopyTo(dst);

            ReadOnlySpan<char> ss = src.AsSpan();

            for (int i = start; i < ss.Length; i++)
            {
                dst[i] = ss[i]
                    .IsWhiteSpaceFast()
                    ? '-'
                    : ss[i];
            }
        });
    }

    /// <summary>
    /// Converts a comma-separated string into a list of trimmed, non-empty substrings.
    /// </summary>
    /// <remarks>Empty entries and whitespace-only substrings are ignored in the resulting list.</remarks>
    /// <param name="value">The comma-separated string to parse. Can be null.</param>
    /// <returns>A list of trimmed, non-empty substrings parsed from the input. Returns an empty list if the input is null or
    /// contains no non-empty values.</returns>
    [Pure]
    public static List<string> FromCommaSeparatedToList(this string? value)
    {
        if (value.IsNullOrEmpty())
            return [];

        var list = new List<string>(value.AsSpan().Count(',') + 1);
        ReadOnlySpan<char> remaining = value.AsSpan();

        while (true)
        {
            int separator = remaining.IndexOf(',');
            ReadOnlySpan<char> item = (separator < 0 ? remaining : remaining[..separator]).Trim();

            if (!item.IsEmpty)
                list.Add(item.ToString());

            if (separator < 0)
                break;

            remaining = remaining[(separator + 1)..];
        }

        return list;
    }

    /// <summary>
    /// Splits the specified string into an array of non-empty, trimmed substrings based on the given delimiter.
    /// </summary>
    /// <param name="value">The string to split. Can be null or contain only whitespace.</param>
    /// <param name="delimiter">The character to use as the delimiter for splitting the string.</param>
    /// <returns>An array of non-empty, trimmed substrings, or null if the input is null, contains only whitespace, or no
    /// non-empty substrings are found.</returns>
    [Pure]
    public static string[]? SplitTrimmedNonEmpty(this string? value, char delimiter)
    {
        if (value.IsNullOrEmpty())
            return null;

        string[] result = value.AsSpan().SplitTrimmedNonEmpty(delimiter);
        return result.Length == 0 ? null : result;
    }

    /// <summary>
    /// Equivalent to Encoding.UTF8.GetBytes(value)
    /// </summary>
    /// <returns>A value equivalent to Encoding.UTF8.GetBytes(value).</returns>
    [Pure]
    public static byte[] ToBytes(this string str)
    {
        if (str.IsNullOrEmpty())
            return [];

        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// Decodes Base64 text to bytes; null or empty text produces an empty array.
    /// </summary>
    /// <param name="value">The Base64 text to decode.</param>
    /// <returns>The decoded bytes.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToBytesFromBase64(this string value)
    {
        // Handle null or empty up front.
        if (value.IsNullOrEmpty())
            return [];

        return Convert.FromBase64String(value);
    }

    /// <summary>
    /// Decodes hexadecimal text to bytes; null or empty text produces an empty array.
    /// </summary>
    /// <param name="hex">The hexadecimal text to decode.</param>
    /// <returns>The decoded bytes.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToBytesFromHex(this string hex)
    {
        if (hex.IsNullOrEmpty())
            return [];

        return Convert.FromHexString(hex);
    }

    /// <summary>
    /// Replaces "\r\n" with "\n"
    /// </summary>
    /// <returns>Replaces "\r\n" with "\n".</returns>
    [Pure]
    public static string ToUnixLineBreaks(this string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    /// <summary>
    /// Extracts the short form of a zip code by removing any characters after the hyphen (if present).
    /// </summary>
    /// <param name="value">The input zip code string.</param>
    /// <returns>The short form of the zip code.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToShortZipCode(this string value)
    {
        int index = value.IndexOf('-');
        return index == -1 ? value : new string(value.AsSpan()[..index]);
    }

    /// <summary>
    /// Shuffles the characters in the string randomly.
    /// </summary>
    /// <param name="value">The input string to be shuffled.</param>
    /// <returns>The shuffled string.</returns>
    [Pure]
    public static string Shuffle(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        int length = value.Length;

        if (length <= _stackallocThreshold)
        {
            Span<char> buffer = stackalloc char[length];
            value.AsSpan()
                 .CopyTo(buffer);
            PerformShuffle(buffer);
            return new string(buffer);
        }

        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rented = pool.Rent(length);

        try
        {
            Span<char> buffer = rented.AsSpan(0, length);
            value.AsSpan()
                 .CopyTo(buffer);
            PerformShuffle(buffer);
            return new string(buffer);
        }
        finally
        {
            pool.Return(rented);
        }
    }

    private static void PerformShuffle(Span<char> buffer)
    {
        int n = buffer.Length;

        // Fisher-Yates Shuffle
        while (n > 1)
        {
            n--;
            int k = RandomUtil.Next(n + 1);

            // Swap elements
            (buffer[n], buffer[k]) = (buffer[k], buffer[n]);
        }
    }

    /// <summary>
    /// Securely shuffles the characters in the specified string.
    /// </summary>
    /// <param name="value">The string to shuffle.</param>
    /// <returns>A new string with the characters shuffled.</returns>
    [Pure]
    public static string SecureShuffle(this string value)
    {
        int length = value?.Length ?? 0;
        if (length == 0)
            return value;

        if (length <= _stackallocThreshold)
        {
            Span<char> buffer = stackalloc char[length];
            value.AsSpan()
                 .CopyTo(buffer);

            PerformSecureShuffle(buffer);
            string result = new string(buffer);
            CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsBytes(buffer));

            return result;
        }

        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rented = pool.Rent(length);

        try
        {
            Span<char> buffer = rented.AsSpan(0, length);
            value.AsSpan()
                 .CopyTo(buffer);
            PerformSecureShuffle(buffer);
            return new string(buffer);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsBytes(rented.AsSpan(0, length)));
            pool.Return(rented, clearArray: false);
        }
    }

    private static void PerformSecureShuffle(Span<char> buffer)
    {
        int n = buffer.Length;

        // Fisher-Yates Shuffle with a cryptographically secure RNG
        while (n > 1)
        {
            n--;
            int k = RandomNumberGenerator.GetInt32(n + 1);

            // Swap elements
            (buffer[n], buffer[k]) = (buffer[k], buffer[n]);
        }
    }

    /// <summary>
    /// Shorthand for <see cref="string.IsNullOrEmpty"/>
    /// </summary>
    /// <returns><see langword="true"/> when the value is null or empty; otherwise <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        // Perform a null check first, then check Length for efficiency
        // and to ensure minimal instruction execution in the common case.
        return value is null || value.Length == 0;
    }

    /// <summary>
    /// Determines whether a non-null string contains zero characters.
    /// </summary>
    /// <returns><see langword="true"/> for an empty string; <see langword="false"/> for null or non-empty text.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(this string? value)
    {
        return value?.Length == 0;
    }

    /// <summary>
    /// Shorthand for <code>string.IsNullOrEmpty() == false</code>
    /// </summary>
    /// <remarks>This should be used over the IsPopulated() method on the IEnumerable extension</remarks>
    /// <returns><see langword="true"/> when the value is neither null nor empty; otherwise <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasContent([NotNullWhen(true)] this string? value)
    {
        return !value.IsNullOrEmpty();
    }

    /// <summary>
    /// Shorthand for <see cref="string.IsNullOrWhiteSpace"/>
    /// </summary>
    /// <returns><see langword="true"/> when the value is null, empty, or whitespace-only; otherwise <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Determines whether every character is whitespace; null and empty strings are considered whitespace.
    /// </summary>
    /// <param name="value">The string to inspect.</param>
    /// <returns>True when the value contains no non-whitespace characters.</returns>
    [Pure]
    public static bool IsWhiteSpace([NotNullWhen(false)] this string? value)
    {
        ReadOnlySpan<char> span = value.AsSpan();

        for (var i = 0; i < span.Length; i++)
        {
            char c = span[i];

            if (!c.IsWhiteSpaceFast())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Removes the trailing character from the specified string, if it exists.
    /// </summary>
    /// <param name="value">The string to remove the trailing character from.</param>
    /// <param name="charToRemove">The character to remove from the end of the string.</param>
    /// <returns>The string with the trailing character removed, or the original string if it is null or empty.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveTrailingChar(this string? value, char charToRemove)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value[^1] == charToRemove)
        {
            return value[..^1]; // Use string slicing for better readability and performance
        }

        return value;
    }

    /// <summary>
    /// Removes the leading character from the specified string, if it exists.
    /// </summary>
    /// <param name="value">The string to remove the leading character from.</param>
    /// <param name="charToRemove">The character to remove from the beginning of the string.</param>
    /// <returns>The string with the leading character removed, or the original string if it is null or empty.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveLeadingChar(this string? value, char charToRemove)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value[0] == charToRemove)
        {
            return value[1..]; // Use string slicing for better performance and readability
        }

        return value;
    }

    /// <summary>
    /// Converts to lowercase, and then removes/replaces characters that are invalid for URIs (does not replace accents right now)
    /// </summary>
    /// <returns>Converts to lowercase, and then removes/replaces characters that are invalid for URIs (does not replace accents right now).</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Slugify(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        ReadOnlySpan<char> s = value.AsSpan();

        Span<char> stack = s.Length <= 512 ? stackalloc char[s.Length] : default;
        char[]? rented = null;
        Span<char> dst = stack.IsEmpty ? (rented = ArrayPool<char>.Shared.Rent(s.Length)).AsSpan(0, s.Length) : stack;

        var w = 0;

        // 0 = not in sep run, 1 = underscore-only run, 2 = dash/whitespace run
        var sepState = 0;

        var changed = false;

        for (var i = 0; i < s.Length; i++)
        {
            char orig = s[i];
            char c = orig;

            char lower = c.ToLowerInvariant();

            if (lower != c)
            {
                c = lower;
                changed = true;
            }

            // ASCII fast path
            if ((uint)c <= 0x7Fu)
            {
                // [a-z0-9]
                if ((uint)(c - 'a') <= 25u || (uint)(c - '0') <= 9u)
                {
                    if (sepState != 0)
                    {
                        if (w > 0)
                        {
                            dst[w++] = sepState == 1 ? '_' : '-';
                            changed = true; // inserted normalized separator
                        }

                        sepState = 0;
                    }

                    dst[w++] = c;

                    // If the original differed (case fold etc.), it's already marked.
                    // If not, this implies no change for this char.
                    continue;
                }

                // underscore => separator run (underscore-only run unless a dash/space appears later)
                if (c == '_')
                {
                    if (sepState == 0)
                        sepState = 1;

                    // We are not copying '_' immediately, so output differs unless it gets emitted
                    // exactly in same positions (we don't guarantee that), so mark changed.
                    changed = true;
                    continue;
                }

                // dash or ASCII whitespace => dash-run
                if (c == '-' || c.IsWhiteSpaceFast())
                {
                    sepState = 2;
                    changed = true;
                    continue;
                }

                // other ASCII punctuation: skip
                changed = true;
                continue;
            }

            // Non-ASCII
            if (char.IsLetterOrDigit(orig))
            {
                if (sepState != 0)
                {
                    if (w > 0)
                    {
                        dst[w++] = sepState == 1 ? '_' : '-';
                        changed = true;
                    }

                    sepState = 0;
                }

                // For Unicode letters/digits, we already lowercased via ToLowerInvariant above
                dst[w++] = c;

                // If c != orig, changed would already be true
                // But letter/digit kept as-is could still be identical; no action needed.
                continue;
            }

            if (orig.IsWhiteSpaceFast())
            {
                sepState = 2;
                changed = true;
                continue;
            }

            // other non-ASCII punctuation/symbol: skip
            changed = true;
        }

        // If output is identical, return original reference (no allocation)
        if (!changed && w == s.Length)
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);

            return value;
        }

        string result = w == 0 ? string.Empty : new string(dst[..w]);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return result;
    }

    /// <summary>
    /// Ignores the case of the string being passed in. 
    /// </summary>
    /// <exception cref="ArgumentException">If parsing fails</exception>
    /// <returns>Ignores the case of the string being passed in.</returns>
    [Pure]
    public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum
    {
        if (value.IsNullOrEmpty())
            throw new ArgumentException($"Empty/null string was attempted to convert to enum of type {typeof(TEnum)}", nameof(value));

        return Enum.Parse<TEnum>(value, true);
    }

    /// <summary>
    /// Parses an enum name or numeric value without regard to case; null, empty, or invalid input produces null.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse.</typeparam>
    /// <param name="value">Receives the parsed value when conversion succeeds.</param>
    /// <returns>The parsed enum value, or null when parsing fails.</returns>
    [Pure]
    public static TEnum? TryToEnum<TEnum>(this string? value) where TEnum : struct, Enum
    {
        if (value.IsNullOrEmpty())
            return null;

        bool parsedSuccessfully = Enum.TryParse(value, true, out TEnum rtn);

        if (parsedSuccessfully)
            return rtn;

        return null;
    }

    /// <summary>
    /// Builds a MemoryStream from a string.
    /// </summary>
    /// <remarks>Preferably you should be using Soenneker.Utils.MemoryStreamUtil!</remarks>
    /// <returns>Builds a MemoryStream from a string.</returns>
    [Pure]
    public static MemoryStream ToMemoryStream(this string str)
    {
        // Short-circuit for null/empty to avoid unnecessary allocations.
        if (str.IsNullOrEmpty())
            return new MemoryStream([]);

        byte[] buffer = Encoding.UTF8.GetBytes(str);

        // Wrap the buffer in a read-only MemoryStream.
        // The 'publiclyVisible' parameter (last bool) allows direct array access via GetBuffer(); 
        return new MemoryStream(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Takes a Base64 encoded string, converts it to a byte array, and then converts it to a UTF8 string.
    /// </summary>
    /// <remarks>Equivalent to <code>Convert.FromBase64String(str).ToStr()</code></remarks>
    /// <returns>Takes a Base64 encoded string, converts it to a byte array, and then converts it to a UTF8 string.</returns>
    [Pure]
    public static string ToStringFromBase64(this string s)
    {
        if (s.IsNullOrEmpty())
            return string.Empty;

        ReadOnlySpan<char> input = s.AsSpan();

        char[]? rentedChars = null;
        byte[]? rentedBytes = null;
        int maxBytes = (input.Length + 3) / 4 * 3;
        Span<byte> bytes = maxBytes <= _largeStackAllocThreshold
            ? stackalloc byte[maxBytes]
            : (rentedBytes = ArrayPool<byte>.Shared.Rent(maxBytes)).AsSpan(0, maxBytes);

        try
        {
            // Avoid scanning and copying ordinary Base64. This also lets the runtime
            // handle permitted whitespace without it affecting padding calculations.
            bool shortBase64Url = input.Length <= 64 && input.IndexOfAny('-', '_') >= 0;
            if (!shortBase64Url && Convert.TryFromBase64Chars(input, bytes, out int standardWritten))
                return Encoding.UTF8.GetString(bytes[..standardWritten]);

            int pad = input.Length & 3;
            if (pad == 1)
                throw new FormatException("Invalid Base64URL length.");

            int extraPad = pad == 0 ? 0 : 4 - pad;
            int outCharsLen = input.Length + extraPad;

            Span<char> chars = outCharsLen <= _largeStackAllocThreshold
                ? stackalloc char[outCharsLen]
                : (rentedChars = ArrayPool<char>.Shared.Rent(outCharsLen)).AsSpan(0, outCharsLen);

            for (var i = 0; i < input.Length; i++)
            {
                char c = input[i];
                chars[i] = c == '-' ? '+' : c == '_' ? '/' : c;
            }

            for (var i = 0; i < extraPad; i++)
                chars[input.Length + i] = '=';

            if (!Convert.TryFromBase64Chars(chars, bytes, out int written))
                throw new FormatException("Invalid Base64 data.");

            return Encoding.UTF8.GetString(bytes[..written]);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);

            if (rentedBytes is not null)
                ArrayPool<byte>.Shared.Return(rentedBytes, clearArray: false);

            if (rentedChars is not null)
            {
                CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(rentedChars.AsSpan()));
                ArrayPool<char>.Shared.Return(rentedChars, clearArray: false);
            }
        }
    }

    /// <summary>
    /// Takes a UTF8 string, converts it to a byte array using UTF8 encoding, and then converts that byte array to a Base64 encoded string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [Pure]
    public static string ToBase64(this string str)
    {
        if (str.IsNullOrEmpty())
            return string.Empty;

        ReadOnlySpan<char> chars = str;
        int utf8Len = Encoding.UTF8.GetByteCount(chars);

        byte[]? rented = null;
        Span<byte> utf8 = utf8Len <= 512 ? stackalloc byte[utf8Len] : (rented = ArrayPool<byte>.Shared.Rent(utf8Len));

        try
        {
            Encoding.UTF8.GetBytes(chars, utf8);
            return Convert.ToBase64String(utf8[..utf8Len]);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(utf8[..utf8Len]);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented, clearArray: false);
        }
    }

    /// <summary>
    /// Essentially wraps string.Split(':').
    /// </summary>
    /// <remarks>Don't use this for splitting into document/partition keys, use <see cref="ToSplitId"/> instead.</remarks>
    /// <returns>Essentially wraps string.Split(':').</returns>
    [Pure]
    public static List<string>? ToIds(this string? value)
    {
        if (value.IsNullOrEmpty())
            return null;

        ReadOnlySpan<char> s = value.AsSpan();
        int colonCount = s.Count(':');

        var list = new List<string>(colonCount + 1);

        var start = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == ':')
            {
                list.Add(new string(s.Slice(start, i - start)));
                start = i + 1;
            }
        }

        list.Add(new string(s[start..]));
        return list;
    }

    /// <summary>Returns slice ranges for partition/document without allocating.</summary>
    /// <returns>Slice ranges for partition/document without allocating.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Range Partition, Range Document) ToSplitIdRanges(this string id)
    {
        id.ThrowIfNullOrEmpty();

        int i = id.LastIndexOf(':');
        if (i < 0)
            return (0..id.Length, 0..id.Length); // no colon

        if (i == 0)
            return (0..0, 1..id.Length); // ":something" or ":"

        if (i == id.Length - 1)
            return (0..(id.Length - 1), id.Length..id.Length); // "something:"

        return (0..i, (i + 1)..id.Length); // "pk:doc"
    }

    /// <summary>
    /// Entity ids are the concatenation of an entity's partitionKey and documentId.
    /// If an entity's partitionKey and documentId are the same, both return values will be equivalent to documentId. <para/>
    /// Format: {partitionKey}:{documentId} <para/>
    /// This also supports 'combined ids'. For example, a partition key could be guid1:guid2, and the document id is guid3. It would return guid1:guid2:guid3.
    /// </summary>
    /// <param name="id">id with 1 or 2 terms delimited by ':'.</param>
    /// <exception cref="ArgumentNullException">id cannot be null</exception>
    /// <returns>partition key, document id</returns>
    /// <summary>Existing API, materializes strings only when needed.</summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (string PartitionKey, string DocumentId) ToSplitId(this string id)
    {
        (Range pr, Range dr) = ToSplitIdRanges(id);

        // No-colon fast path reuses original ref twice
        if (pr.Start.Value == 0 && pr.End.Value == id.Length && dr.Start.Value == 0 && dr.End.Value == id.Length)
            return (id, id);

        string pk = pr.End.Value == pr.Start.Value ? string.Empty : id[pr];
        string doc = dr.End.Value == dr.Start.Value ? string.Empty : id[dr];
        return (pk, doc);
    }

    /// <summary>
    /// Concatenates the partition key and document ID with a colon separator using a stack-allocated buffer for optimized performance.
    /// </summary>
    /// <param name="documentId">The document ID to concatenate. Cannot be null.</param>
    /// <param name="partitionKey">The partition key to concatenate. Cannot be null.</param>
    /// <returns>A concatenated string in the format "partitionKey:documentId" using stack allocation to minimize memory usage and improve performance.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AddPartitionKey(this string documentId, string partitionKey)
    {
        return string.Concat(partitionKey, ':', documentId);
    }

    /// <summary>
    /// Concatenates the partition key and document ID with a colon separator using a stack-allocated buffer for optimized performance.
    /// </summary>
    /// <param name="partitionKey">The partition key to concatenate. Cannot be null.</param>
    /// <param name="documentId">The document ID to concatenate. Cannot be null.</param>
    /// <returns>A concatenated string in the format "partitionKey:documentId" using stack allocation to minimize memory usage and improve performance.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AddDocumentId(this string partitionKey, string documentId)
    {
        return string.Concat(partitionKey, ':', documentId);
    }

    /// <summary>
    /// Converts a string to a boolean value. Accepts "true", "false", "1", "0" (case-insensitive).
    /// Returns false if input is null, empty, or unrecognized.
    /// </summary>
    /// <returns>Converts a string to a boolean value. Accepts "true", "false", "1", "0" (case-insensitive). Returns false if input is null, empty, or unrecognized.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(this string? value)
    {
        if (value is null)
            return false;

        ReadOnlySpan<char> s = value.AsSpan();

        if (s.Length == 1)
            return s[0] == '1';

        if (s.Length == 4)
        {
            return (s[0] | 0x20) == 't' &&
                   (s[1] | 0x20) == 'r' &&
                   (s[2] | 0x20) == 'u' &&
                   (s[3] | 0x20) == 'e';
        }

        return s.Length > 4 && (char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[^1])) && bool.TryParse(s, out bool result) && result;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the input string is null or empty.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null</exception>
    /// <exception cref="ArgumentException">Thrown when the input string is empty.</exception>
    public static void ThrowIfNullOrEmpty(this string? input, [CallerMemberName] string? name = null)
    {
        if (input.IsNullOrEmpty())
            throw input is null ? new ArgumentNullException(name, "String cannot be null") : new ArgumentException("String cannot be empty", name);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the input string is null or whitespace.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null</exception>
    /// <exception cref="ArgumentException">Thrown when the input string is empty or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(this string? input, [CallerMemberName] string? name = null)
    {
        input.ThrowIfNullOrEmpty();

        if (input.IsWhiteSpace())
            throw new ArgumentException("String cannot be whitespace", name);
    }

    /// <summary>
    /// Masks sensitive information in a string by replacing a portion of characters with asterisks.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <returns>The masked string with sensitive information replaced by asterisks.</returns>
    [Pure]
    public static string Mask(this string input)
    {
        int len = input?.Length ?? 0;
        if (len == 0)
            return "";
        if (len <= 6)
            return new string('*', len);

        int maskLen = Math.Max(0, len - 3);
        int visLen = Math.Min(13, len - maskLen);

        return string.Create(len, (input, maskLen, visLen), static (dst, st) =>
        {
            (string? src, int m, int v) = st;
            dst[..m]
                .Fill('*');
            src.AsSpan(m, v)
               .CopyTo(dst[m..]);
        });
    }

    /// <summary>
    /// Formats a 10-digit, 11-digit, or 12-digit phone number string into a standard US phone number format.
    /// </summary>
    /// <remarks>
    /// This method uses Span&lt;T&gt; for efficient memory usage and performance. 
    /// It assumes the input is a valid 11-digit, or 12-digit number without any formatting.
    /// </remarks>
    /// <param name="str">The unformatted 11-digit, or 12-digit phone number string.</param>
    /// <returns>The phone number formatted as (XXX) XXX-XXXX.</returns>
    [Pure]
    public static string ToDisplayPhoneNumber(this string str)
    {
        if (str.Length == 10 || (str.Length == 11 && str[0] == '1') || (str.Length == 12 && str.StartsWith("+1")))
        {
            int offset = str.Length == 10 ? 0 : str.Length == 11 ? 1 : 2;

            return string.Create(14, (str, offset), static (destination, state) =>
            {
                (string source, int start) = state;
                destination[0] = '(';
                source.AsSpan(start, 3).CopyTo(destination[1..]);
                destination[4] = ')';
                destination[5] = ' ';
                source.AsSpan(start + 3, 3).CopyTo(destination[6..]);
                destination[9] = '-';
                source.AsSpan(start + 6, 4).CopyTo(destination[10..]);
            });
        }

        throw new ArgumentException("Invalid phone number format. Expected formats: 8887737326, 18887737326, or +18887737326");
    }

    /// <summary>
    /// Sanitizes the input phone number by removing all non-numeric characters,
    /// except for a leading plus sign if present.
    /// </summary>
    /// <param name="input">The input phone number as a string.</param>
    /// <returns>
    /// A sanitized phone number string containing only digits and possibly a leading plus sign.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input is null or empty.
    /// </exception>
    [Pure]
    public static string SanitizePhoneNumber(this string input)
    {
        input.ThrowIfNullOrWhiteSpace();

        bool unchanged = true;
        for (var i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if ((uint)(c - '0') > 9u && (c != '+' || i != 0))
            {
                unchanged = false;
                break;
            }
        }

        if (unchanged)
            return input;

        if (input.Length <= _largeStackAllocThreshold)
        {
            Span<char> result = stackalloc char[input.Length];
            var idx = 0;
            for (var i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if ((uint)(c - '0') <= 9u || (c == '+' && idx == 0))
                    result[idx++] = c;
            }

            return new string(result[..idx]);
        }

        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] arr = pool.Rent(input.Length);
        try
        {
            var idx = 0;
            for (var i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if ((uint)(c - '0') <= 9u || (c == '+' && idx == 0))
                    arr[idx++] = c;
            }

            return new string(arr, 0, idx);
        }
        finally
        {
            pool.Return(arr);
        }
    }

    /// <summary>
    /// Formats a given phone number into the 'tel:+countryCode' format.
    /// </summary>
    /// <param name="phoneNumber">The phone number to be formatted. It can include non-digit characters which will be removed.</param>
    /// <param name="countryCode">The country code to be prefixed to the phone number. Defaults to 1 for the US.</param>
    /// <returns>A formatted phone number string in the 'tel:+countryCode' format.</returns>
    /// <example>
    /// <code>
    /// string formattedNumber = "123-456-7890".ToTelFormat(); // Outputs: tel:+11234567890
    /// string formattedNumberWithCountryCode = "123-456-7890".ToTelFormat(44); // Outputs: tel:+441234567890
    /// </code>
    /// </example>
    [Pure]
    public static string ToTelFormat(this string phoneNumber, int countryCode = 1)
    {
        return ToPhoneUri(phoneNumber, countryCode, "tel:");
    }

    /// <summary>
    /// Converts the given email address to a <c>mailto:</c> URI format.
    /// </summary>
    /// <param name="email">The email address to convert.</param>
    /// <returns>A string formatted as <c>mailto:[email]</c>.</returns>
    [Pure]
    public static string ToMailToFormat(this string email)
    {
        return $"mailto:{email}";
    }

    /// <summary>
    /// Converts the given phone number to an <c>sms:</c> URI format, including the specified country code.
    /// </summary>
    /// <param name="phoneNumber">The phone number to convert.</param>
    /// <param name="countryCode">The numeric country code to prefix the number with. Defaults to <c>1</c> (US).</param>
    /// <returns>A string formatted as <c>sms:+[countryCode][cleanedNumber]</c>.</returns>
    [Pure]
    public static string ToSmsFormat(this string phoneNumber, int countryCode = 1)
    {
        return ToPhoneUri(phoneNumber, countryCode, "sms:");
    }

    private static string ToPhoneUri(string phoneNumber, int countryCode, string prefix)
    {
        phoneNumber.ThrowIfNullOrWhiteSpace();

        var phoneLength = 0;
        for (var i = 0; i < phoneNumber.Length; i++)
        {
            char c = phoneNumber[i];
            if ((uint)(c - '0') <= 9u || c == '+' && i == 0)
                phoneLength++;
        }

        Span<char> countryCodeBuffer = stackalloc char[11];
        if (!countryCode.TryFormat(countryCodeBuffer, out int countryCodeLength, provider: CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Could not format the country code.");

        int length = prefix.Length + 1 + countryCodeLength + phoneLength;
        return string.Create(length, (phoneNumber, countryCode, prefix), static (destination, state) =>
        {
            state.prefix.AsSpan().CopyTo(destination);
            int position = state.prefix.Length;
            destination[position++] = '+';

            state.countryCode.TryFormat(destination[position..], out int written, provider: CultureInfo.InvariantCulture);
            position += written;

            for (var i = 0; i < state.phoneNumber.Length; i++)
            {
                char c = state.phoneNumber[i];
                if ((uint)(c - '0') <= 9u || c == '+' && i == 0)
                    destination[position++] = c;
            }
        });
    }

    /// <summary>
    /// Extracts the file extension from the given file name.
    /// </summary>
    /// <param name="fileName">The name of the file from which to extract the extension.</param>
    /// <returns>The file extension without the leading dot, in lowercase. Returns an empty string if the file name does not have an extension.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="fileName"/> is null or empty.</exception>
    [Pure]
    public static string ToFileExtension(this string fileName)
    {
        fileName.ThrowIfNullOrEmpty();

        ReadOnlySpan<char> ext = Path.GetExtension(fileName.AsSpan());
        if (ext.IsEmpty)
            return string.Empty;

        if (ext[0] == '.')
            ext = ext[1..];

        if (ext.IsEmpty)
            return string.Empty;

        return string.Create(ext.Length, ext, static (dst, src) =>
        {
            for (var i = 0; i < src.Length; i++)
                dst[i] = src[i]
                    .ToLowerInvariant(); // fast ASCII + Unicode fallback
        });
    }

    /// <summary>
    /// Converts a URI string to a filename by extracting the file name from the URI. If the provided string is not a uri, returns null.
    /// </summary>
    /// <param name="uri">The URI string to extract the file name from.</param>
    /// <returns>The file name extracted from the URI.</returns>
    /// <exception cref="UriFormatException">Thrown if the input string is not a valid URI.</exception>
    /// <example>
    /// <code>
    /// string uri = "http://www.example.com/path/to/your/file.txt";
    /// string fileName = uri.ToFilenameFromUri();
    /// Console.WriteLine(fileName); // Outputs: "file.txt"
    /// </code>
    /// </example>
    [Pure]
    public static string? ToFileNameFromUri(this string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri? uriObj))
            return null;

        return Path.GetFileName(uriObj.AbsolutePath);
    }

    /// <summary>
    /// Converts a string to a <see cref="Uri"/> if valid and absolute; otherwise returns <c>null</c>.
    /// </summary>
    /// <param name="uri">The value to convert.</param>
    /// <returns>A parsed absolute <see cref="Uri"/> when valid; otherwise, null.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri? ToUri(this string? uri)
    {
        if (TryCreateExplicitAbsoluteUri(uri, out Uri? result))
            return result;

        return null;
    }

    /// <summary>
    /// Determines whether the value is a valid absolute URI.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><c>true</c> if the value is a valid absolute URI; otherwise, <c>false</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUri(this string? value)
    {
        return TryCreateExplicitAbsoluteUri(value, out Uri? uri);
    }

    [Pure]
    private static bool TryCreateExplicitAbsoluteUri(string? value, out Uri? uri)
    {
        uri = null;

        if (!HasExplicitUriScheme(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.IsAbsoluteUri;
    }

    [Pure]
    private static bool HasExplicitUriScheme(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        int colonIndex = value.IndexOf(':');

        if (colonIndex <= 0)
            return false;

        // Treat Windows drive paths as paths, not one-letter custom URI schemes.
        if (colonIndex == 1 && value.Length > 2 && (value[2] == '\\' || value[2] == '/' && (value.Length == 3 || value[3] != '/')))
            return false;

        if (!IsUriSchemeFirstChar(value[0]))
            return false;

        for (var i = 1; i < colonIndex; i++)
        {
            if (!IsUriSchemeChar(value[i]))
                return false;
        }

        return true;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsUriSchemeFirstChar(char value)
    {
        return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsUriSchemeChar(char value)
    {
        return IsUriSchemeFirstChar(value) || value is >= '0' and <= '9' or '+' or '-' or '.';
    }

    /// <summary>
    /// Performs a lightweight HTTP/HTTPS URI check by requiring a scheme and rejecting whitespace or control characters.
    /// </summary>
    /// <param name="value">The candidate URI text.</param>
    /// <returns>True when the text passes the lightweight check.</returns>
    [Pure]
    public static bool IsHttpUriLike(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        ReadOnlySpan<char> span = value.AsSpan();

        if (!span.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !span.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        for (int i = 0; i < span.Length; i++)
        {
            if (char.IsWhiteSpace(span[i]) || char.IsControl(span[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Compares the current string with the specified string, ignoring case using ordinal comparison.
    /// </summary>
    /// <param name="str">The current string instance.</param>
    /// <param name="value">The string to compare against.</param>
    /// <returns>True if the strings are equal ignoring case; otherwise, false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string str, string value)
    {
        return str.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the beginning of the current string matches the specified string, ignoring case using ordinal comparison.
    /// </summary>
    /// <param name="str">The current string instance.</param>
    /// <param name="value">The string to compare against.</param>
    /// <returns>True if the current string starts with the specified string ignoring case; otherwise, false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithIgnoreCase(this string str, string value)
    {
        return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the end of the current string matches the specified string, ignoring case using ordinal comparison.
    /// </summary>
    /// <param name="str">The current string instance.</param>
    /// <param name="value">The string to compare against.</param>
    /// <returns>True if the current string ends with the specified string ignoring case; otherwise, false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithIgnoreCase(this string str, string value)
    {
        return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="value"/> occurs within the current string
    /// using an ordinal (case-insensitive) comparison.
    /// </summary>
    /// <param name="str">The string to search in.</param>
    /// <param name="value">The string to seek.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is found in <paramref name="str"/>; otherwise, <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsIgnoreCase(this string str, string value)
    {
        return str.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes Markdown-style triple backtick code block markers (e.g., <c>```csharp</c>) from the start and end of the string.
    /// </summary>
    /// <param name="input">The input string that may contain Markdown code block delimiters.</param>
    /// <returns>
    /// A trimmed string with the opening <c>```</c> marker (and optional language identifier) and the closing <c>```</c> removed,
    /// or the original string if it is null, empty, or whitespace.
    /// </returns>
    /// <remarks>
    /// This method trims leading and trailing whitespace, removes an optional opening Markdown code fence (e.g., <c>```js\n</c>),
    /// and also strips a closing <c>```</c> if present. The content between the markers is preserved as-is.
    /// </remarks>
    [Pure]
    public static string RemoveCodeBlockMarkers(this string input)
    {
        if (input.IsNullOrWhiteSpace())
            return input;

        ReadOnlySpan<char> raw = input.AsSpan();
        ReadOnlySpan<char> s = raw.Trim();

        // If already clean and no fences, return original reference
        bool hasOuterWs = raw.Length != s.Length;
        bool mightHaveFence = raw.Length >= 3 && (raw.StartsWith("```".AsSpan()) || raw.EndsWith("```".AsSpan()));

        if (!hasOuterWs && !mightHaveFence)
            return input;

        var start = 0;
        if (s.StartsWith("```".AsSpan()))
        {
            var i = 3;
            while (i < s.Length && s[i] != '\n' && s[i] != '\r')
                i++;
            if (i < s.Length)
            {
                if (s[i] == '\r' && i + 1 < s.Length && s[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
        }

        int end = s.Length;
        if (end >= 3 && s[(end - 3)..]
                .SequenceEqual("```".AsSpan()))
        {
            end -= 3;
            while (end > start && s[end - 1]
                       .IsWhiteSpaceFast())
                end--;
        }

        ReadOnlySpan<char> span = s.Slice(start, Math.Max(0, end - start));

        // If result equals original input, return original ref
        if (span.Length == raw.Length && span.SequenceEqual(raw))
            return input;

        return new string(span);
    }

    /// <summary>
    /// Attempts to determine the correct <see cref="Encoding"/> from a response or request
    /// <c>Content-Type</c> header string.  
    /// </summary>
    /// <param name="contentType">
    /// The Content-Type header value, such as <c>"application/json; charset=utf-16"</c>.  
    /// If <c>null</c> or no valid <c>charset</c> is specified, UTF-8 is returned.
    /// </param>
    /// <returns>
    /// A <see cref="Encoding"/> instance representing the declared <c>charset</c>, or
    /// <see cref="Encoding.UTF8"/> if no charset is specified or the charset is invalid.
    /// </returns>
    /// <remarks>
    /// This method performs lightweight parsing of the <c>charset=</c> parameter.  
    /// Any exceptions when resolving the encoding (e.g., unsupported or malformed values) 
    /// are caught and ignored, falling back to UTF-8.
    /// </remarks>
    [Pure]
    public static Encoding GetEncoding(this string? contentType)
    {
        if (contentType.IsNullOrEmpty())
            return Encoding.UTF8;

        ReadOnlySpan<char> s = contentType.AsSpan();
        int idx = s.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return Encoding.UTF8;

        ReadOnlySpan<char> rest = s[(idx + 8)..];
        int semi = rest.IndexOf(';');
        if (semi >= 0)
            rest = rest[..semi];

        rest = rest.Trim().Trim('"');

        if (rest.Equals("utf-8".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("utf8".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF8;

        if (rest.Equals("utf-16".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("utf16".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("unicode".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return Encoding.Unicode;

        if (rest.Equals("us-ascii", StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("ascii", StringComparison.OrdinalIgnoreCase))
            return Encoding.ASCII;

        if (rest.Equals("iso-8859-1", StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("latin1", StringComparison.OrdinalIgnoreCase))
            return Encoding.Latin1;

        if (rest.Equals("utf-32", StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("utf32", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF32;

        if (rest.Equals("utf-16be", StringComparison.OrdinalIgnoreCase) ||
            rest.Equals("unicodefffe", StringComparison.OrdinalIgnoreCase))
            return Encoding.BigEndianUnicode;

        try
        {
            return Encoding.GetEncoding(rest.ToString());
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}
