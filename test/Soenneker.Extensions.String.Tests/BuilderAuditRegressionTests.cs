using System;
using System.Globalization;
using System.Linq;
using AwesomeAssertions;

namespace Soenneker.Extensions.String.Tests;

public class BuilderAuditRegressionTests
{
    [Test]
    public void Scriban_preserves_brace_pair_and_whitespace_semantics()
    {
        foreach (int length in new[] { 1, 31, 32, 127, 128, 495, 496, 511, 512, 513, 4096 })
        {
            string text = new('x', length);
            ("\t{{{" + text + "}}}\u3000\t\"\\y\r\n").ToEscapedForScriban().Should().Be("{" + text + "}  '/y");
            ("{{}}\t" + new string(' ', length)).ToEscapedForScriban().Should().BeEmpty();
            ReferenceEquals(text.ToEscapedForScriban(), text).Should().BeTrue();
            string singleBraces = "{" + text + "}";
            ReferenceEquals(singleBraces.ToEscapedForScriban(), singleBraces).Should().BeTrue();
        }
    }

    [Test]
    public void Phone_uris_keep_ascii_filtering_leading_plus_and_invariant_country_codes()
    {
        foreach (int length in new[] { 1, 31, 32, 495, 496, 497, 511, 512, 513, 4096 })
        {
            const string token = "+12 (٣४5)- x";
            string text = string.Concat(Enumerable.Repeat(token, (length + token.Length - 1) / token.Length))[..length];
            string filtered = "+" + string.Concat(text.Where(c => c is >= '0' and <= '9'));
            foreach (int country in new[] { 1, -44, 0, int.MinValue, int.MaxValue })
            {
                string tail = "+" + country.ToString(CultureInfo.InvariantCulture) + filtered;
                text.ToTelFormat(country).Should().Be("tel:" + tail);
                text.ToSmsFormat(country).Should().Be("sms:" + tail);
            }
        }
        " (+12)".ToTelFormat().Should().Be("tel:+112");
        "abc".ToSmsFormat(-44).Should().Be("sms:+-44");
    }
}
