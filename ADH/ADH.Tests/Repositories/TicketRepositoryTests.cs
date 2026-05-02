using ADH.Core.Entities;
using ADH.Core.Interfaces;
using ADH.Infrastructure.Persistence;
using ADH.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ADH.Tests.Repositories;

public class TicketRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly TicketRepository _repository;
    private readonly Mock<IAppLogger<Ticket>> _loggerMock;
    private readonly Mock<ICurrentUserService> _userServiceMock;

    public TicketRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<IAppLogger<Ticket>>();
        _userServiceMock = new Mock<ICurrentUserService>();
        
        _repository = new TicketRepository(_context, _loggerMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Ticket_To_Database()
    {
        // Arrange
        var ticket = new Ticket { Description = "Test Description", UserId = Guid.NewGuid() };

        // Act
        await _repository.AddAsync(ticket);

        // Assert
        var dbTicket = await _context.Tickets.FirstOrDefaultAsync();
        dbTicket.Should().NotBeNull();
        dbTicket!.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetAllForUserAsync_Should_Return_Only_User_Tickets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        _context.Tickets.AddRange(new List<Ticket>
        {
            new Ticket { Description = "User Ticket 1", UserId = userId },
            new Ticket { Description = "User Ticket 2", UserId = userId },
            new Ticket { Description = "Other Ticket", UserId = otherUserId }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllForUserAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.All(t => t.UserId == userId).Should().BeTrue();
    }
}
