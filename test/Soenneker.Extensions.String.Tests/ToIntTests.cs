using AwesomeAssertions;

namespace Soenneker.Extensions.String.Tests;

public class ToIntTests
{
    [Test]
    [Arguments("123", 123)]
    [Arguments("0", 0)]
    [Arguments("-456", -456)]
    [Arguments("   789   ", 789)]
    [Arguments(null, 0)]
    [Arguments("", 0)]
    [Arguments("   ", 0)]
    [Arguments("abc", 0)]
    [Arguments("123abc", 0)]
    [Arguments("9999999999", 0)] // Overflow case
    public void ToInt_ShouldReturnExpectedResult(string? input, int expected)
    {
        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToInt_ShouldHandleMaximumIntValue()
    {
        // Arrange
        var input = int.MaxValue.ToString();

        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(int.MaxValue);
    }

    [Test]
    public void ToInt_ShouldHandleMinimumIntValue()
    {
        // Arrange
        var input = int.MinValue.ToString();

        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(int.MinValue);
    }

    [Test]
    [Arguments("2147483648", 0)] // Overflow case
    [Arguments("-2147483649", 0)] // Underflow case
    public void ToInt_ShouldReturnZeroForOverflowOrUnderflow(string? input, int expected)
    {
        // Act
        int result = input.ToInt();

        // Assert
        result.Should().Be(expected);
    }
}
