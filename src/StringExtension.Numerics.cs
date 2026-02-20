using Soenneker.Culture.English.US;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    private const NumberStyles _floatStyles = NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
    private const NumberStyles _integerStyles = NumberStyles.Integer | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

    /// <summary>
    /// Determines whether a string contains only numeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is numeric, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumeric(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        ReadOnlySpan<char> s = value; // implicit conversion; null becomes default (handled above)

        for (var i = 0; i < s.Length; i++)
        {
            // Branchless ASCII digit check
            if ((uint)(s[i] - '0') > 9u)
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? ToDouble(this string? value)
    {
        // Let TryParse handle whitespace to avoid a separate Trim() pass
        return double.TryParse(value, _floatStyles, CultureEnUsCache.Instance, out double result) ? result : null;
    }

    /// <summary>
    /// Converts the string representation of a number to its nullable decimal equivalent.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A <see cref="Nullable{Decimal}"/> that represents the converted nullable decimal number if the conversion succeeds; otherwise, <c>null</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal? ToDecimal(this string? value)
    {
        // Same rationale as ToDouble; NumberStyles.Float works for decimal too
        return decimal.TryParse(value, _floatStyles, CultureEnUsCache.Instance, out decimal result) ? result : null;
    }

    /// <summary>
    /// Converts the specified string to an integer. If the conversion fails, it returns 0.
    /// </summary>
    /// <param name="str">The string to convert to an integer. Can be null.</param>
    /// <summary>
    /// Converts the specified string to an integer. If the conversion fails, it returns 0.
    /// </summary>
    /// <returns>An integer value if the string can be parsed; otherwise, 0.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this string? str)
    {
        return int.TryParse(str, _integerStyles, CultureEnUsCache.Instance, out int v) ? v : 0;
    }
}