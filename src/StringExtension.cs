using Soenneker.Extensions.Char;
using Soenneker.Utils.Random;
using Soenneker.Utils.RegexCollection;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
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

        // If the requested length <= 0, return empty (or you could throw, depending on your needs).
        if (length <= 0)
            return "";

        // Allocate a new string from the first 'length' characters.
        return new string(value.AsSpan(0, length));
    }

    /// <summary>
    /// Determines whether a string contains only alphanumeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is alphanumeric, otherwise false.</returns>
    [Pure]
    public static bool IsAlphaNumeric(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (!value[i].IsLetterOrDigitFast())
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

        // 1) Count digits
        var outLen = 0;

        for (var i = 0; i < s.Length; i++)
        {
            if (s[i].IsDigit())
                outLen++;
        }

        // 2) No non-digits? Return original ref
        if (outLen == s.Length)
            return value;

        if (outLen == 0)
            return "";

        // 3) Create and fill
        return string.Create(outLen, value, static (dst, src) =>
        {
            ReadOnlySpan<char> sp = src.AsSpan();
            var w = 0;

            for (var i = 0; i < sp.Length; i++)
            {
                char c = sp[i];

                if (c.IsDigit())
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

        ReadOnlySpan<char> s = value.AsSpan();

        // 1) Count non-whitespace
        var outLen = 0;

        for (var i = 0; i < s.Length; i++)
        {
            if (!char.IsWhiteSpace(s[i]))
                outLen++;
        }

        // 2) Fast paths
        if (outLen == s.Length)
            return value;

        if (outLen == 0)
            return "";

        // 3) Create and fill
        return string.Create(outLen, value, static (dst, src) =>
        {
            ReadOnlySpan<char> sp = src.AsSpan();
            var w = 0;
            for (var i = 0; i < sp.Length; i++)
            {
                char c = sp[i];

                if (!char.IsWhiteSpace(c))
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

        ReadOnlySpan<char> s = value.AsSpan();

        // 1) Count survivors
        var outLen = 0;

        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != removeChar)
                outLen++;
        }

        // 2) Fast paths
        if (outLen == s.Length)
            return value;

        if (outLen == 0)
            return string.Empty;

        // 3) Create and fill
        return string.Create(outLen, (value, removeChar), static (dst, st) =>
        {
            (string src, char rm) = st;
            ReadOnlySpan<char> sp = src.AsSpan();
            var w = 0;

            for (var i = 0; i < sp.Length; i++)
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
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RemoveDashes(this string? value)
    {
        return RemoveAllChar(value, '-');
    }

    /// <summary>
    /// Determines whether the string ends with any of the specified suffixes.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="suffixes">An enumerable collection of suffixes to check against.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if the string ends with any of the specified suffixes; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool EndsWithAny(this string value, IEnumerable<string> suffixes, StringComparison comparison = StringComparison.Ordinal)
    {
        if (value.IsNullOrEmpty())
            return false;

        foreach (string suffix in suffixes)
        {
            if (!suffix.IsNullOrEmpty() && value.EndsWith(suffix, comparison))
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
    public static bool StartsWithAny(this string value, IEnumerable<string> prefixes, StringComparison comparison = StringComparison.Ordinal)
    {
        if (value.IsNullOrEmpty())
            return false;

        foreach (string prefix in prefixes)
        {
            if (!prefix.IsNullOrEmpty() && value.StartsWith(prefix, comparison))
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
        return value.AsSpan().IndexOfAny(characters) >= 0;
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
        if (strings == null || strings.Length == 0)
            return false;

        StringComparer comparer = StringComparer.FromComparison(comparison);
        int n = strings.Length;

        for (var i = 0; i < n; i++)
        {
            string s = strings[i];

            if (comparer.Equals(value, s))
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
    [Pure]
    public static DateTime? ToDateTime(this string? date)
    {
        if (date is null)
            return null;

        bool successful = DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime result);

        if (successful)
            return result;

        return null;
    }

    [Pure]
    public static DateTime? ToUtcDateTime(this string? value)
    {
        if (value is null)
            return null;

        bool successful = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime result);

        if (successful)
            return result;

        return null;
    }

    /// <summary>
    /// Replaces periods with dashes
    /// </summary>
    [Pure]
    public static string ToDashesFromPeriods(this string value)
    {
        int first = value.IndexOf('.');

        if (first < 0)
            return value; // return original ref

        return string.Create(value.Length, (value, first), static (Span<char> dst, (string src, int start) st) =>
        {
            (string src, int start) = st;
            // copy the prefix unchanged
            src.AsSpan(0, start).CopyTo(dst);

            // from first '.' onward, replace per char
            ReadOnlySpan<char> s = src.AsSpan();

            for (int i = start; i < s.Length; i++)
            {
                dst[i] = s[i] == '.' ? '-' : s[i];
            }
        });
    }

    /// <summary>
    /// Replaces whitespace with dashes
    /// </summary>
    [Pure]
    public static string ToDashesFromWhiteSpace(this string value)
    {
        int first = -1;
        ReadOnlySpan<char> s = value.AsSpan();
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i].IsWhiteSpaceFast())
            {
                first = i;
                break;
            }
        }

        if (first < 0)
            return value; // return original ref

        return string.Create(value.Length, (value, first), static (Span<char> dst, (string src, int start) st) =>
        {
            (string src, int start) = st;
            src.AsSpan(0, start).CopyTo(dst);

            ReadOnlySpan<char> ss = src.AsSpan();

            for (int i = start; i < ss.Length; i++)
            {
                dst[i] = ss[i].IsWhiteSpaceFast() ? '-' : ss[i];
            }
        });
    }

    /// <summary>
    /// Splits a comma-separated string into a list of substrings. Spaces are not expected.
    /// </summary>
    /// <param name="value">The comma-separated string to split.</param>
    /// <returns>A list containing the substrings separated by commas.</returns>
    [Pure]
    public static List<string> FromCommaSeparatedToList(this string value)
    {
        if (value.IsNullOrEmpty())
            return [];

        // no commas? just return [value]
        int first = value.IndexOf(',');
        if (first < 0)
            return [value];

        // rough capacity (upper bound). ok if we skip empties later.
        var commas = 1; // we already found one'

        for (int i = first + 1; i < value.Length; i++)
        {
            if (value[i] == ',')
                commas++;
        }

        var list = new List<string>(commas + 1);

        ReadOnlySpan<char> s = value.AsSpan();
        var start = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                if (i > start) // skip empty segments from ",,"
                    list.Add(new string(s.Slice(start, i - start)));
                start = i + 1;
            }
        }

        if (start < s.Length) // tail segment
            list.Add(new string(s.Slice(start)));

        return list;
    }

    /// <summary>
    /// Equivalent to Encoding.UTF8.GetBytes(value)
    /// </summary>
    [Pure]
    public static byte[] ToBytes(this string str)
    {
        if (str.IsNullOrEmpty())
            return [];

        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// <inheritdoc cref="Convert.FromBase64String(string)"/>
    /// </summary>
    /// <remarks>Equivalent to Convert.FromBase64String(value)</remarks>
    /// <param name="value"></param>
    /// <returns></returns>
    [Pure]
    public static byte[] ToBytesFromBase64(this string value)
    {
        // Handle null or empty up front.
        if (value.IsNullOrEmpty())
            return [];

        return Convert.FromBase64String(value);
    }

    /// <summary>
    /// <inheritdoc cref="Convert.FromHexString(string)"/>
    /// </summary>
    /// <remarks>Equivalent to Convert.FromHexString(value)</remarks>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static byte[] ToBytesFromHex(this string hex)
    {
        if (hex.IsNullOrEmpty())
            return [];

        return Convert.FromHexString(hex);
    }

    /// <summary>
    /// Replaces "\r\n" with "\n"
    /// </summary>
    [Pure]
    public static string ToUnixLineBreaks(this string value)
    {
        ReadOnlySpan<char> s = value;
        var pairs = 0;

        for (var i = 0; i + 1 < s.Length; i++)
        {
            if (s[i] == '\r' && s[i + 1] == '\n')
                pairs++;
        }

        if (pairs == 0)
            return value; // return original ref

        int outLen = s.Length - pairs;

        // state is the original string (class, OK) — not a ReadOnlySpan<char>
        return string.Create(outLen, value, static (dst, src) =>
        {
            ReadOnlySpan<char> ss = src.AsSpan();
            var w = 0;
            for (var r = 0; r < ss.Length; r++)
            {
                if (r + 1 < ss.Length && ss[r] == '\r' && ss[r + 1] == '\n')
                {
                    dst[w++] = '\n';
                    r++; // skip '\n'
                }
                else
                {
                    dst[w++] = ss[r];
                }
            }
        });
    }

    /// <summary>
    /// Extracts the short form of a zip code by removing any characters after the hyphen (if present).
    /// </summary>
    /// <param name="value">The input zip code string.</param>
    /// <returns>The short form of the zip code.</returns>
    [Pure]
    public static string ToShortZipCode(this string value)
    {
        int index = value.IndexOf('-');
        return index == -1 ? value : new string(value.AsSpan().Slice(0, index));
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
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[length];
            value.AsSpan().CopyTo(buffer);

            PerformShuffle(buffer);
            return new string(buffer);
        }

        // Use ArrayPool for larger strings
        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rentedBuffer = pool.Rent(length);

        Span<char> spanBuffer = rentedBuffer.AsSpan(0, length);
        value.AsSpan().CopyTo(spanBuffer);

        PerformShuffle(spanBuffer);

        var result = new string(spanBuffer);

        // Explicitly return the buffer to the pool
        pool.Return(rentedBuffer);

        return result;
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
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[length];
            value.AsSpan().CopyTo(buffer);

            PerformSecureShuffle(buffer);
            return new string(buffer);
        }

        // Use ArrayPool for larger strings
        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rentedBuffer = pool.Rent(length);

        Span<char> spanBuffer = rentedBuffer.AsSpan(0, length);
        value.AsSpan().CopyTo(spanBuffer);

        PerformSecureShuffle(spanBuffer);

        var result = new string(spanBuffer);

        // Explicitly return the buffer to the pool
        pool.Return(rentedBuffer);

        return result;
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
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        // Perform a null check first, then check Length for efficiency
        // and to ensure minimal instruction execution in the common case.
        return value is null || value.Length == 0;
    }

    /// <summary>
    /// Shorthand for value == ""/>
    /// </summary>
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
    [Pure]
    public static bool HasContent([NotNullWhen(true)] this string? value)
    {
        return !value.IsNullOrEmpty();
    }

    /// <summary>
    /// Shorthand for <see cref="string.IsNullOrWhiteSpace"/>
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        if (value.IsNullOrEmpty())
            return true;

        return value.IsWhiteSpace();
    }

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
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Slugify(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        //First to lower case
        value = value.ToLowerInvariantFast();

        //Replace spaces
        value = RegexCollection.Spaces().Replace(value, "-");

        //Remove invalid chars
        value = RegexCollection.AlphaNumericAndDashUnderscore().Replace(value, "");

        //Trim dashes from end
        value = value.Trim('-', '_');

        //Replace double occurrences of - or _
        value = RegexCollection.DoubleOccurrencesOfDashUnderscore().Replace(value, "$1");

        return value;
    }

    /// <summary>
    /// Ignores the case of the string being passed in. 
    /// </summary>
    /// <exception cref="ArgumentException">If parsing fails</exception>
    [Pure]
    public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum
    {
        if (value.IsNullOrEmpty())
            throw new ArgumentException($"Empty/null string was attempted to convert to enum of type {typeof(TEnum)}", nameof(value));

        return Enum.Parse<TEnum>(value, true);
    }

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
    [Pure]
    public static MemoryStream ToMemoryStream(this string str)
    {
        // Short-circuit for null/empty to avoid unnecessary allocations.
        if (str.IsNullOrEmpty())
            return new MemoryStream([]);

        // Determine how many bytes are needed in UTF-8.
        int byteCount = Encoding.UTF8.GetByteCount(str);

        // Allocate one uninitialized array for the exact size needed.
        byte[] buffer = GC.AllocateUninitializedArray<byte>(byteCount);

        // Encode directly into the buffer.
        Encoding.UTF8.GetBytes(str.AsSpan(), buffer);

        // Wrap the buffer in a read-only MemoryStream.
        // The 'publiclyVisible' parameter (last bool) allows direct array access via GetBuffer(); 
        return new MemoryStream(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Takes a Base64 encoded string, converts it to a byte array, and then converts it to a UTF8 string.
    /// </summary>
    /// <remarks>Equivalent to <code>Convert.FromBase64String(str).ToStr()</code></remarks>
    [Pure]
    public static string ToStringFromBase64(this string s)
    {
        if (s.IsNullOrWhiteSpace())
            return "";

        // Normalize Base64URL (-,_ ) -> Base64 (+,/), add padding if needed
        if (s.IndexOf('-') >= 0 || s.IndexOf('_') >= 0)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            int pad = s.Length % 4;

            if (pad == 2)
                s += "==";
            else if (pad == 3)
                s += "=";
            else if (pad == 1)
                throw new FormatException("Invalid Base64URL length.");
        }

        int maxLen = s.Length * 3 / 4; // upper bound
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxLen);

        try
        {
            if (!Convert.TryFromBase64String(s, buffer, out int bytesWritten))
                throw new FormatException("Invalid Base64 data.");

            return Encoding.UTF8.GetString(buffer, 0, bytesWritten);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Takes a UTF8 string, converts it to a byte array using UTF8 encoding, and then converts that byte array to a Base64 encoded string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToBase64(this string str)
    {
        ReadOnlySpan<char> chars = str;
        int utf8Len = Encoding.UTF8.GetByteCount(chars);

        byte[]? utf8Array = null;
        Span<byte> utf8 = utf8Len <= 512 ? stackalloc byte[utf8Len] : (utf8Array = ArrayPool<byte>.Shared.Rent(utf8Len));

        try
        {
            Encoding.UTF8.GetBytes(chars, utf8);

            int max64 = Base64.GetMaxEncodedToUtf8Length(utf8Len);

            byte[]? b64Array = null;
            Span<byte> b64 = max64 <= 1024 ? stackalloc byte[max64] : (b64Array = ArrayPool<byte>.Shared.Rent(max64));

            try
            {
                Base64.EncodeToUtf8(utf8.Slice(0, utf8Len), b64, out _, out int written);
                return Encoding.ASCII.GetString(b64.Slice(0, written));
            }
            finally
            {
                if (b64Array != null)
                    ArrayPool<byte>.Shared.Return(b64Array);
            }
        }
        finally
        {
            if (utf8Array != null)
                ArrayPool<byte>.Shared.Return(utf8Array);
        }
    }

    /// <summary>
    /// Essentially wraps string.Split(':').
    /// </summary>
    /// <remarks>Don't use this for splitting into document/partition keys, use <see cref="ToSplitId"/> instead.</remarks>
    [Pure]
    public static List<string>? ToIds(this string? value)
    {
        if (value.IsNullOrEmpty())
            return null;

        ReadOnlySpan<char> s = value.AsSpan();
        var list = new List<string>(Math.Max(1, s.Length / 8)); // tiny pre-size guess

        var start = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == ':')
            {
                list.Add(new string(s.Slice(start, i - start)));
                start = i + 1;
            }
        }

        list.Add(new string(s.Slice(start)));
        return list;
    }

    // Helper method to parse IDs into a list
    private static List<string> ParseIds(ReadOnlySpan<char> span, Span<char> buffer)
    {
        var list = new List<string>();
        var startIndex = 0;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == ':')
            {
                int segmentLength = i - startIndex;

                // Copy the current segment to the buffer and add to the list
                span.Slice(startIndex, segmentLength).CopyTo(buffer.Slice(0, segmentLength));
                list.Add(new string(buffer.Slice(0, segmentLength)));

                startIndex = i + 1;
            }
        }

        // Handle the last segment after the final colon
        int remainingLength = span.Length - startIndex;
        span.Slice(startIndex).CopyTo(buffer.Slice(0, remainingLength));
        list.Add(new string(buffer.Slice(0, remainingLength)));

        return list;
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
    [Pure]
    public static (string PartitionKey, string DocumentId) ToSplitId(this string id)
    {
        ThrowIfNullOrEmpty(id);

        for (int i = id.Length - 1; i >= 0; i--)
        {
            if (id[i] == ':')
            {
                // If colon is at position 0, partition key is empty
                if (i == 0)
                {
                    // Edge case: If the string is just ":" or ":something"
                    string documentId = id.Length == 1 ? "" : new string(id.AsSpan(1));
                    return ("", documentId);
                }

                // If colon is at the very end, document ID is empty
                if (i == id.Length - 1)
                {
                    // Edge case: If the string is "something:"
                    string partitionKey = id.Length == 1
                        ? "" // means ":", but that was caught above
                        : new string(id.AsSpan(0, id.Length - 1));
                    return (partitionKey, "");
                }

                // General case: partitionKey:documentId
                var partitionKeyGeneral = new string(id.AsSpan(0, i));
                var documentIdGeneral = new string(id.AsSpan(i + 1));
                return (partitionKeyGeneral, documentIdGeneral);
            }
        }

        // No colon found
        return (id, id);
    }

    /// <summary>
    /// Concatenates the partition key and document ID with a colon separator using a stack-allocated buffer for optimized performance.
    /// </summary>
    /// <param name="documentId">The document ID to concatenate. Cannot be null.</param>
    /// <param name="partitionKey">The partition key to concatenate. Cannot be null.</param>
    /// <returns>A concatenated string in the format "partitionKey:documentId" using stack allocation to minimize memory usage and improve performance.</returns>
    [Pure]
    public static string AddPartitionKey(this string documentId, string partitionKey)
    {
        int totalLength = partitionKey.Length + 1 + documentId.Length;
        Span<char> result = stackalloc char[totalLength];

        partitionKey.AsSpan().CopyTo(result);
        result[partitionKey.Length] = ':';
        documentId.AsSpan().CopyTo(result.Slice(partitionKey.Length + 1));

        return new string(result);
    }

    /// <summary>
    /// Concatenates the partition key and document ID with a colon separator using a stack-allocated buffer for optimized performance.
    /// </summary>
    /// <param name="partitionKey">The partition key to concatenate. Cannot be null.</param>
    /// <param name="documentId">The document ID to concatenate. Cannot be null.</param>
    /// <returns>A concatenated string in the format "partitionKey:documentId" using stack allocation to minimize memory usage and improve performance.</returns>
    [Pure]
    public static string AddDocumentId(this string partitionKey, string documentId)
    {
        int totalLength = partitionKey.Length + 1 + documentId.Length;
        Span<char> result = stackalloc char[totalLength];

        partitionKey.AsSpan().CopyTo(result);
        result[partitionKey.Length] = ':';
        documentId.AsSpan().CopyTo(result.Slice(partitionKey.Length + 1));

        return new string(result);
    }

    /// <summary>
    /// Converts a string to a boolean value. Accepts "true", "false", "1", "0" (case-insensitive).
    /// Returns false if input is null, empty, or unrecognized.
    /// </summary>
    public static bool ToBool(this string? value)
    {
        ReadOnlySpan<char> span = value.AsSpan().Trim();

        if (bool.TryParse(span, out bool result))
            return result;

        if (span is "1")
            return true;

        return false;
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
        if (len == 0) return "";
        if (len <= 6) return new string('*', len);

        int maskLen = Math.Max(0, len - 3);
        int visLen = Math.Min(13, len - maskLen);

        return string.Create(len, (input, maskLen, visLen), static (dst, st) =>
        {
            (string? src, int m, int v) = st;
            dst.Slice(0, m).Fill('*');
            src.AsSpan(m, v).CopyTo(dst.Slice(m));
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
            Span<char> spanNumber = stackalloc char[14]; // Pre-allocated for the final format
            int offset = str.Length == 10 ? 0 : str.Length == 11 ? 1 : 2;

            spanNumber[0] = '(';
            str.AsSpan(offset, 3).CopyTo(spanNumber.Slice(1, 3));
            spanNumber[4] = ')';
            spanNumber[5] = ' ';
            str.AsSpan(offset + 3, 3).CopyTo(spanNumber.Slice(6, 3));
            spanNumber[9] = '-';
            str.AsSpan(offset + 6, 4).CopyTo(spanNumber.Slice(10, 4));

            return new string(spanNumber);
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

        Span<char> result = stackalloc char[input.Length];
        var index = 0;

        for (var i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c.IsDigit() || (c == '+' && index == 0))
            {
                result[index++] = c;
            }
        }

        return new string(result.Slice(0, index));
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
        string cleanedNumber = SanitizePhoneNumber(phoneNumber);

        return $"tel:+{countryCode}{cleanedNumber}";
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
        string cleanedNumber = SanitizePhoneNumber(phoneNumber);

        return $"sms:+{countryCode}{cleanedNumber}";
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

        string extension = Path.GetExtension(fileName);

        if (extension.IsNullOrEmpty())
            return "";

        // Remove the leading '.' and convert to lower case
        return extension[0] == '.' ? extension.AsSpan(1).ToString().ToLowerInvariantFast() : extension.ToLowerInvariantFast();
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
    /// Compares the current string with the specified string, ignoring case using ordinal comparison.
    /// </summary>
    /// <param name="str">The current string instance.</param>
    /// <param name="value">The string to compare against.</param>
    /// <returns>True if the strings are equal ignoring case; otherwise, false.</returns>
    [Pure]
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

        ReadOnlySpan<char> s = input.AsSpan().Trim();

        // opening: ```[lang]? \r?\n
        var start = 0;
        if (s.StartsWith("```".AsSpan()))
        {
            var i = 3;
            while (i < s.Length && s[i] != '\n' && s[i] != '\r') i++;
            if (i < s.Length)
            {
                if (s[i] == '\r' && i + 1 < s.Length && s[i + 1] == '\n') i++;
                start = i + 1;
            }
        }

        // closing: trailing ```
        int end = s.Length;
        if (end >= 3 && s.Slice(end - 3).SequenceEqual("```".AsSpan()))
        {
            end -= 3;
            // trim trailing whitespace before ```
            while (end > start && char.IsWhiteSpace(s[end - 1])) end--;
        }

        ReadOnlySpan<char> span = s.Slice(start, Math.Max(0, end - start));
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
    public static Encoding GetEncoding(this string? contentType)
    {
        if (contentType.IsNullOrEmpty())
            return Encoding.UTF8;

        ReadOnlySpan<char> s = contentType.AsSpan();
        int idx = s.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);

        if (idx < 0) 
            return Encoding.UTF8;

        ReadOnlySpan<char> rest = s.Slice(idx + "charset=".Length);
        int semi = rest.IndexOf(';');

        if (semi >= 0) 
            rest = rest.Slice(0, semi);

        rest = rest.Trim();

        try
        {
            return Encoding.GetEncoding(rest.ToString()); // unavoidable lookup string
        }
        catch
        {
            return Encoding.UTF8;
        }
    }
}