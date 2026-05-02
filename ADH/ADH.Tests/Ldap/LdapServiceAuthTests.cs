using System.Collections.Generic;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace ADH.Tests.Ldap;

public class LdapServiceAuthTests
{
    [Fact]
    public void Authenticate_PrimaryFailsButPartnerSucceeds_ReturnsTrue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ldap:Server"] = "primary.local",
                ["Ldap:Partners:Partner1:Server"] = "partner1.local",
                ["Ldap:Partners:Partner1:Domain"] = "PARTNER1"
            })
            .Build();

        var loggerMock = new Mock<IAppLogger<LdapService>>();
        
        // Use Mock to override protected virtual method
        var serviceMock = new Mock<LdapService>(config, loggerMock.Object) { CallBase = true };
        
        // Primary fails
        serviceMock.Protected()
            .Setup<bool>("AuthenticateInternal", 
                ItExpr.IsAny<string>(), 
                ItExpr.IsAny<string>(), 
                ItExpr.Is<IConfigurationSection>(s => s.Key == "Ldap"))
            .Returns(false);

        // Partner succeeds
        serviceMock.Protected()
            .Setup<bool>("AuthenticateInternal", 
                ItExpr.IsAny<string>(), 
                ItExpr.IsAny<string>(), 
                ItExpr.Is<IConfigurationSection>(s => s.Key == "Partner1"))
            .Returns(true);

        // Act
        var result = serviceMock.Object.Authenticate("user", "pass");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Authenticate_AllFails_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ldap:Server"] = "primary.local",
                ["Ldap:Partners:Partner1:Server"] = "partner1.local"
            })
            .Build();

        var loggerMock = new Mock<IAppLogger<LdapService>>();
        var serviceMock = new Mock<LdapService>(config, loggerMock.Object) { CallBase = true };
        
        serviceMock.Protected()
            .Setup<bool>("AuthenticateInternal", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<IConfigurationSection>())
            .Returns(false);

        // Act
        var result = serviceMock.Object.Authenticate("user", "pass");

        // Assert
        result.Should().BeFalse();
    }
}
