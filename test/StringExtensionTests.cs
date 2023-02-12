using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Extensions.String.Tests;

public class StringExtensionTests
{
    public StringExtensionTests(ITestOutputHelper output)
    {
    }

    [Fact]
    public void String_date_should_parse()
    {
        const string date = "03/22/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().NotBeNull();

        dateTime!.Value.Day.Should().Be(22);
        dateTime.Value.Month.Should().Be(3);
        dateTime.Value.Year.Should().Be(2019);
    }

    [Fact]
    public void String_date_should_parse_without_leading_zero()
    {
        const string date = "3/22/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().NotBeNull();

        dateTime!.Value.Day.Should().Be(22);
        dateTime.Value.Month.Should().Be(3);
        dateTime.Value.Year.Should().Be(2019);
    }

    [Fact]
    public void String_date_european_should_not_parse()
    {
        const string date = "22/05/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().BeNull();
    }

    [Fact]
    public void ToDouble_should_parse_correctly()
    {
        const string test = "2.5";

        var result = test.ToDouble();

        result.Should().Be(2.5D);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("arst", "")]
    [InlineData("arst23k@3 3 test", "2333")]
    public void RemoveNonDigits_tests(string? test, string expected)
    {
        string? result = test.RemoveNonDigits();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("arst23k", "arst23k")]
    [InlineData("arst23k@3 3 test", "arst23k@33test")]
    public void RemoveWhiteSpace_should_remove_white_space(string? test, string expected)
    {
        string? result = test.RemoveWhiteSpace();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Test with space", "Test%20with%20space")]
    [InlineData("Testwith:", "Testwith%3A")]
    public void ToEscaped_should_escape(string? test, string expected)
    {
        string? result = test.ToEscaped();

        result.Should().Be(expected);
    }
}