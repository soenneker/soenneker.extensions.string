using System.Buffers;
using Soenneker.Utils.PooledStringBuilders;

namespace BuilderSnapshot;

public static partial class StringExtension
{
    public static string NewlinesBuilder(string value)
    {
        int length = value.Length;
        int first = value.AsSpan().IndexOf("\r\n");
        if (first < 0) return value;
        Span<char> initial = stackalloc char[Math.Min(length, 512)];
        using var builder = length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(length);
        ReadOnlySpan<char> remaining = value;
        do
        {
            builder.Append(remaining[..first]);
            remaining = remaining[(first + 1)..];
            first = remaining.IndexOf("\r\n");
        } while (first >= 0);
        builder.Append(remaining);
        return builder.ToString();
    }

    public static string NewlinesBuffer(string value)
    {
        int length = value.Length;
        int first = value.AsSpan().IndexOf("\r\n");
        if (first < 0) return value;
        char[]? rented = null;
        Span<char> destination = length <= 512 ? stackalloc char[length] : (rented = ArrayPool<char>.Shared.Rent(length));
        try
        {
            ReadOnlySpan<char> remaining = value;
            int written = 0;
            do
            {
                remaining[..first].CopyTo(destination[written..]);
                written += first;
                remaining = remaining[(first + 1)..];
                first = remaining.IndexOf("\r\n");
            } while (first >= 0);
            remaining.CopyTo(destination[written..]);
            return new string(destination[..(written + remaining.Length)]);
        }
        finally { if (rented is not null) ArrayPool<char>.Shared.Return(rented); }
    }

    public static string TelBuffer(string phoneNumber, int countryCode = 1)
    {
        phoneNumber.ThrowIfNullOrWhiteSpace();
        int bound = checked(phoneNumber.Length + 16);
        char[]? rented = null;
        Span<char> destination = bound <= 512 ? stackalloc char[bound] : (rented = ArrayPool<char>.Shared.Rent(bound));
        try
        {
            "tel:+".AsSpan().CopyTo(destination);
            countryCode.TryFormat(destination[5..], out int count, provider: System.Globalization.CultureInfo.InvariantCulture);
            int written = 5 + count;
            for (int i = 0; i < phoneNumber.Length; i++)
            {
                char c = phoneNumber[i];
                if ((uint)(c - '0') <= 9 || (c == '+' && i == 0)) destination[written++] = c;
            }
            return new string(destination[..written]);
        }
        finally { if (rented is not null) ArrayPool<char>.Shared.Return(rented); }
    }

    public static string ScribanBuilder(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        ReadOnlySpan<char> s = input;
        if (!char.IsWhiteSpace(s[0]) && !char.IsWhiteSpace(s[^1]) && !s.ContainsAny(_scribanChanges)) return input;
        Span<char> initial = stackalloc char[Math.Min(s.Length, 512)];
        using var builder = s.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(s.Length);
        int pending = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if ((c == '{' || c == '}') && i + 1 < s.Length && s[i + 1] == c) { i++; continue; }
            char mapped = c switch { '"' => '\'', '\\' => '/', '\r' or '\n' => ' ', _ => c };
            if (char.IsWhiteSpace(mapped)) { if (builder.Length != 0) pending++; continue; }
            if (pending != 0) { builder.Append(' ', pending); pending = 0; }
            builder.Append(mapped);
        }
        return builder.AsSpan().SequenceEqual(s) ? input : builder.ToString();
    }

    public static string ScribanBuffer(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        ReadOnlySpan<char> s = input;
        if (!char.IsWhiteSpace(s[0]) && !char.IsWhiteSpace(s[^1]) && !s.ContainsAny(_scribanChanges)) return input;
        char[]? rented = null;
        Span<char> destination = s.Length <= 512 ? stackalloc char[s.Length] : (rented = ArrayPool<char>.Shared.Rent(s.Length));
        try
        {
            int pending = 0, written = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if ((c == '{' || c == '}') && i + 1 < s.Length && s[i + 1] == c) { i++; continue; }
                char mapped = c switch { '"' => '\'', '\\' => '/', '\r' or '\n' => ' ', _ => c };
                if (char.IsWhiteSpace(mapped)) { if (written != 0) pending++; continue; }
                if (pending != 0) { destination.Slice(written, pending).Fill(' '); written += pending; pending = 0; }
                destination[written++] = mapped;
            }
            return destination[..written].SequenceEqual(s) ? input : new string(destination[..written]);
        }
        finally { if (rented is not null) ArrayPool<char>.Shared.Return(rented); }
    }

    public static string PhoneBuilder(string input)
    {
        input.ThrowIfNullOrWhiteSpace();
        bool unchanged = true;
        for (int i = 0; i < input.Length; i++)
            if ((uint)(input[i] - '0') > 9 && (input[i] != '+' || i != 0)) { unchanged = false; break; }
        if (unchanged) return input;
        Span<char> initial = stackalloc char[Math.Min(input.Length, 512)];
        using var builder = input.Length <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(input.Length);
        foreach (char c in input)
            if ((uint)(c - '0') <= 9 || (c == '+' && builder.Length == 0)) builder.Append(c);
        return builder.ToString();
    }

    public static string TelBuilder(string phoneNumber, int countryCode = 1)
    {
        phoneNumber.ThrowIfNullOrWhiteSpace();
        int bound = checked(phoneNumber.Length + 16);
        Span<char> initial = stackalloc char[Math.Min(bound, 512)];
        using var builder = bound <= 512 ? new PooledStringBuilder(initial) : new PooledStringBuilder(bound);
        builder.Append("tel:+");
        builder.Append(countryCode);
        for (int i = 0; i < phoneNumber.Length; i++)
        {
            char c = phoneNumber[i];
            if ((uint)(c - '0') <= 9 || (c == '+' && i == 0)) builder.Append(c);
        }
        return builder.ToString();
    }
}
