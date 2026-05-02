using ADH.Core.Interfaces;
using ADH.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ADH.Tests;

public class LdapServiceTests
{
    [Fact]
    public async Task UnlockUserAsync_InvalidConfig_ReturnsFalse()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Ldap:Server"]).Returns(""); // Empty config
        
        var loggerMock = new Mock<IAppLogger<LdapService>>();
        var service = new LdapService(configMock.Object, loggerMock.Object);

        // Act
        var result = await service.UnlockUserAsync("testuser");

        // Assert
        result.Should().BeFalse();
    }
}
