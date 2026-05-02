using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ADH.Infrastructure.Persistence;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace ADH.Tests.Integration;

public class IntegrationTestBase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("Environment", "Testing");
        builder.ConfigureServices(services =>
        {
            // Remove real DB
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add In-Memory DB
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });

            // Disable background jobs for integration tests to avoid interference
            var jobs = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            foreach (var job in jobs) services.Remove(job);

            // Remove AI services that cause scope validation issues
            var aiServices = services.Where(d => d.ServiceType.Name.Contains("Kernel") || d.ServiceType.Name.Contains("ChatOrchestrator")).ToList();
            foreach (var ai in aiServices) services.Remove(ai);
        });
    }

    public HttpClient CreateUnauthenticatedClient() => CreateClient();
}
