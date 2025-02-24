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
    /// Converts a GUID string into a non-negative integer in a reproducible manner.
    /// </summary>
    /// <param name="guidString">The GUID string in "D" format (e.g., "3F2504E0-4F89-41D3-9A0C-0305E82C3301").</param>
    /// <returns>
    /// A non-negative integer representation of the first four bytes of the GUID.
    /// The same GUID will always produce the same integer.
    /// </returns>
    /// <exception cref="FormatException">Thrown if the input is not a valid GUID in "D" format.</exception>
    /// <remarks>
    /// This method extracts the first four bytes of the GUID and converts them to an integer,
    /// ensuring a consistent and deterministic output. The result is always non-negative
    /// to prevent unexpected behavior with negative numbers. <para/>
    /// Keep in mind that there can be collisions with different GUIDs because GUIDs are 128bits and integers are 32bits.
    /// </remarks>
    /// <example>
    /// <code>
    /// string guid = "3F2504E0-4F89-41D3-9A0C-0305E82C3301";
    /// int result = guid.ToIntFromGuid();
    /// Console.WriteLine(result); // Output: 1056753311 (example value)
    /// </code>
    /// </example>
    [Pure]
    public static int ToIntFromGuid(this string guidString)
    {
        // Parse without validation if you trust the input
        Guid guid = Guid.ParseExact(guidString, "D");

        // Work with Span<byte> for performance
        Span<byte> bytes = stackalloc byte[16];
        MemoryMarshal.TryWrite(bytes, in guid); // Efficiently writes GUID to bytes

        // Read first 4 bytes as an int (ensuring consistency)
        var result = Unsafe.ReadUnaligned<int>(ref bytes[0]);

        return result & int.MaxValue; // Ensure non-negative value
    }
}