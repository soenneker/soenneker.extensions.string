using System;
using FluentAssertions;
using Xunit;

namespace Soenneker.Extensions.String.Tests;

public class StringExtensionGuidTests
{
    [Fact]
    public void ToIntFromGuid_ValidGuid_ReturnsConsistentInteger()
    {
        // Arrange
        string guidString = "3F2504E0-4F89-41D3-9A0C-0305E82C3301";

        // Act
        int result1 = guidString.ToIntFromGuid();
        int result2 = guidString.ToIntFromGuid();

        result1.Should().Be(1059390688);

        // Assert
        result1.Should().Be(result2, because: "the same GUID should always map to the same integer");
        result1.Should().BeGreaterThanOrEqualTo(0, because: "the integer should be non-negative");
    }

    [Fact]
    public void ToIntFromGuid_DifferentGuids_ReturnDifferentIntegers()
    {
        // Arrange
        string guid1 = "3F2504E0-4F89-41D3-9A0C-0305E82C3301";
        string guid2 = "5F2504E0-4F89-41D3-9A0C-0305E82C3302";

        // Act
        int result1 = guid1.ToIntFromGuid();
        int result2 = guid2.ToIntFromGuid();

        // Assert
        result1.Should().NotBe(result2, because: "different GUIDs should produce different integers");
    }

    [Fact]
    public void ToIntFromGuid_InvalidGuid_ThrowsFormatException()
    {
        // Arrange
        string invalidGuid = "INVALID-GUID";

        // Act
        Action act = () => invalidGuid.ToIntFromGuid();

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ToIntFromGuid_EmptyGuid_ThrowsFormatException()
    {
        // Arrange
        string emptyGuid = "";

        // Act
        Action act = () => emptyGuid.ToIntFromGuid();

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ToIntFromGuid_ValidGuidWithDifferentFormats_ThrowsFormatException()
    {
        // Arrange
        string guidWithBraces = "{3F2504E0-4F89-41D3-9A0C-0305E82C3301}";

        // Act
        Action act = () => guidWithBraces.ToIntFromGuid();

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ToIntFromGuid_PerformanceTest()
    {
        // Arrange
        string guidString = "3F2504E0-4F89-41D3-9A0C-0305E82C3301";

        // Act
        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 100_000; i++)
        {
            _ = guidString.ToIntFromGuid();
        }

        watch.Stop();

        // Assert (Adjust threshold if necessary)
        watch.ElapsedMilliseconds.Should().BeLessThan(50, because: "the conversion should be fast even for 100,000 executions");
    }
}