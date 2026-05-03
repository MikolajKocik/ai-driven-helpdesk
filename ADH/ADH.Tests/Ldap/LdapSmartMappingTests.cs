using System;
using System.Collections.Generic;
using ADH.Infrastructure.BackgroundServices;
using ADH.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using System.Reflection;

namespace ADH.Tests.BackgroundServices;

public class LdapSmartMappingTests
{
    private readonly UserSyncService _service;

    public LdapSmartMappingTests()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<IAppLogger<UserSyncService>>();
        _service = new UserSyncService(serviceProviderMock.Object, loggerMock.Object);
    }

    [Fact]
    public void MapGroupsToRole_UserInBothAdminAndAgentGroups_ReturnsAdmin()
    {
        // Arrange
        var groups = new List<string> 
        { 
            "CN=Helpdesk-Agents,OU=Groups,DC=corp", 
            "CN=Domain Admins,OU=Groups,DC=corp" 
        };

        // Act
        MethodInfo? method = typeof(UserSyncService).GetMethod("MapGroupsToRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string result = (string)method!.Invoke(_service, new object[] { groups })!;

        // Assert
        result.Should().Be("Admin");
    }

    [Fact]
    public void MapGroupsToRole_UserInAgentAndClientGroups_ReturnsAgent()
    {
        // Arrange
        List<string> groups = new List<string> 
        { 
            "CN=IT-Staff,OU=Groups,DC=corp", 
            "CN=Users,OU=Groups,DC=corp" 
        };

        // Act
        MethodInfo? method = typeof(UserSyncService).GetMethod("MapGroupsToRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string result = (string)method!.Invoke(_service, new object[] { groups })!;

        // Assert
        result.Should().Be("Agent");
    }

    [Fact]
    public void MapGroupsToRole_EmptyGroups_ReturnsClient()
    {
        // Arrange
        List<string> groups = new List<string>();

        // Act
        MethodInfo? method = typeof(UserSyncService).GetMethod("MapGroupsToRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string result = (string)method!.Invoke(_service, new object[] { groups })!;

        // Assert
        result.Should().Be("Client");
    }
}
