using System;
using System.Collections.Generic;
using ADH.Infrastructure.BackgroundServices;
using ADH.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ADH.Tests.BackgroundServices;

public class UserSyncServiceTests
{
    [Theory]
    [InlineData("CN=Domain Admins,OU=Groups,DC=company,DC=com", "Admin")]
    [InlineData("CN=IT-Staff,OU=Groups,DC=company,DC=com", "Agent")]
    [InlineData("CN=Users,OU=Groups,DC=company,DC=com", "Client")]
    public void MapGroupsToRole_ShouldAssignCorrectRole(string groupName, string expectedRole)
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<IAppLogger<UserSyncService>>();
        var service = new UserSyncService(serviceProviderMock.Object, loggerMock.Object);

        // Act
        var method = typeof(UserSyncService).GetMethod("MapGroupsToRole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (string)method!.Invoke(service, new object[] { new List<string> { groupName } })!;

        // Assert
        result.Should().Be(expectedRole);
    }
}
