using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Soenneker.Extensions.String.Tests;

public class StringExtensionTests
{
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
    [InlineData(null, null)]
    [InlineData("arst", "")]
    [InlineData("arst23k@3 3 test", "2333")]
    public void RemoveNonDigits_tests(string? test, string? expected)
    {
        string? result = test.RemoveNonDigits();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("arst23k", "arst23k")]
    [InlineData("arst23k@3 3 test", "arst23k@33test")]
    public void RemoveWhiteSpace_should_remove_white_space(string? test, string? expected)
    {
        string? result = test.RemoveWhiteSpace();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Test with space", "Test%20with%20space")]
    [InlineData("Testwith:", "Testwith%3A")]
    public void ToEscaped_should_escape(string? test, string? expected)
    {
        string? result = test.ToEscaped();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("www.google.com", "wwwgooglecom")]
    [InlineData("https://forwardslashperiod.com/", "httpsforwardslashperiodcom")]
    [InlineData("allows-dashes", "allows-dashes")]
    [InlineData("Double--dash", "double-dash")]
    [InlineData("underscore_-dash", "underscore-dash")]
    [InlineData("double__underscore", "double_underscore")]
    [InlineData("ending-dash-", "ending-dash")]
    [InlineData("ending-underscore_", "ending-underscore")]
    [InlineData("Uppercase", "uppercase")]
    [InlineData("spaces to dashes", "spaces-to-dashes")]
    [InlineData("&ampersand", "ampersand")]
    [InlineData("#hash", "hash")]
    [InlineData("\\Backslash", "backslash")]
    [InlineData("\"Quote", "quote")]
    [InlineData("`';&~", "")]
    public void Slugify_should_replace(string? test, string? expected)
    {
        string? result = test.Slugify();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", " ")]
    [InlineData("/www.google.com", "www.google.com")]
    public void RemoveLeadingChar_should_remove_char(string? test, string? expected)
    {
        string? result = test.RemoveLeadingChar('/');

        result.Should().Be(expected);
    }


    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", " ")]
    [InlineData("arst", "arst")]
    [InlineData("/www.google.com", "/www.google.co")]
    public void RemoveTrailingChar_should_remove_char(string? test, string? expected)
    {
        string? result = test.RemoveTrailingChar('m');

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", " ")]
    [InlineData("blerg", "Blerg")]
    [InlineData("a", "A")]
    public void ToUpperFirstChar_should_capitalize_first_char(string? test, string? expected)
    {
        string? result = test.ToUpperFirstChar();

        result.Should().Be(expected);
    }

    [Fact]
    public void ToIds_with_values_should_give_result()
    {
        const string test = "blah:test";
        List<string>? result = test.ToIds();
        result!.First().Should().Be("blah");
        result![1].Should().Be("test");
    }

    [Fact]
    public void ToIds_with_no_separator_should_give_result()
    {
        const string test = "blah";
        List<string>? result = test.ToIds();
        result!.First().Should().Be("blah");
    }

    [Fact]
    public void ToIds_with_null_should_be_null()
    {
        string? test = null;
        List<string>? result = test.ToIds();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(" ", 0)]
    [InlineData("1", 1)]
    public void ToInt_should_produce_expected(string? test, int expected)
    {
        int result = test.ToInt();

        result.Should().Be(expected);
    }

    [Fact]
    public void Mask_ShortString_ReturnsAsterisks()
    {
        const string input = "Short";
        string result = input.Mask();

        result.Should().Be("*****");
    }

    [Fact]
    public void Mask_LongString_ReturnsMaskedString()
    {
        const string input = "ThisIsALongString";
        string result = input.Mask();

        result.Should().Be("**************ing");
    }

    [Theory]
    [InlineData("id", "id", "id")]
    [InlineData("partitionKey:documentId", "partitionKey", "documentId")]
    [InlineData("otherid:partitionkey:documentid", "otherid:partitionkey", "documentid")]
    public void ToSplitId_ReturnsCorrectValues(string input, string expectedPartitionKey, string expectedDocumentId)
    {
        (string partitionKey, string documentId) = input.ToSplitId();

        partitionKey.Should().Be(expectedPartitionKey);
        documentId.Should().Be(expectedDocumentId);
    }

    [Fact]
    public void RemoveDashes_should_remove_dashes()
    {
        const string input = "This-Is-ALongString";
        string? result = input.RemoveDashes();

        result.Should().Be("ThisIsALongString");
    }
}