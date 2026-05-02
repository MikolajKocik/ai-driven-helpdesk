using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ADH.API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ADH.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(
            next: (innerContext) => throw new Exception("Test exception"),
            logger: loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        var responseJson = JsonDocument.Parse(responseBody);
        responseJson.RootElement.GetProperty("Message").GetString().Should().Be("Internal Server Error. Please contact support.");
        responseJson.RootElement.GetProperty("Detailed").GetString().Should().Be("Test exception");
    }
}
