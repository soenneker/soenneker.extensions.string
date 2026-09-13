using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;

namespace Soenneker.Extensions.String.Tests;

public class ReplaceMatchesTests
{
    [Test]
    public void No_matches_reuses_input_and_does_not_call_evaluator()
    {
        string input = new('a', 4096);
        string result = input.ReplaceMatches(new Regex(@"\d+"), _ => throw new InvalidOperationException());
        ReferenceEquals(result, input).Should().BeTrue();
    }

    [Test]
    public void Replacements_are_literal_and_not_reprocessed()
    {
        "a1b22c".ReplaceMatches(new Regex(@"\d+"), static match => match.Length == 1 ? "$&123" : null)
            .Should().Be("a$&123bc");
    }

    [Test]
    public void Empty_matches_are_processed_including_empty_input()
    {
        foreach (RegexOptions options in new[] { RegexOptions.None, RegexOptions.RightToLeft })
        {
            var regex = new Regex("", options);
            "ab".ReplaceMatches(regex, static _ => "-").Should().Be("-a-b-");
            "".ReplaceMatches(regex, static _ => "x").Should().Be("x");
            "".ReplaceMatches(regex, static _ => null).Should().BeEmpty();
        }
    }

    [Test]
    public void Evaluator_runs_once_per_match_in_regex_order()
    {
        foreach (RegexOptions options in new[] { RegexOptions.None, RegexOptions.RightToLeft })
        {
            var regex = new Regex(@"\d+", options);
            var expectedOrder = new List<string>();
            string expected = regex.Replace("a1b22c333", match => { expectedOrder.Add(match.Value); return expectedOrder.Count.ToString(); });
            var actualOrder = new List<string>();
            string actual = "a1b22c333".ReplaceMatches(regex, match => { actualOrder.Add(match.ToString()); return actualOrder.Count.ToString(); });
            actual.Should().Be(expected);
            actualOrder.Should().Equal(expectedOrder);
        }
    }

    [Test]
    public void Matches_runtime_across_growth_unicode_and_search_options()
    {
        string[] patterns = [@"\d+", @"\w+", ".", "", @"(?=a)", @"a*", @"(a)(b)?", @"^|$"];
        foreach (RegexOptions options in new[] { RegexOptions.None, RegexOptions.RightToLeft, RegexOptions.Compiled, RegexOptions.NonBacktracking })
        foreach (string pattern in patterns)
        {
            if (options == RegexOptions.NonBacktracking && pattern == @"(?=a)") continue;
            var regex = new Regex(pattern, options);
            foreach (int length in new[] { 0, 1, 15, 255, 256, 257, 1024, 4096 })
            {
                const string token = "ab12é😀\ud800\t";
                string input = string.Concat(Enumerable.Repeat(token, (length + token.Length - 1) / token.Length))[..length];
                foreach (string? replacement in new[] { null, "", "$1", "é😀", new string('x', 300) })
                    input.ReplaceMatches(regex, _ => replacement).Should().Be(regex.Replace(input, _ => replacement!));
            }
        }
    }

    [Test]
    public void Callback_exception_propagates_after_growth_and_later_calls_still_work()
    {
        var expected = new InvalidOperationException("callback failed");
        var regex = new Regex("x");
        int calls = 0;
        try
        {
            "xx".ReplaceMatches(regex, _ => ++calls == 1 ? new string('a', 4096) : throw expected);
            throw new Exception("Expected callback failure");
        }
        catch (InvalidOperationException actual)
        {
            ReferenceEquals(actual, expected).Should().BeTrue();
        }
        calls.Should().Be(2);
        "x".ReplaceMatches(regex, static _ => "ok").Should().Be("ok");
    }

    [Test]
    public void Null_arguments_are_rejected_even_without_matches()
    {
        AssertNull(() => StringExtension.ReplaceMatches(null!, new Regex("x"), static _ => ""), "value");
        AssertNull(() => "".ReplaceMatches(null!, static _ => ""), "regex");
        AssertNull(() => "".ReplaceMatches(new Regex("x"), null!), "evaluator");
    }

    private static void AssertNull(Action action, string parameter)
    {
        try { action(); throw new Exception("Expected ArgumentNullException"); }
        catch (ArgumentNullException exception) { exception.ParamName.Should().Be(parameter); }
    }
}
