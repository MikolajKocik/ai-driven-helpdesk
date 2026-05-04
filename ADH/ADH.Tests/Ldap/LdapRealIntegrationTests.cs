using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using ADH.Infrastructure.Services.Identity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ADH.Tests.Ldap;

public sealed class LdapRealIntegrationTests
{
    private readonly IConfiguration _configuration;
    private readonly Mock<IAppLogger<LdapService>> _loggerMock;

    public LdapRealIntegrationTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ldap:Server"] = "localhost",
                ["Ldap:Domain"] = "adh.local",
                ["Ldap:SearchBase"] = "dc=adh,dc=local",
                ["Ldap:ServiceUser"] = "cn=admin,dc=adh,dc=local",
                ["Ldap:ServicePassword"] = "admin",
                ["Ldap:UseSsl"] = "false"
            })
            .Build();

        _loggerMock = new Mock<IAppLogger<LdapService>>();
    }

    [Fact(Skip = "Requires running Docker LDAP container")]
    public void Authenticate_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        LdapService service = new LdapService(_configuration, _loggerMock.Object);

        // Act
        bool result = service.Authenticate("mkocik", "password123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact(Skip = "Requires running Docker LDAP container")]
    public async Task GetUsersAsync_ReturnsSampleUser()
    {
        // Arrange
        LdapService service = new LdapService(_configuration, _loggerMock.Object);

        // Act
        IEnumerable<LdapUserDto> users = await service.GetUsersAsync("mkocik");

        // Assert
        users.Should().NotBeEmpty();
        users.First().Username.Should().Be("mkocik");
    }

    [Fact(Skip = "Requires running Docker LDAP container and SSL")]
    public async Task ResetPasswordAsync_ChangesPasswordSuccessfully()
    {
        // Arrange
        LdapService service = new LdapService(_configuration, _loggerMock.Object);

        // Act
        bool result = await service.ResetPasswordAsync("mkocik", "NewSecurePass123!");

        // Assert
        result.Should().BeTrue();
        
        // Verify new password
        bool authResult = service.Authenticate("mkocik", "NewSecurePass123!");
        authResult.Should().BeTrue();
    }
}
