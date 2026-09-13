using System;
using System.Linq;
using AwesomeAssertions;

namespace Soenneker.Extensions.String.Tests;

public class PerformanceRegressionTests
{
    [Test]
    public void Whitespace_search_matches_every_utf16_character()
    {
        for (int i = 0; i <= char.MaxValue; i++)
        {
            char c = (char)i;
            string value = "a" + c + "b";
            value.RemoveWhiteSpace().Should().Be(char.IsWhiteSpace(c) ? "ab" : value);
        }
    }

    [Test]
    public void Ordinal_casing_preserves_non_ascii_at_vector_boundaries()
    {
        foreach (int length in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 63, 64, 65, 511, 512, 513, 4096 })
        {
            string input = string.Concat(Enumerable.Range(0, length).Select(i => "aZ09éÉ\ud800\udc00"[i % 8]));
            string lower = string.Concat(input.Select(c => c is >= 'A' and <= 'Z' ? (char)(c + 32) : c));
            string upper = string.Concat(input.Select(c => c is >= 'a' and <= 'z' ? (char)(c - 32) : c));
            input.ToLowerOrdinal().Should().Be(lower);
            input.ToUpperOrdinal().Should().Be(upper);
        }
    }

    [Test]
    public void Unchanged_slugs_and_unsplit_items_reuse_input()
    {
        foreach (string input in new[] { "hello-world", "hello_world", "école", new string('a', 4096) })
        {
            ReferenceEquals(input.Slugify(), input).Should().BeTrue();
            ReferenceEquals(input.SplitTrimmedNonEmpty(',')![0], input).Should().BeTrue();
            ReferenceEquals(input.FromCommaSeparatedToList()[0], input).Should().BeTrue();
            ReferenceEquals(input.ToIds()![0], input).Should().BeTrue();
        }
    }

    [Test]
    public void Secure_shuffle_preserves_characters_and_single_character_identity()
    {
        foreach (string input in new[] { "", "a", "aabbcc", new string('x', 1024) + "0123456789" })
            input.SecureShuffle().Order().Should().Equal(input.Order());
        const string single = "a";
        ReferenceEquals(single.SecureShuffle(), single).Should().BeTrue();
    }

    [Test]
    public void Slug_transformation_handles_long_ascii_runs_and_boundaries()
    {
        foreach (int length in new[] { 15, 16, 17, 31, 32, 33, 127, 128, 129, 511, 512, 513, 4096 })
        {
            string plain = new string('a', length);
            (" Hello___WORLD!! " + plain).Slugify().Should().Be("hello_world-" + plain);
            (plain + "!").Slugify().Should().Be(plain);
            ("_" + plain + "__").Slugify().Should().Be(plain);
            new string('A', length).Slugify().Should().Be(plain);
            (plain + " é " + plain).Slugify().Should().Be(plain + "-é-" + plain);
        }
    }

    [Test]
    [Arguments("-+", "\ufffd")]
    [Arguments("SGVsbG8", "Hello")]
    [Arguments(" SGVsbG8=\r\n", "Hello")]
    public void Base64_retains_mixed_alphabet_and_whitespace_support(string input, string expected)
    {
        input.ToStringFromBase64().Should().Be(expected);
    }
}
