using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Soenneker.Culture.English.US;
using Soenneker.Extensions.Char;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Determines whether a string contains only numeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is numeric, otherwise false.</returns>
    [Pure]
    public static bool IsNumeric(this string? value)
    {
        ReadOnlySpan<char> span = value;

        if (span.IsEmpty)
            return false;

        for (var i = 0; i < span.Length; i++)
        {
            if (!span[i].IsDigit())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the string representation of a number to its nullable double-precision floating-point equivalent.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A <see cref="Nullable{Double}"/> that represents the converted nullable double-precision floating-point number if the conversion succeeds; otherwise, <c>null</c>.</returns>
    [Pure]
    public static double? ToDouble(this string? value)
    {
        ReadOnlySpan<char> s = value.AsSpan().Trim();

        if (s.IsEmpty)
            return null;

        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite |
                                    NumberStyles.AllowTrailingWhite;

        return double.TryParse(s, styles, CultureEnUsCache.Instance, out double result) ? result : null;
    }


    /// <summary>
    /// Converts the string representation of a number to its nullable decimal equivalent.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A <see cref="Nullable{Decimal}"/> that represents the converted nullable decimal number if the conversion succeeds; otherwise, <c>null</c>.</returns>
    [Pure]
    public static decimal? ToDecimal(this string? value)
    {
        ReadOnlySpan<char> s = value.AsSpan().Trim();

        if (s.IsEmpty)
            return null;

        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite |
                                    NumberStyles.AllowTrailingWhite;

        return decimal.TryParse(s, styles, CultureEnUsCache.Instance, out decimal v) ? v : null;
    }

    /// <summary>
    /// Converts the specified string to an integer. If the conversion fails, it returns 0.
    /// </summary>
    /// <param name="str">The string to convert to an integer. Can be null.</param>
    /// <summary>
    /// Converts the specified string to an integer. If the conversion fails, it returns 0.
    /// </summary>
    /// <param name="str">The string to convert to an integer. Can be null.</param>
    /// <returns>An integer value if the string can be parsed; otherwise, 0.</returns>
    [Pure]
    public static int ToInt(this string? str)
    {
        ReadOnlySpan<char> s = str.AsSpan().Trim();

        if (s.IsEmpty)
            return 0;

        return int.TryParse(s, NumberStyles.Integer, CultureEnUsCache.Instance, out int v) ? v : 0;
    }
}