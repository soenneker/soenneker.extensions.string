using Soenneker.Extensions.Char;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Converts the first character of the string to lowercase if the string is not null or white-space.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string with the first character converted to lowercase, or the original string if it is null or white-space.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToLowerFirstChar(this string? value)
    {
        int length = value?.Length ?? 0;

        if (length == 0)
            return value;

        if (length == 1)
        {
            char c = value[0];
            char lowered = c.ToLowerInvariant();

            if (c == lowered)
                return value; // No change, return original string

            return lowered.ToString();
        }

        // Check if first char is already lowercase
        if (!value[0].IsUpperFast())
            return value;

        Span<char> buffer = stackalloc char[length];
        value.AsSpan().CopyTo(buffer);
        buffer[0] = buffer[0].ToLowerInvariant();

        return new string(buffer);
    }

    /// <summary>
    /// Converts the first character of the string to uppercase if the string is not null or white-space.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string with the first character converted to uppercase, or the original string if it is null or white-space.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToUpperFirstChar(this string? value)
    {
        int length = value?.Length ?? 0;

        if (length == 0)
            return value;

        if (length == 1)
        {
            char c = value[0];
            char uppered = c.ToUpperInvariant();

            if (c == uppered)
                return value; // No change, return original string

            return uppered.ToString();
        }

        // Check if first char is already uppercase
        if (!value[0].IsLowerFast())
            return value;

        Span<char> buffer = stackalloc char[length];
        value.AsSpan().CopyTo(buffer);
        buffer[0] = buffer[0].ToUpperInvariant();

        return new string(buffer);
    }

    /// <summary>
    /// Converts each character in the current <see cref="string"/> to its lowercase invariant equivalent.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A new string in which each character has been converted to its lowercase invariant equivalent.</returns>
    /// <remarks>This method is similar to <see cref="string.ToLowerInvariant"/> but operates on each character individually.</remarks>
    [Pure]
    public static string ToLowerInvariantFast(this string str)
    {
        int length = str.Length;

        if (length == 0)
            return "";

        return string.Create(length, str, static (span, source) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                char c = source[i];

                // Fast ASCII A–Z check
                if ((uint)(c - 'A') <= ('Z' - 'A'))
                {
                    span[i] = (char)(c + 32); // ASCII uppercase to lowercase
                }
                else
                {
                    // Unicode fallback - lowercase properly
                    span[i] = c.IsUpperFast() ? c.ToLowerInvariant() : c;
                }
            }
        });
    }

    /// <summary>
    /// Converts each character in the current <see cref="string"/> to its uppercase invariant equivalent.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A new string in which each character has been converted to its uppercase invariant equivalent.</returns>
    /// <remarks>This method is similar to <see cref="string.ToUpperInvariant"/> but operates on each character individually.</remarks>
    [Pure]
    public static string ToUpperInvariantFast(this string str)
    {
        int length = str.Length;

        if (length == 0)
            return "";

        return string.Create(length, str, static (span, source) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                char c = source[i];

                // Fast ASCII a–z check
                if ((uint)(c - 'a') <= ('z' - 'a'))
                {
                    span[i] = (char)(c - 32); // ASCII lowercase to uppercase
                }
                else
                {
                    // Unicode fallback - uppercase properly if needed
                    span[i] = c.IsLowerFast() ? c.ToUpperInvariant() : c;
                }
            }
        });
    }

    /// <summary>
    /// Converts all uppercase ASCII letters ('A'-'Z') in the specified string to lowercase ('a'-'z') 
    /// using ordinal casing rules. Non-ASCII characters are left unchanged.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>
    /// A new string where all ASCII uppercase letters have been converted to lowercase.
    /// If the input string is empty, an empty string is returned.
    /// </returns>
    /// <remarks>
    /// This method is optimized for performance and only affects ASCII characters. 
    /// It does not perform culture-aware casing like <see cref="string.ToLower()"/>.
    /// </remarks>
    [Pure]
    public static string ToLowerOrdinal(this string str)
    {
        int length = str.Length;

        if (length == 0)
            return "";

        return string.Create(length, str, static (span, s) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                char c = s[i];

                // Fast path for ASCII A–Z:
                if ((uint)(c - 'A') <= ('Z' - 'A'))
                {
                    span[i] = (char)(c + 32); // Convert ASCII uppercase to lowercase
                }
                else
                {
                    span[i] = c; // Leave non-ASCII characters unchanged
                }
            }
        });
    }

    /// <summary>
    /// Converts all lowercase ASCII letters ('a'-'z') in the specified string to uppercase ('A'-'Z') 
    /// using ordinal casing rules. Non-ASCII characters are left unchanged.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>
    /// A new string where all ASCII lowercase letters have been converted to uppercase.
    /// If the input string is empty, an empty string is returned.
    /// </returns>
    /// <remarks>
    /// This method is optimized for performance and only affects ASCII characters. 
    /// It does not perform culture-aware casing like <see cref="string.ToUpper()"/>.
    /// </remarks>
    [Pure]
    public static string ToUpperOrdinal(this string str)
    {
        int length = str.Length;

        if (length == 0)
            return "";

        return string.Create(length, str, static (span, s) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                char c = s[i];

                // Fast path for ASCII a–z:
                if ((uint)(c - 'a') <= ('z' - 'a'))
                {
                    span[i] = (char)(c - 32); // Convert ASCII lowercase to uppercase
                }
                else
                {
                    span[i] = c; // Leave non-ASCII characters unchanged
                }
            }
        });
    }

    /// <summary>
    /// Converts the specified string to title case (each word capitalized), using spaces to determine word boundaries.
    /// </summary>
    /// <param name="str">The string to convert to title case.</param>
    /// <returns>A string converted to title case where each word is capitalized.</returns>
    /// <remarks>
    /// This method uses the current culture's <see cref="TextInfo"/> to perform the conversion.
    /// If the input string is null or empty, it returns the original string.
    /// </remarks>
    [Pure]
    public static string ToTitleCaseViaSpaces(this string str)
    {
        int length = str?.Length ?? 0;

        if (length == 0)
            return str;

        if (length <= _stackallocThreshold)
        {
            Span<char> buffer = stackalloc char[length];
            ProcessToTitleCase(str, buffer);
            return new string(buffer);
        }
        else
        {
            ArrayPool<char> pool = ArrayPool<char>.Shared;
            char[] rentedBuffer = pool.Rent(length);

            Span<char> buffer = rentedBuffer.AsSpan(0, length);
            ProcessToTitleCase(str, buffer);

            var result = new string(buffer);
            pool.Return(rentedBuffer);

            return result;
        }
    }

    private static void ProcessToTitleCase(string input, Span<char> buffer)
    {
        var newWord = true;

        for (var i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c.IsWhiteSpaceFast())
            {
                newWord = true;
                buffer[i] = c;
            }
            else if (newWord)
            {
                buffer[i] = c.ToUpperInvariant();
                newWord = false;
            }
            else
            {
                buffer[i] = c.ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// Converts the input <see cref="string"/> from PascalCase to snake_case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A new string in snake_case format.</returns>
    /// <remarks>
    /// This method converts a PascalCase string to snake_case format.
    /// For example, "PascalCaseString" will be converted to "pascal_case_string".
    /// </remarks>
    [Pure]
    public static string ToSnakeCaseFromPascal(this string input)
    {
        int length = input?.Length ?? 0;

        if (length == 0)
            return input;

        // Over-allocate for worst-case scenario: input.Length * 2
        int maxBufferSize = length * 2;

        if (maxBufferSize <= _stackallocThreshold)
        {
            // Use stackalloc for small strings
            Span<char> buffer = stackalloc char[maxBufferSize];
            int outputIndex = ProcessSnakeCase(input, buffer);
            return new string(buffer.Slice(0, outputIndex));
        }
        else
        {
            // Use ArrayPool for large strings
            ArrayPool<char> pool = ArrayPool<char>.Shared;
            char[] rentedBuffer = pool.Rent(maxBufferSize);
            Span<char> buffer = rentedBuffer.AsSpan(0, maxBufferSize);

            int outputIndex = ProcessSnakeCase(input, buffer);
            var result = new string(buffer.Slice(0, outputIndex));

            // Explicitly return the buffer to the pool
            pool.Return(rentedBuffer);

            return result;
        }
    }

    private static int ProcessSnakeCase(string input, Span<char> buffer)
    {
        var outputIndex = 0;

        for (var i = 0; i < input.Length; i++)
        {
            char currentChar = input[i];

            // If it's an uppercase letter and not at the beginning, prepend an underscore.
            if (currentChar.IsUpperFast() && i > 0)
            {
                buffer[outputIndex++] = '_';
            }

            buffer[outputIndex++] = currentChar.ToLowerInvariant();
        }

        return outputIndex;
    }
}