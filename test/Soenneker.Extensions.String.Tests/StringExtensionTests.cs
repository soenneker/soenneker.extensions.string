using AwesomeAssertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soenneker.Extensions.String.Tests;

public class StringExtensionTests
{
    [Test]
    [Arguments(null, false)]
    [Arguments("", true)]
    [Arguments(" ", false)]
    public void IsEmpty(string? value, bool expected)
    {
        bool result = value.IsEmpty();
        result.Should().Be(expected);
    }

    [Test]
    public void String_date_should_parse()
    {
        const string date = "03/22/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().NotBeNull();

        dateTime!.Value.Day.Should().Be(22);
        dateTime.Value.Month.Should().Be(3);
        dateTime.Value.Year.Should().Be(2019);
    }

    [Test]
    public void String_date_should_parse_without_leading_zero()
    {
        const string date = "3/22/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().NotBeNull();

        dateTime!.Value.Day.Should().Be(22);
        dateTime.Value.Month.Should().Be(3);
        dateTime.Value.Year.Should().Be(2019);
    }

    [Test]
    public void String_date_european_should_not_parse()
    {
        const string date = "22/05/2019";

        var dateTime = date.ToDateTime();
        dateTime.Should().BeNull();
    }

    [Test]
    public void ToDouble_should_parse_correctly()
    {
        const string test = "2.5";

        var result = test.ToDouble();

        result.Should().Be(2.5D);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments("arst", "")]
    [Arguments("arst23k@3 3 test", "2333")]
    public void RemoveNonDigits_tests(string? test, string? expected)
    {
        string? result = test.RemoveNonDigits();

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments("arst23k", "arst23k")]
    [Arguments("arst23k@3 3 test", "arst23k@33test")]
    [Arguments("before\u00A0middle after", "beforemiddleafter")]
    public void RemoveWhiteSpace_should_remove_white_space(string? test, string? expected)
    {
        string? result = test.RemoveWhiteSpace();

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments("Test with space", "Test%20with%20space")]
    [Arguments("Testwith:", "Testwith%3A")]
    public void ToEscaped_should_escape(string? test, string? expected)
    {
        string? result = test.ToEscaped();

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments("www.google.com", "wwwgooglecom")]
    [Arguments("https://forwardslashperiod.com/", "httpsforwardslashperiodcom")]
    [Arguments("allows-dashes", "allows-dashes")]
    [Arguments("Double--dash", "double-dash")]
    [Arguments("underscore_-dash", "underscore-dash")]
    [Arguments("double__underscore", "double_underscore")]
    [Arguments("ending-dash-", "ending-dash")]
    [Arguments("ending-underscore_", "ending-underscore")]
    [Arguments("Uppercase", "uppercase")]
    [Arguments("spaces to dashes", "spaces-to-dashes")]
    [Arguments("&ampersand", "ampersand")]
    [Arguments("#hash", "hash")]
    [Arguments("\\Backslash", "backslash")]
    [Arguments("\"Quote", "quote")]
    [Arguments("`';&~", "")]
    public void Slugify_should_replace(string? test, string? expected)
    {
        string? result = test.Slugify();

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments(" ", " ")]
    [Arguments("/www.google.com", "www.google.com")]
    public void RemoveLeadingChar_should_remove_char(string? test, string? expected)
    {
        string? result = test.RemoveLeadingChar('/');

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments(" ", " ")]
    [Arguments("arst", "arst")]
    [Arguments("/www.google.com", "/www.google.co")]
    public void RemoveTrailingChar_should_remove_char(string? test, string? expected)
    {
        string? result = test.RemoveTrailingChar('m');

        result.Should().Be(expected);
    }

    [Test]
    [Arguments(null, null)]
    [Arguments(" ", " ")]
    [Arguments("blerg", "Blerg")]
    [Arguments("a", "A")]
    public void ToUpperFirstChar_should_capitalize_first_char(string? test, string? expected)
    {
        string? result = test.ToUpperFirstChar();

        result.Should().Be(expected);
    }

    [Test]
    public void ToIds_with_values_should_give_result()
    {
        const string test = "blah:test";
        List<string>? result = test.ToIds();
        result!.First().Should().Be("blah");
        result![1].Should().Be("test");
    }

    [Test]
    public void ToIds_with_no_separator_should_give_result()
    {
        const string test = "blah";
        List<string>? result = test.ToIds();
        result!.First().Should().Be("blah");
    }

    [Test]
    public void ToIds_with_null_should_be_null()
    {
        string? test = null;
        List<string>? result = test.ToIds();
        result.Should().BeNull();
    }

    [Test]
    [Arguments("MTIzNA==", "1234")]
    [Arguments("MTIzNA", "1234")]
    [Arguments("MTIzNA==\r\n", "1234")]
    [Arguments("SGVsbG8td29ybGQ_", "Hello-world?")]
    public void ToStringFromBase64_should_decode_standard_and_url_formats(string input, string expected)
    {
        input.ToStringFromBase64().Should().Be(expected);
    }

    [Test]
    [Arguments(null, false)]
    [Arguments("0", false)]
    [Arguments("1", true)]
    [Arguments("TRUE", true)]
    [Arguments("false", false)]
    [Arguments(" true ", true)]
    [Arguments("invalid", false)]
    public void ToBool_should_preserve_supported_parsing(string? input, bool expected)
    {
        input.ToBool().Should().Be(expected);
    }

    [Test]
    [Arguments(null, false)]
    [Arguments("", false)]
    [Arguments("   ", false)]
    [Arguments("abc123", true)]
    [Arguments("abc-123", false)]
    public void IsAlphaNumeric_should_identify_letters_and_digits(string? input, bool expected)
    {
        input.IsAlphaNumeric().Should().Be(expected);
    }

    [Test]
    public void ToEscapedForScriban_should_reuse_unchanged_input()
    {
        var input = new string("ordinary value".ToCharArray());

        string result = input.ToEscapedForScriban();

        ReferenceEquals(input, result).Should().BeTrue();
    }

    [Test]
    [Arguments("  one, two, , three  ", ',', new[] { "one", "two", "three" })]
    [Arguments("   ", ',', null)]
    public void SplitTrimmedNonEmpty_should_trim_and_omit_empty_values(string input, char delimiter, string[]? expected)
    {
        input.SplitTrimmedNonEmpty(delimiter).Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ToUnixLineBreaks_should_replace_only_crlf_pairs()
    {
        "one\r\ntwo\nthree\rfour".ToUnixLineBreaks().Should().Be("one\ntwo\nthree\rfour");
    }

    [Test]
    [Arguments("text/plain; charset=us-ascii", 20127)]
    [Arguments("text/plain; charset=iso-8859-1", 28591)]
    [Arguments("text/plain; charset=utf-32", 12000)]
    [Arguments("text/plain; charset=utf-16be", 1201)]
    [Arguments("application/json; charset=utf-8", 65001)]
    public void GetEncoding_should_return_cached_common_encodings(string contentType, int expectedCodePage)
    {
        contentType.GetEncoding().CodePage.Should().Be(expectedCodePage);
    }

    [Test]
    public void Truncate_should_return_requested_prefix()
    {
        "abcdefgh".Truncate(4).Should().Be("abcd");
    }

    [Test]
    [Arguments(null, 0)]
    [Arguments(" ", 0)]
    [Arguments("1", 1)]
    public void ToInt_should_produce_expected(string? test, int expected)
    {
        int result = test.ToInt();

        result.Should().Be(expected);
    }

    [Test]
    public void Mask_ShortString_ReturnsAsterisks()
    {
        const string input = "Short";
        string result = input.Mask();

        result.Should().Be("*****");
    }

    [Test]
    public void Mask_LongString_ReturnsMaskedString()
    {
        const string input = "ThisIsALongString";
        string result = input.Mask();

        result.Should().Be("**************ing");
    }

    [Test]
    [Arguments("id", "id", "id")]
    [Arguments("partitionKey:documentId", "partitionKey", "documentId")]
    [Arguments("otherid:partitionkey:documentid", "otherid:partitionkey", "documentid")]
    public void ToSplitId_ReturnsCorrectValues(string input, string expectedPartitionKey, string expectedDocumentId)
    {
        (string partitionKey, string documentId) = input.ToSplitId();

        partitionKey.Should().Be(expectedPartitionKey);
        documentId.Should().Be(expectedDocumentId);
    }

    [Test]
    public void RemoveDashes_should_remove_dashes()
    {
        const string input = "This-Is-ALongString";
        string? result = input.RemoveDashes();

        result.Should().Be("ThisIsALongString");
    }

    [Test]
    [Arguments(null, null)]
    [Arguments("", "")]
    [Arguments("hello world", "hello-world")]
    [Arguments("Hello World", "hello-world")]
    [Arguments("HelloWorld", "helloworld")]
    [Arguments("hello_world", "hello_world")]
    [Arguments("Hello_World", "hello_world")]
    [Arguments("Hello__World", "hello_world")]
    [Arguments("Hello-World_", "hello-world")]
    [Arguments("-Hello-World-", "hello-world")]
    [Arguments("  Hello  World  ", "hello-world")]
    [Arguments("12345", "12345")]
    [Arguments("123-45", "123-45")]
    public void Slugify_ReturnsExpectedResult(string? input, string? expectedResult)
    {
        // Act
        string? result = input.Slugify();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public void Slugify_WithNullInput_ReturnsNull()
    {
        // Act
        string? input = null;

        string? result = input.Slugify();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    [Arguments("test", "Test")]
    [Arguments("TEST", "Test")]
    public void ToLowerAndToUpperFirstChar_should_give_result(string input, string expectedOutput)
    {
        input.ToLowerInvariantFast().ToUpperFirstChar().Should().Be(expectedOutput);
    }

    [Test]
    [Arguments("AÉÖ", "aéö")]
    [Arguments("ABC Δ", "abc δ")]
    public void ToLowerInvariantFast_should_lower_non_ascii_characters(string input, string expected)
    {
        input.ToLowerInvariantFast().Should().Be(expected);
    }

    [Test]
    [Arguments("1234567890", "(123) 456-7890")]
    [Arguments("9876543210", "(987) 654-3210")]
    [Arguments("+19876543210", "(987) 654-3210")]
    [Arguments("19876543210", "(987) 654-3210")]
    public void ToDisplayPhoneNumber_ShouldFormatCorrectly(string input, string expected)
    {
        // Act
        string result = input.ToDisplayPhoneNumber();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToDisplayPhoneNumber_WithInvalidInput_ShouldThrowException()
    {
        // Arrange
        var input = "12345"; // Not enough digits to format

        // Act
        Action act = () => input.ToDisplayPhoneNumber();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [Arguments("example.txt", "txt")]
    [Arguments("archive.tar.gz", "gz")]
    [Arguments("no_extension", "")]
    [Arguments("hiddenfile.", "")]
    [Arguments("UPPERCASE.TXT", "txt")]
    public void ToFileExtension_ShouldReturnExpectedExtension(string input, string expected)
    {
        // Act
        string? result = input.ToFileExtension();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToMemoryStream_should_be_left_open()
    {
        const string testStr = "test";
        var result = testStr.ToMemoryStream();
        result.CanRead.Should().BeTrue();
        result.CanWrite.Should().BeTrue();
        
    }
}
