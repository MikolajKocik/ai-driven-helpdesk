using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Services.Plugins.Ldap;
using FluentAssertions;
using Moq;
using Xunit;

namespace ADH.Tests.Plugins;

public class LdapAccountPluginTests
{
    private readonly Mock<ILdapService> _ldapServiceMock;
    private readonly Mock<IAppLogger<LdapAccountPlugin>> _loggerMock;
    private readonly LdapAccountPlugin _plugin;

    public LdapAccountPluginTests()
    {
        _ldapServiceMock = new Mock<ILdapService>();
        _loggerMock = new Mock<IAppLogger<LdapAccountPlugin>>();
        _plugin = new LdapAccountPlugin(_ldapServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UnlockAccount_WhenSuccess_ReturnsSuccessMessage()
    {
        // Arrange
        _ldapServiceMock.Setup(s => s.UnlockUserAsync("jdoe")).ReturnsAsync(true);

        // Act
        var result = await _plugin.UnlockAccount("jdoe");

        // Assert
        result.Should().Contain("SUCCESS");
        _ldapServiceMock.Verify(s => s.UnlockUserAsync("jdoe"), Times.Once);
    }

    [Fact]
    public async Task UnlockAccount_WhenFailure_ReturnsFailureMessage()
    {
        // Arrange
        _ldapServiceMock.Setup(s => s.UnlockUserAsync("jdoe")).ReturnsAsync(false);

        // Act
        var result = await _plugin.UnlockAccount("jdoe");

        // Assert
        result.Should().Contain("FAILURE");
    }
}
