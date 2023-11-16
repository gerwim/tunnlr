using FluentAssertions;

namespace Tunnlr.Client.Web.Tests;

public class HelperTests
{
    [Theory]
    [InlineData(500, "500ms")]
    [InlineData(1500, "1.5s")]
    [InlineData(1750, "1.8s")]
    [InlineData(30*1000, "30s")]
    [InlineData(60*1000, "1m")]
    [InlineData(60001, "1m")]
    [InlineData(90*1000, "1m30s")]
    [InlineData(500*1000, "8m20s")]
    [InlineData(3660*1000, "1h1m")]
    [InlineData(3661*1000, "1h1m1s")]
    [InlineData(3600*24*1000, "1d")]
    [InlineData((3600*24+1)*1000, "1d1s")]
    public void RequestDurationFormatter_CorrectOutput(int totalMilliseconds, string expectedOutput)
    {
        // Arrange
        var start = DateTime.Now;
        var end = start.AddMilliseconds(totalMilliseconds);
        
        // Act
        var result = Helpers.RequestDurationFormatter.FormatRequestDuration(start, end);

        // Assert
        result.Should().Be(expectedOutput);
    }
}