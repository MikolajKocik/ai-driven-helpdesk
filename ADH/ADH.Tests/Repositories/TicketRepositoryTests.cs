using ADH.Core.Entities;
using ADH.Application.Interfaces;
using ADH.Application.Interfaces;
using ADH.Infrastructure.Persistence;
using ADH.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ADH.Tests.Repositories;

public class TicketRepositoryTests : RepositoryTestBase
{
    private readonly TicketRepository _repository;
    private readonly Mock<IAppLogger<Ticket>> _loggerMock;
    private readonly Mock<ICurrentUserService> _userServiceMock;

    public TicketRepositoryTests(PostgresTestContainer container) : base(container)
    {
        _loggerMock = new Mock<IAppLogger<Ticket>>();
        _userServiceMock = new Mock<ICurrentUserService>();
        
        _repository = new TicketRepository(Context, _loggerMock.Object, _userServiceMock.Object);
        
        // Cleanup for isolation
        Context.Tickets.RemoveRange(Context.Tickets);
        Context.Users.RemoveRange(Context.Users);
        Context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_Should_Add_Ticket_To_Database()
    {
        // Arrange
        var user = new AppUser { 
            Id = Guid.NewGuid(), 
            Username = "testuser", 
            PasswordHash = "hash" 
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        
        var ticket = new Ticket { 
            Description = "Test Description", 
            UserId = user.Id 
        };

        // Act
        await _repository.AddAsync(ticket, CancellationToken.None);

        // Assert
        var dbTicket = await Context.Tickets.FirstOrDefaultAsync();
        dbTicket.Should().NotBeNull();
        dbTicket!.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetAllForUserAsync_Should_Return_Only_User_Tickets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        Context.Users.AddRange(new List<AppUser>
        {
            new AppUser { 
                Id = userId, 
                Username = "user1", 
                PasswordHash = "hash" 
            },
            new AppUser { 
                Id = otherUserId, 
                Username = "user2", 
                PasswordHash = "hash" 
            }
        });
        await Context.SaveChangesAsync();
        
        Context.Tickets.AddRange(new List<Ticket>
        {
            new Ticket { 
                Description = "User Ticket 1", 
                UserId = userId 
            },
            new Ticket { 
                Description = "User Ticket 2", 
                UserId = userId 
            },
            new Ticket { 
                Description = "Other Ticket", 
                UserId = otherUserId 
            }
        });
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForUserAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(t => t.UserId == userId).Should().BeTrue();
    }
}
