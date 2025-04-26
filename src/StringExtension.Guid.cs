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
        return Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Makes sure result is not an empty GUID.
    /// </summary>
    [Pure]
    public static bool IsValidPopulatedGuid(this string? input)
    {
        return Guid.TryParse(input, out Guid result) && result != Guid.Empty;
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
        return input is null || (Guid.TryParse(input, out Guid result) && result != Guid.Empty);
    }

    /// <summary>
    /// Extracts a deterministic integer from the first 4 bytes of a valid GUID string.
    /// </summary>
    /// <param name="guidString">A valid GUID string in "D" format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).</param>
    /// <returns>A non-negative integer derived from the first 4 bytes of the GUID.</returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid GUID in "D" format.</exception>
    [Pure]
    public static int ToIntFromGuid(this string guidString)
    {
        if (!Guid.TryParseExact(guidString, "D", out Guid guid))
            throw new FormatException("Invalid GUID format. Expected a GUID in 'D' format.");

        int extracted = Unsafe.As<Guid, int>(ref guid);

        return extracted & int.MaxValue;
    }
}