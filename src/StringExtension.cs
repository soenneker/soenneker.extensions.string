using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Soenneker.Utils.Random;

namespace Soenneker.Extensions.String;

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
    public static bool IsNumeric(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        bool result = double.TryParse(value, out double _);
        return result;
    }

    [Pure]
    public static double? ToDouble(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        bool successful = double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us"), out double result);

        if (successful)
            return result;

        return null;
    }

    [Pure]
    public static decimal? ToDecimal(this string value)
    {
        if (string.IsNullOrEmpty(value))
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

    /// <returns>true if only digits are in the string. true if string is null. false otherwise.</returns>
    [Pure]
    public static bool ContainsOnlyDigits(this string? str)
    {
        if (str == null)
            return true;

        foreach (char c in str)
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
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

    /// <returns>true if any are equal</returns>
    [Pure]
    public static bool EqualsAny(this string value, bool ignoreCase = true, params string[] strings)
    {
        return ignoreCase ? strings.Any(test => value.Equals(test, StringComparison.OrdinalIgnoreCase)) : strings.Any(value.Equals);
    }

    [Pure]
    public static string ToLowerFirstChar(this string value)
    {
        if (string.IsNullOrEmpty(value))
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


    public static string Shuffle(this string str)
    {
        char[] array = str.ToCharArray();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = RandomUtil.Next(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
    }

    public static string SecureShuffle(this string str)
    {
        char[] array = str.ToCharArray();
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
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
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
}