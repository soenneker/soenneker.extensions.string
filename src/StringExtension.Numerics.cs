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
        if (value.IsNullOrWhiteSpace())
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (!value[i].IsDigit())
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
    public static double? ToDouble(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return null;

        bool successful = double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureEnUsCache.CultureInfo, out double result);

        if (successful)
            return result;

        return null;
    }

    /// <summary>
    /// Converts the string representation of a number to its nullable decimal equivalent.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A <see cref="Nullable{Decimal}"/> that represents the converted nullable decimal number if the conversion succeeds; otherwise, <c>null</c>.</returns>
    [Pure]
    public static decimal? ToDecimal(this string value)
    {
        if (value.IsNullOrWhiteSpace())
            return null;

        bool successful = decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureEnUsCache.CultureInfo, out decimal result);

        if (successful)
            return result;

        return null;
    }


    /// <summary>
    /// Converts the specified string to an integer. If the conversion fails, it returns 0.
    /// </summary>
    /// <param name="str">The string to convert to an integer. Can be null.</param>
    /// <returns>An integer value if the string can be parsed; otherwise, 0.</returns>
    [Pure]
    public static int ToInt(this string? str)
    {
        // Return 0 on null or empty
        if (str.IsNullOrEmpty())
            return 0;

        ReadOnlySpan<char> span = str.AsSpan();

        // 1) Skip leading whitespace
        var pos = 0;
        while (pos < span.Length && span[pos].IsWhiteSpaceFast())
            pos++;

        // If we've reached end, no digits => return 0
        if (pos >= span.Length)
            return 0;

        // 2) Check for sign
        var negative = false;
        char c = span[pos];
        if (c == '-')
        {
            negative = true;
            pos++;
        }
        else if (c == '+')
        {
            pos++;
        }

        long result = 0;
        int startDigitPos = pos;

        // 3) Parse digits
        for (; pos < span.Length; pos++)
        {
            c = span[pos];
            // Fast check for ASCII digit
            if ((uint) (c - '0') <= 9)
            {
                int digit = c - '0';

                // Quick overflow check vs. int range
                // If already beyond int.MaxValue, return 0
                if (result > int.MaxValue)
                    return 0;

                result = result * 10 + digit;
            }
            else
            {
                // Possibly trailing whitespace -> skip it
                if (c.IsWhiteSpaceFast())
                {
                    pos++;
                    // Skip all remaining whitespace
                    while (pos < span.Length && span[pos].IsWhiteSpaceFast())
                        pos++;
                    // If there's anything else after trailing whitespace => invalid
                    return pos >= span.Length ? FinalizeResult(result, negative) : 0;
                }

                // Non‐digit and non‐whitespace => invalid
                return 0;
            }
        }

        // If no digits were parsed => return 0
        if (pos == startDigitPos)
            return 0;

        return FinalizeResult(result, negative);
    }

    // Applies sign and checks final overflow (still no exceptions)
    private static int FinalizeResult(long value, bool negative)
    {
        // If negative => valid range is up to 2147483648 for absolute value
        if (negative)
        {
            // If above 2147483648 => out of range for int.MinValue
            if (value > 2147483648)
                return 0;
            return (int) -value; // e.g. 2147483648 => -2147483648
        }
        else
        {
            // If above 2147483647 => out of range
            if (value > 2147483647)
                return 0;
            return (int) value;
        }
    }
}