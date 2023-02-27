using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Soenneker.Utils.Random;
using Soenneker.Utils.RegexCollection;

namespace Soenneker.Extensions.String;

/// <summary>
/// Useful string operations
/// </summary>
public static class StringExtension
{
    [Pure]
    public static string Truncate(this string value, int length)
    {
        if (value.Length <= length)
            return value;

        string result = value[..Math.Min(length, value.Length)];
        return result;
    }

    [Pure]
    public static bool IsAlphaNumeric(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.All(char.IsLetterOrDigit);
    }

    /// <returns>false if string is null or empty</returns>
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
        if (value == null)
            return "";

        IEnumerable<char> nonDigits = value.Where(char.IsDigit);

        string result = string.Concat(nonDigits);
        return result;
    }

    [Pure]
    public static string RemoveWhiteSpace(this string? value)
    {
        if (value == null)
            return "";

        IEnumerable<char> nonWhiteSpaced = value.Where(c => !char.IsWhiteSpace(c));

        string result = string.Concat(nonWhiteSpaced);
        return result;
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
        if (string.IsNullOrEmpty(value) || characters == null || characters.Length == 0)
            return false;

        foreach (var t in value)
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
        return strings.Any(test => value.Equals(test, comparison));
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
        string result = value.Replace('.', '-');
        return result;
    }

    [Pure]
    public static string ToLowerFirstChar(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        string result = char.ToLower(value[0]) + value[1..];
        return result;
    }

    /// <summary>
    /// Expects no spaces!
    /// </summary>
    [Pure]
    public static List<string> FromCommaSeparatedToList(this string value)
    {
        List<string> list = value.Split(',').ToList();
        return list;
    }

    /// <summary>
    /// Uses UTF8 encoding
    /// </summary>
    [Pure]
    public static byte[] ToBytes(this string value)
    {
        var utf8 = new UTF8Encoding();
        byte[] result = utf8.GetBytes(value);
        return result;
    }

    /// <summary> Utilizes <see cref="Uri.EscapeDataString"/></summary>
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
    public static string ToUnescaped(this string value)
    {
        string result = Uri.UnescapeDataString(value);
        return result;
    }

    /// <summary>
    /// Replaces "\r\n" with "\n"
    /// </summary>
    [Pure]
    public static string ToUnixLineBreaks(this string value)
    {
        string result = value.Replace("\r\n", "\n");
        return result;
    }

    [Pure]
    public static string ToShortZipCode(this string value)
    {
        if (!value.Contains('-'))
            return value;

        string result = value.Split('-')[0];
        return result;
    }

    [Pure]
    public static string Shuffle(this string value)
    {
        char[] array = value.ToCharArray();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = RandomUtil.Next(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
    }

    [Pure]
    public static string SecureShuffle(this string value)
    {
        char[] array = value.ToCharArray();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = RandomNumberGenerator.GetInt32(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
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
    /// Shorthand for <see cref="string.IsNullOrWhiteSpace"/>
    /// </summary>
    [Pure]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    [Pure]
    public static string? RemoveTrailingChar([NotNullIfNotNull(nameof(value))] this string? value, char charToRemove)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value.EndsWith(charToRemove))
        {
            return value[..^1];
        }

        return value;
    }

    [Pure]
    public static string? RemoveLeadingChar([NotNullIfNotNull(nameof(value))] this string? value, char charToRemove)
    {
        if (value.IsNullOrEmpty())
            return value;

        if (value.StartsWith(charToRemove))
        {
            return value.Substring(1, value.Length - 1);
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
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"Empty/null string was attempted to convert to enum of type {typeof(TEnum)}", nameof(value));

        return (TEnum) Enum.Parse(typeof(TEnum), value, true);
    }

    [Pure]
    public static TEnum? TryToEnum<TEnum>(this string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        bool parsedSuccessfully = Enum.TryParse(typeof(TEnum), value, true, out object? rtn);

        if (parsedSuccessfully)
            return (TEnum?) rtn;

        return null;
    }

    /// <summary>
    /// Builds a MemoryStream from a string. There's no need to dispose of this after using.
    /// </summary>
    /// <remarks>Preferably you should be using Soenneker.Utils.MemoryStreamUtil!</remarks>
    [Pure]
    public static async ValueTask<MemoryStream> ToMemoryStream(this string str)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(str);
        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }
}