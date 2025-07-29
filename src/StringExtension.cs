using Soenneker.Extensions.Arrays.Bytes;
using Soenneker.Extensions.Char;
using Soenneker.Extensions.Stream;
using Soenneker.Utils.Random;
using Soenneker.Utils.RegexCollection;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        ReadOnlySpan<char> span = value.AsSpan();
        int length = span.Length;

        char[]? buffer = null;
        var idx = 0;

        for (var i = 0; i < length; i++)
        {
            char c = span[i];

            if (c.IsDigit())
            {
                if (buffer != null)
                    buffer[idx++] = c;
            }
            else
            {
                if (buffer == null)
                {
                    // ❗ First non-digit detected: allocate buffer, copy previous valid chars
                    buffer = GC.AllocateUninitializedArray<char>(length);
                    span.Slice(0, i).CopyTo(buffer);
                    idx = i;
                }
            }
        }

        // No non-digit characters? Return original reference (no alloc)
        if (buffer == null)
            return value;

        // All removed? Return empty
        if (idx == 0)
            return "";

        // Return new string from valid digits
        return new string(buffer, 0, idx);
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

        ReadOnlySpan<char> span = value.AsSpan();
        int length = span.Length;

        char[]? buffer = null;
        var idx = 0;

        for (var i = 0; i < length; i++)
        {
            char c = span[i];

            if (!c.IsWhiteSpaceFast())
            {
                if (buffer != null)
                    buffer[idx++] = c;
            }
            else
            {
                if (buffer == null)
                {
                    buffer = GC.AllocateUninitializedArray<char>(length);
                    span.Slice(0, i).CopyTo(buffer);
                    idx = i;
                }
            }
        }

        if (buffer == null)
            return value;

        if (idx == 0)
            return "";

        return new string(buffer, 0, idx);
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

        ReadOnlySpan<char> span = value.AsSpan();
        int length = span.Length;

        char[]? buffer = null;
        var idx = 0;

        for (var i = 0; i < length; i++)
        {
            char c = span[i];

            if (c != removeChar)
            {
                if (buffer != null)
                    buffer[idx++] = c;
            }
            else
            {
                if (buffer == null)
                {
                    buffer = GC.AllocateUninitializedArray<char>(length);
                    span.Slice(0, i).CopyTo(buffer);
                    idx = i;
                }
            }
        }

        if (buffer == null)
            return value;

        if (idx == 0)
            return "";

        return new string(buffer, 0, idx);
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
        for (var i = 0; i < values.Count; i++)
        {
            if (value.IndexOf(values[i], comparison) != -1)
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

        for (var i = 0; i < value.Length; i++)
        {
            if (Array.IndexOf(characters, value[i]) >= 0)
                return true;
        }

        return false;
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
        for (var i = 0; i < strings.Length; i++)
        {
            if (string.Equals(value, strings[i], comparison))
            {
                return true;
            }
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
        int length = value.Length;

        if (length == 0)
            return "";

        Span<char> buffer = length <= 128 // Inline threshold (optional tuning)
            ? stackalloc char[length]
            : new char[length]; // fallback for large strings, no pooling to keep it simple

        ReadOnlySpan<char> input = value;

        for (var i = 0; i < length; i++)
        {
            buffer[i] = input[i] == '.' ? '-' : input[i];
        }

        return new string(buffer);
    }

    /// <summary>
    /// Replaces whitespace with dashes
    /// </summary>
    [Pure]
    public static string ToDashesFromWhiteSpace(this string value)
    {
        int length = value.Length;

        if (length == 0)
            return "";

        Span<char> buffer = length <= 128 ? stackalloc char[length] : new char[length]; // Heap fallback for large strings

        ReadOnlySpan<char> input = value;

        for (var i = 0; i < length; i++)
        {
            char c = input[i];
            buffer[i] = c.IsWhiteSpaceFast() ? '-' : c;
        }

        return new string(buffer);
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

        List<string> list = [];
        ReadOnlySpan<char> span = value.AsSpan();

        var startIndex = 0;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == ',')
            {
                if (i > startIndex) // Avoid empty strings from consecutive commas
                {
                    list.Add(new string(span.Slice(startIndex, i - startIndex)));
                }

                startIndex = i + 1;
            }
        }

        // Add the remaining substring after the last comma
        if (startIndex < span.Length)
        {
            list.Add(new string(span.Slice(startIndex)));
        }

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
        ReadOnlySpan<char> span = value.AsSpan();
        int length = span.Length;
        int index = -1;

        // Find the first occurrence of '\r\n'
        for (var i = 0; i < length - 1; i++)
        {
            if (span[i] == '\r' && span[i + 1] == '\n')
            {
                index = i;
                break;
            }
        }

        // If no '\r\n' found, return the original string
        if (index == -1)
            return value;

        // Determine whether to use stackalloc or ArrayPool
        if (length <= _stackallocThreshold)
        {
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[length - 1];

            // Copy the initial part before '\r\n'
            span.Slice(0, index).CopyTo(buffer);

            // Copy the remaining part after '\r\n'
            span.Slice(index + 1).CopyTo(buffer.Slice(index));

            return new string(buffer);
        }

        // Use ArrayPool for larger strings
        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rentedBuffer = pool.Rent(length - 1);

        Span<char> bufferSpan = rentedBuffer.AsSpan(0, length - 1);

        // Copy the initial part before '\r\n'
        span.Slice(0, index).CopyTo(bufferSpan);

        // Copy the remaining part after '\r\n'
        span.Slice(index + 1).CopyTo(bufferSpan.Slice(index));

        var result = new string(bufferSpan);

        // Explicitly return the buffer to the pool
        pool.Return(rentedBuffer);

        return result;
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
        Encoding.UTF8.GetBytes(str.AsSpan(), buffer.AsSpan());

        // Wrap the buffer in a read-only MemoryStream.
        // The 'publiclyVisible' parameter (last bool) allows direct array access via GetBuffer(); 
        var stream = new MemoryStream(buffer, 0, buffer.Length);
        stream.ToStart();
        return stream;
    }

    /// <summary>
    /// Takes a Base64 encoded string, converts it to a byte array, and then converts it to a UTF8 string.
    /// </summary>
    /// <remarks>Equivalent to <code>Convert.FromBase64String(str).ToStr()</code></remarks>
    [Pure]
    public static string ToStringFromEncoded64(this string str)
    {
        return str.ToBytesFromBase64().ToStr();
    }

    /// <summary>
    /// Essentially wraps string.Split(':').
    /// </summary>
    /// <remarks>Don't use this for splitting into document/partition keys, use <see cref="ToSplitId"/> instead.</remarks>
    [Pure]
    public static List<string>? ToIds(this string? value)
    {
        int length = value?.Length ?? 0;

        if (length == 0)
            return null;

        if (length <= _stackallocThreshold)
        {
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[length];
            return ParseIds(value.AsSpan(), buffer);
        }

        // Use ArrayPool for larger strings
        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rentedBuffer = pool.Rent(length);

        Span<char> bufferSpan = rentedBuffer.AsSpan(0, length);
        List<string> result = ParseIds(value.AsSpan(), bufferSpan);

        // Return the buffer to the pool after use
        pool.Return(rentedBuffer);

        return result;
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
        if (input is null)
            throw new ArgumentNullException(name, "String cannot be null");

        if (input.Length == 0)
            throw new ArgumentException("String cannot be empty", name);
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
        int length = input?.Length ?? 0;

        if (length == 0)
            return "";

        if (length <= 6)
            return new string('*', length);

        int maskLength = Math.Max(0, length - 3);
        int visibleLength = Math.Min(13, length - maskLength);

        if (length <= _stackallocThreshold)
        {
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[length];

            // Fill masked part with '*'
            buffer.Slice(0, maskLength).Fill('*');

            // Copy the visible part
            input.AsSpan(maskLength, visibleLength).CopyTo(buffer.Slice(maskLength));

            return new string(buffer);
        }

        // Use ArrayPool for larger strings
        ArrayPool<char> pool = ArrayPool<char>.Shared;
        char[] rentedBuffer = pool.Rent(length);

        Span<char> bufferSpan = rentedBuffer.AsSpan(0, length);

        // Fill masked part with '*'
        bufferSpan.Slice(0, maskLength).Fill('*');

        // Copy the visible part
        input.AsSpan(maskLength, visibleLength).CopyTo(bufferSpan.Slice(maskLength));

        var result = new string(bufferSpan.Slice(0, length));

        // Explicitly return the rented buffer
        pool.Return(rentedBuffer);

        return result;
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

        ReadOnlySpan<char> span = input.AsSpan().Trim();

        // Remove opening code block marker (```lang\n or ```\n)
        Match match = RegexCollection.MarkdownCodeFence().Match(span.ToString());

        if (match is {Success: true, Index: 0})
            span = span.Slice(match.Length);

        // Remove closing code block if present at the end
        if (span.EndsWith("```".AsSpan()))
        {
            span = span.Slice(0, span.LastIndexOf("```".AsSpan(), StringComparison.Ordinal));
            span = span.TrimEnd();
        }

        return span.ToString();
    }
}