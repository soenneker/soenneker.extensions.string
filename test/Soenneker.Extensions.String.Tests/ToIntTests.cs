using AwesomeAssertions;
using Xunit;

namespace Soenneker.Extensions.String.Tests;

public class ToIntTests
{
    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-456", -456)]
    [InlineData("   789   ", 789)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("abc", 0)]
    [InlineData("123abc", 0)]
    [InlineData("9999999999", 0)] // Overflow case
    public void ToInt_ShouldReturnExpectedResult(string? input, int expected)
    {
        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToInt_ShouldHandleMaximumIntValue()
    {
        // Arrange
        var input = int.MaxValue.ToString();

        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(int.MaxValue);
    }

    [Fact]
    public void ToInt_ShouldHandleMinimumIntValue()
    {
        // Arrange
        var input = int.MinValue.ToString();

        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(int.MinValue);
    }

    [Theory]
    [InlineData("2147483648", 0)] // Overflow case
    [InlineData("-2147483649", 0)] // Underflow case
    public void ToInt_ShouldReturnZeroForOverflowOrUnderflow(string? input, int expected)
    {
        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(expected);
    }
}