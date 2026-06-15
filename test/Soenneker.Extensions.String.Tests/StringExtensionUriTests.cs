using System;
using AwesomeAssertions;

namespace Soenneker.Extensions.String.Tests;

public class StringExtensionUriTests
{
    [Test]
    [Arguments("https://example.com/path?x=1", true)]
    [Arguments("http://example.com", true)]
    [Arguments("mailto:test@example.com", true)]
    [Arguments("ftp://files.example.com/resource", true)]
    [Arguments("/relative/path", false)]
    [Arguments("not a uri", false)]
    [Arguments(null, false)]
    public void IsUri_ShouldReturnExpectedResult(string? input, bool expected)
    {
        // Act
        bool result = input.IsUri();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    [Arguments("https://example.com/path?x=1")]
    [Arguments("mailto:test@example.com")]
    [Arguments("ftp://files.example.com/resource")]
    public void ToUri_ShouldParseAbsoluteUris(string input)
    {
        // Act
        Uri? uri = input.ToUri();

        // Assert
        uri.Should().NotBeNull();
        uri!.IsAbsoluteUri.Should().BeTrue();
        uri!.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ToUri_ShouldReturnNullForInvalidUri()
    {
        // Arrange
        var input = "not a uri";

        // Act
        Uri? uri = input.ToUri();

        // Assert
        uri.Should().BeNull();
    }
}
