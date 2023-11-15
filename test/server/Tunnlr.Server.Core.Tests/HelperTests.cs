using FluentAssertions;

namespace Tunnlr.Server.Core.Tests;

public class HelperTests
{
    [Fact]
    public void ValidateAndConvertDomainLength_NoTruncation()
    {
        // Arrange & act
        var result = Helpers.Domains.ValidateAndConvertDomainLength("abc");

        // Assert
        result.Should().Be("abc");
    }
    
    [Fact]
    public void ValidateAndConvertDomainLength_Truncates()
    {
        // Arrange & act
        var result = Helpers.Domains.ValidateAndConvertDomainLength("thisdomainiswaytoolongandshouldbetruncated-9117db3e-29c5-4665-b8f3-6bfbfcc458bd");

        // Assert
        result.Should().Be("oolongandshouldbetruncated-9117db3e-29c5-4665-b8f3-6bfbfcc458bd");
    }
}