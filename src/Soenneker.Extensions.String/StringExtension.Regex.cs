using System;
using System.Text.RegularExpressions;
using Soenneker.Utils.PooledStringBuilders;

namespace Soenneker.Extensions.String;

public static partial class StringExtension
{
    /// <summary>
    /// Replaces regular expression matches using a callback that receives the matched text as a span.
    /// </summary>
    /// <param name="value">The string to transform.</param>
    /// <param name="regex">The regular expression to use, including its options and match timeout.</param>
    /// <param name="evaluator">Called once per match, in the regular expression's search order.
    /// Its result is inserted literally; returning <see langword="null"/> or an empty string removes the match.</param>
    /// <returns>The transformed string, or the original string if there are no matches.</returns>
    /// <remarks>
    /// The callback receives only the complete matched text, without a separate substring or
    /// <see cref="Match"/> object for each match. Use <see cref="Regex.Replace(string, MatchEvaluator)"/> when capture groups
    /// are needed. Empty matches and right-to-left expressions are supported. Replacement text is not searched again.
    /// Output is assembled in stack storage that grows into pooled storage as needed; the callback is never
    /// repeated to calculate the output length. Any strings created by the callback contribute additional allocations.
    /// </remarks>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <exception cref="RegexMatchTimeoutException">The regular expression's match timeout is exceeded.</exception>
    public static string ReplaceMatches(this string value, Regex regex, Func<ReadOnlySpan<char>, string?> evaluator)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(evaluator);

        Regex.ValueMatchEnumerator matches = regex.EnumerateMatches(value);
        if (!matches.MoveNext())
            return value;

        Span<char> initial = stackalloc char[256];
        using var builder = new PooledStringBuilder(initial);

        if (regex.RightToLeft)
        {
            int end = value.Length;
            do
            {
                ValueMatch match = matches.Current;
                int matchEnd = match.Index + match.Length;
                CopyMatchTextReversed(value.AsSpan(matchEnd, end - matchEnd), builder.AppendSpan(end - matchEnd));
                string? replacement = evaluator(value.AsSpan(match.Index, match.Length));
                CopyMatchTextReversed(replacement.AsSpan(), builder.AppendSpan(replacement?.Length ?? 0));
                end = match.Index;
            } while (matches.MoveNext());

            CopyMatchTextReversed(value.AsSpan(0, end), builder.AppendSpan(end));
            return string.Create(builder.Length, builder.AsSpan(), static (destination, source) =>
            {
                source.CopyTo(destination);
                destination.Reverse();
            });
        }

        int position = 0;
        do
        {
            ValueMatch match = matches.Current;
            builder.Append(value.AsSpan(position, match.Index - position));
            builder.Append(evaluator(value.AsSpan(match.Index, match.Length)));
            position = match.Index + match.Length;
        } while (matches.MoveNext());

        builder.Append(value.AsSpan(position));
        return builder.ToString();
    }

    private static void CopyMatchTextReversed(ReadOnlySpan<char> text, Span<char> destination)
    {
        text.CopyTo(destination);
        destination.Reverse();
    }
}
