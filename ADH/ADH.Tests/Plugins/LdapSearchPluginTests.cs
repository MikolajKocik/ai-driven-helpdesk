using System.Collections.Generic;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Application.DTOs;
using ADH.Infrastructure.Services.Plugins.Ldap;
using FluentAssertions;
using Moq;
using Xunit;

namespace ADH.Tests.Plugins;

public class LdapSearchPluginTests
{
    private readonly Mock<ILdapService> _ldapServiceMock;
    private readonly LdapSearchPlugin _plugin;

    public LdapSearchPluginTests()
    {
        _ldapServiceMock = new Mock<ILdapService>();
        _plugin = new LdapSearchPlugin(_ldapServiceMock.Object);
    }

    [Fact]
    public async Task SearchUsers_ValidFilter_CallsService()
    {
        // Arrange
        var filter = "(mail=*@google.com)";
        _ldapServiceMock.Setup(s => s.GetUsersAsync(filter))
            .ReturnsAsync(new List<LdapUserDto> { new LdapUserDto("mikocik", "m@g.com", "Miko") });

        // Act
        var result = await _plugin.SearchUsers(filter);

        // Assert
        result.Should().HaveCount(1);
        _ldapServiceMock.Verify(s => s.GetUsersAsync(filter), Times.Once);
    }

    [Fact]
    public async Task FindUsersInGroup_CorrectDn_CallsServiceWithFilter()
    {
        // Arrange
        string groupDn = "CN=Admins,DC=corp";
        string expectedFilter = $"(memberOf={groupDn})";
        
        _ldapServiceMock.Setup(s => s.GetUsersAsync(expectedFilter))
            .ReturnsAsync(new List<LdapUserDto>());

        // Act
        await _plugin.FindUsersInGroup(groupDn);

        // Assert
        _ldapServiceMock.Verify(s => s.GetUsersAsync(expectedFilter), Times.Once);
    }
}
