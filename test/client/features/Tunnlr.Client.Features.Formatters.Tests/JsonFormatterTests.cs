using FluentAssertions;
using Tunnlr.Client.Core.Models;
using Tunnlr.Common.Protobuf;
using HttpMethod = Tunnlr.Common.Protobuf.HttpMethod;

namespace Tunnlr.Client.Features.Formatters.Tests;

public class JsonFormatterTests
{
    [Fact]
    public void IsJsonRequest_True()
    {
        // Arrange
        var sut = new JsonFormatter();
        var httpRequest = new HttpRequest
        {
            TargetUri = "https://localhost:5109",
            HttpMethod = HttpMethod.Post,
        };
        httpRequest.Headers.Add("Content-Type", "application/json");
        var request = new Request(httpRequest);
        
        // Act
        var result = sut.IsJsonRequest(request);

        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void IsJsonRequest_False()
    {
        // Arrange
        var sut = new JsonFormatter();
        var httpRequest = new HttpRequest
        {
            TargetUri = "https://localhost:5109",
            HttpMethod = HttpMethod.Post,
        };
        var request = new Request(httpRequest);
        
        // Act
        var result = sut.IsJsonRequest(request);

        // Assert
        result.Should().BeFalse();
    }
}