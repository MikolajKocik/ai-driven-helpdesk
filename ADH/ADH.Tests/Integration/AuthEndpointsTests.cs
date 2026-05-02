using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ADH.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace ADH.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _factory;

    public AuthEndpointsTests(IntegrationTestBase factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();
        var request = new RegisterRequest("testuser", "Password123!", "Test User", "test@example.com");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();
        var request = new LoginRequest("nonexistent", "wrongpassword");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
