using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Soenneker.Extensions.ByteArray;
using Soenneker.Extensions.Stream;
using Soenneker.Utils.Random;
using Soenneker.Utils.RegexCollection;

namespace Soenneker.Extensions.String;

/// <summary>
/// A collection of useful string extension methods
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// Truncates a string to the specified length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="length">The maximum length of the truncated string.</param>
    /// <returns>The truncated string.</returns>
    [Pure]
    public static string Truncate(this string value, int length)
    {
        if (value.Length <= length)
            return value;

        string result = value.AsSpan(0, length).ToString();
        return result;
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

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (!char.IsLetterOrDigit(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a string contains only numeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is numeric, otherwise false.</returns>
    [Pure]
    public static bool IsNumeric(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return false;

        return value.All(char.IsDigit);
    }

    [Pure]
    public static double? ToDouble(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return null;

        bool successful = double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us"), out double result);

        if (successful)
            return result;

        return null;
    }

    [Pure]
    public static decimal? ToDecimal(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return null;

        bool successful = decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us"), out decimal result);

        if (successful)
            return result;

        return null;
    }

    [Pure]
    public static string RemoveNonDigits(this string? value)
    {
        if (value.IsNullOrEmpty())
            return "";

        Span<char> result = new char[value.Length];

        int index = 0;
        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                result[index] = c;
                index++;
            }
        }

        return new string(result.Slice(0, index));
    }

    [Pure]
    public static string RemoveWhiteSpace(this string? value)
    {
        if (value.IsNullOrEmpty())
            return "";

        Span<char> resultSpan = new char[value.Length];
        int index = 0;

        foreach (char c in value)
        {
            if (!char.IsWhiteSpace(c))
            {
                resultSpan[index++] = c;
            }
        }

        return new string(resultSpan.Slice(0, index));
    }


    [Pure]
    public static bool EndsWithAny(this string value, IEnumerable<string> suffixes, StringComparison comparison = StringComparison.Ordinal)
    {
        return suffixes.Any(t => value.EndsWith(t, comparison));
    }

    [Pure]
    public static bool StartsWithAny(this string value, IEnumerable<string> prefixes, StringComparison comparison = StringComparison.Ordinal)
    {
        return prefixes.Any(t => value.StartsWith(t, comparison));
    }

    [Pure]
    public static bool ContainsAny(this string value, List<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        return value.Where((t, i) => value.IndexOf(values[i], comparison) != -1).Any();
    }

    /// <summary>
    /// Determine if a string contains any of the given characters
    /// </summary>
    [Pure]
    public static bool ContainsAny(this string value, params char[]? characters)
    {
        if (value.IsNullOrEmpty() || characters == null || characters.Length == 0)
            return false;

        foreach (char t in value)
        {
            if (Array.IndexOf(characters, t) >= 0)
                return true;
        }

        return false;
    }

    /// <returns>true if any are equal</returns>
    [Pure]
    public static bool EqualsAny(this string value, StringComparison comparison = StringComparison.Ordinal, params string[] strings)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            if (string.Equals(value, strings[i], comparison))
            {
                return true;
            }
        }

        return false;
    }

    /// <returns>true if any are equal</returns>
    [Pure]
    public static bool EqualsAny(this string value, IEnumerable<string> strings, StringComparison comparison = StringComparison.Ordinal)
    {
        foreach (string test in strings)
        {
            if (value.Equals(test, comparison))
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
        if (date == null)
            return null;

        bool successful = DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime result);

        if (successful)
            return result;

        return null;
    }

    [Pure]
    public static DateTime? ToUtcDateTime(this string? value)
    {
        if (value == null)
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
    public static string FromPeriodsToDashes(this string value)
    {
        Span<char> result = new char[value.Length];

        for (int i = 0; i < value.Length; i++)
        {
            result[i] = value[i] == '.' ? '-' : value[i];
        }

        return new string(result);
    }

    [Pure]
    public static string? ToLowerFirstChar([NotNullIfNotNull(nameof(value))] this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    [Pure]
    public static string? ToUpperFirstChar([NotNullIfNotNull(nameof(value))] this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return value;

        if (value.Length == 1)
            return char.ToUpperInvariant(value[0]).ToString();

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// This expects no spaces!
    /// </summary>
    [Pure]
    public static List<string> FromCommaSeparatedToList(this string value)
    {
        List<string> list = new List<string>();
        ReadOnlySpan<char> span = value.AsSpan();
        int startIndex = 0;

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ',')
            {
                // Add the substring between the current start index and the comma
                list.Add(span.Slice(startIndex, i - startIndex).ToString());
                startIndex = i + 1;
            }
        }

        // Add the remaining substring after the last comma
        if (startIndex < span.Length)
        {
            list.Add(span.Slice(startIndex).ToString());
        }

        return list;
    }

    /// <summary>
    /// Uses UTF8 encoding
    /// </summary>
    [Pure]
    public static byte[] ToBytes(this string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    /// <summary>
    /// Use whenever a URL needs to be encoded etc.
    /// Utilizes <see cref="Uri.EscapeDataString"/>
    /// </summary>
    /// <remarks>https://stackoverflow.com/questions/602642/server-urlencode-vs-httputility-urlencode/1148326#1148326</remarks>
    [Pure]
    public static string? ToEscaped(this string? value)
    {
        if (value == null)
            return null;

        string result = Uri.EscapeDataString(value);
        return result;
    }

    /// <summary>
    /// Utilizes <see cref="Uri.UnescapeDataString"/>
    /// </summary>
    [Pure]
    public static string? ToUnescaped(this string? value)
    {
        if (value == null)
            return null;

        string result = Uri.UnescapeDataString(value);
        return result;
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

        for (int i = 0; i < length - 1; i++)
        {
            if (span[i] == '\r' && span[i + 1] == '\n')
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            return value;

        Span<char> result = new char[length];
        span.Slice(0, index).CopyTo(result);
        span.Slice(index + 1).CopyTo(result.Slice(index));

        return new string(result);
    }

    [Pure]
    public static string ToShortZipCode(this string value)
    {
        int index = value.IndexOf('-');
        return index == -1 ? value : value.AsSpan().Slice(0, index).ToString();
    }

    [Pure]
    public static string Shuffle(this string value)
    {
        char[] array = value.ToCharArray();
        int n = array.Length;

        Span<char> span = array;

        while (n > 1)
        {
            n--;
            int k = RandomUtil.Next(n + 1);

            // ReSharper disable once SwapViaDeconstruction
            char temp = span[n];
            span[n] = span[k];
            span[k] = temp;
        }

        return new string(span);
    }

    /// <summary>
    /// Securely shuffles the characters in the specified string.
    /// </summary>
    /// <param name="value">The string to shuffle.</param>
    /// <returns>A new string with the characters shuffled.</returns>
    [Pure]
    public static string SecureShuffle(this string value)
    {
        char[] array = value.ToCharArray();
        int n = array.Length;

        Span<char> span = array;

        while (n > 1)
        {
            n--;
            int k = RandomNumberGenerator.GetInt32(n + 1);

            // ReSharper disable once SwapViaDeconstruction
            char temp = span[n];
            span[n] = span[k];
            span[k] = temp;
        }

        return new string(span);
    }

    /// <summary>
    /// Shorthand for <see cref="string.IsNullOrEmpty"/>
    /// </summary>
    [Pure]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
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
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Removes the trailing character from the specified string, if it exists.
    /// </summary>
    /// <param name="value">The string to remove the trailing character from.</param>
    /// <param name="charToRemove">The character to remove from the end of the string.</param>
    /// <returns>The string with the trailing character removed, or the original string if it is null or empty.</returns>
    [Pure]
    public static string? RemoveTrailingChar([NotNullIfNotNull(nameof(value))] this string? value, char charToRemove)
    {
        if (value != null && value.Length > 0 && value[value.Length - 1] == charToRemove)
        {
            ReadOnlySpan<char> span = value.AsSpan();
            return span.Slice(0, span.Length - 1).ToString();
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
    public static string? RemoveLeadingChar([NotNullIfNotNull(nameof(value))] this string? value, char charToRemove)
    {
        if (value != null && value.Length > 0 && value[0] == charToRemove)
        {
            ReadOnlySpan<char> span = value.AsSpan();
            return span.Slice(1).ToString();
        }

        return value;
    }

    /// <summary>
    /// Converts to lowercase, and then removes/replaces characters that are invalid for URIs (does not replace accents right now)
    /// </summary>
    [Pure]
    public static string? Slugify(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        //First to lower case
        value = value.ToLowerInvariant();

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

        return (TEnum) Enum.Parse(typeof(TEnum), value, true);
    }

    [Pure]
    public static TEnum? TryToEnum<TEnum>(this string? value) where TEnum : struct, Enum
    {
        if (value.IsNullOrEmpty())
            return null;

        bool parsedSuccessfully = Enum.TryParse(typeof(TEnum), value, true, out object? rtn);

        if (parsedSuccessfully)
            return (TEnum?) rtn;

        return null;
    }

    /// <summary>
    /// Builds a MemoryStream from a string.
    /// </summary>
    /// <remarks>Preferably you should be using Soenneker.Utils.MemoryStreamUtil!</remarks>
    [Pure]
    public static async ValueTask<MemoryStream> ToMemoryStream(this string str)
    {
        var stream = new MemoryStream();

        await using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
        {
            await writer.WriteAsync(str).ConfigureAwait(false);
        }

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
        string result = Convert.FromBase64String(str).ToStr();
        return result;
    }

    /// <summary>
    /// Essentially wraps string.Split(':').
    /// </summary>
    /// <remarks>Don't use this for splitting into document/partition keys, use <see cref="ToSplitId"/> instead.</remarks>
    [Pure]
    public static List<string>? ToIds(this string? str)
    {
        if (str.IsNullOrEmpty())
            return null;

        var list = new List<string>();
        ReadOnlySpan<char> span = str.AsSpan();
        int startIndex = 0;

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ':')
            {
                list.Add(span.Slice(startIndex, i - startIndex).ToString());
                startIndex = i + 1;
            }
        }

        list.Add(span.Slice(startIndex).ToString());

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
        if (id.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(id), $"Argument '{nameof(id)}' may not be null or empty");

        ReadOnlySpan<char> idSpan = id.AsSpan();

        int colonIndex = idSpan.IndexOf(':');

        if (colonIndex == -1)
            return (id, id);

        ReadOnlySpan<char> partitionKeySpan = idSpan.Slice(0, colonIndex);
        ReadOnlySpan<char> documentIdSpan = idSpan.Slice(colonIndex + 1);

        return (partitionKeySpan.ToString(), documentIdSpan.ToString());
    }

    [Pure]
    public static string AddPartitionKey(this string documentId, string partitionKey)
    {
        return partitionKey + ":" + documentId;
    }

    [Pure]
    public static string AddDocumentId(this string partitionKey, string documentId)
    {
        return partitionKey + ":" + documentId;
    }

    /// <returns>A 32 bit int, or if null or whitespace, 0</returns>
    [Pure]
    public static int ToInt(this string? str)
    {
        if (str.IsNullOrWhiteSpace())
            return 0;

        return int.Parse(str);
    }

    /// <summary>
    /// Does not check for empty GUID, <see cref="IsValidPopulatedGuid"/> for this.
    /// </summary>
    [Pure]
    public static bool IsValidGuid(this string? input)
    {
        return input != null && Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Makes sure result is not an empty GUID.
    /// </summary>
    [Pure]
    public static bool IsValidPopulatedGuid(this string? input)
    {
        if (input == null)
            return false;

        bool success = Guid.TryParse(input, out Guid result);

        if (success && result != Guid.Empty)
            return true;

        return false;
    }

    /// <summary>
    /// Does not check for empty GUID, <see cref="IsValidPopulatedNullableGuid"/> for this.
    /// </summary>
    [Pure]
    public static bool IsValidNullableGuid(this string? input)
    {
        return input == null || Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Makes sure result is not an empty GUID.
    /// </summary>
    [Pure]
    public static bool IsValidPopulatedNullableGuid(this string? input)
    {
        if (input == null)
            return true;

        bool success = Guid.TryParse(input, out Guid result);

        if (success && result != Guid.Empty)
            return true;

        return false;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the input string is null or empty.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentException">Thrown when the input string is null or empty.</exception>
    public static void ThrowIfNullOrEmpty(this string? input, [CallerMemberName] string? name = null)
    {
        bool result = input.IsNullOrEmpty();

        if (result)
            throw new ArgumentException("String cannot be null or empty", name);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the input string is null or whitespace.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="name">The name of the calling member.</param>
    /// <exception cref="ArgumentException">Thrown when the input string is null or whitespace.</exception>
    public static void ThrowIfNullOrWhitespace(this string? input, [CallerMemberName] string? name = null)
    {
        bool result = input.IsNullOrWhiteSpace();

        if (result)
            throw new ArgumentException("String cannot be null or whitespace", name);
    }

    /// <summary>
    /// Masks sensitive information in a string by replacing a portion of characters with asterisks.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <returns>The masked string with sensitive information replaced by asterisks.</returns>
    [Pure]
    public static string Mask(this string input)
    {
        if (input.IsNullOrEmpty())
            return "";

        if (input.Length <= 6)
            return new string('*', input.Length);

        int maskLength = Math.Max(0, input.Length - 3);
        var maskedPart = new string('*', maskLength);
        string visiblePart = input.Substring(maskLength, Math.Min(13, input.Length - maskLength));
        return maskedPart + visiblePart;
    }
}