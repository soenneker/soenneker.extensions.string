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

    [Test]
    [Arguments("123", 123L)]
    [Arguments("0", 0L)]
    [Arguments("-456", -456L)]
    [Arguments("   789   ", 789L)]
    [Arguments(null, 0L)]
    [Arguments("", 0L)]
    [Arguments("   ", 0L)]
    [Arguments("abc", 0L)]
    [Arguments("123abc", 0L)]
    public void ToLong_ShouldReturnExpectedResult(string? input, long expected)
    {
        // Act
        long result = input.ToLong();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToLong_ShouldHandleMaximumLongValue()
    {
        // Arrange
        var input = long.MaxValue.ToString();

        // Act
        long result = input.ToLong();

        // Assert
        result.Should().Be(long.MaxValue);
    }

    [Test]
    public void ToLong_ShouldHandleMinimumLongValue()
    {
        // Arrange
        var input = long.MinValue.ToString();

        // Act
        long result = input.ToLong();

        // Assert
        result.Should().Be(long.MinValue);
    }

    [Test]
    public void ToLong_ShouldReturnZeroForOverflowOrUnderflow()
    {
        // Arrange
        const string maxPlusOne = "9223372036854775808";
        const string minMinusOne = "-9223372036854775809";

        // Act
        long maxResult = maxPlusOne.ToLong();
        long minResult = minMinusOne.ToLong();

        // Assert
        maxResult.Should().Be(0);
        minResult.Should().Be(0);
    }
}
