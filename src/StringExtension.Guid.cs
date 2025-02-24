using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Does not check for empty GUID, <see cref="IsValidPopulatedGuid"/> for this.
    /// </summary>
    [Pure]
    public static bool IsValidGuid(this string? input)
    {
        return input is not null && Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Makes sure result is not an empty GUID.
    /// </summary>
    [Pure]
    public static bool IsValidPopulatedGuid(this string? input)
    {
        if (input is null)
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
        return input is null || Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Makes sure result is not an empty GUID.
    /// </summary>
    [Pure]
    public static bool IsValidPopulatedNullableGuid(this string? input)
    {
        if (input is null)
            return true;

        bool success = Guid.TryParse(input, out Guid result);

        if (success && result != Guid.Empty)
            return true;

        return false;
    }

    /// <summary>
    /// Extracts an integer from any valid GUID string.
    /// </summary>
    /// <param name="guidString">A valid GUID string in "D" format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).</param>
    /// <returns>A deterministic integer derived from the first four bytes of the GUID.</returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid GUID.</exception>
    [Pure]
    public static int ToIntFromGuid(this string guidString)
    {
        // Validate and parse the GUID
        if (!Guid.TryParseExact(guidString, "D", out Guid guid))
        {
            throw new FormatException("Invalid GUID format. Expected a GUID in 'D' format.");
        }

        // Read the first 4 bytes of the GUID as an integer
        Span<byte> bytes = stackalloc byte[16];
        MemoryMarshal.TryWrite(bytes, in guid);

        int extractedValue = Unsafe.ReadUnaligned<int>(ref bytes[0]);

        // Ensure the integer is always non-negative
        return extractedValue & int.MaxValue;
    }
}