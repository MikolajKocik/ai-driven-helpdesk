using ADH.Application.Interfaces;
using ADH.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace ADH.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IUserRepository userRepo)
    {
        await context.Database.MigrateAsync();

        var existingAdmin = await userRepo.GetByUsernameAsync("admin");
        if (existingAdmin == null)
        {
            await userRepo.AddAsync(new AppUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin")
            });
        }

        if (!context.AssetTypes.Any())
        {
            context.AssetTypes.AddRange(new[]
            {
                new AssetType { Name = "Network Device" },
                new AssetType { Name = "Laptop" },
                new AssetType { Name = "Server" },
                new AssetType { Name = "Mobile" }
            });
            await context.SaveChangesAsync();
        }
    }
}
