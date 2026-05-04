using ADH.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ADH.Tests;

[CollectionDefinition("Postgres collection")]
public class PostgresCollection : ICollectionFixture<PostgresTestContainer>
{
}

[Collection("Postgres collection")]
public abstract class RepositoryTestBase
{
    protected readonly ApplicationDbContext Context;
    protected readonly PostgresTestContainer Container;

    protected RepositoryTestBase(PostgresTestContainer container)
    {
        Container = container;
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Container.ConnectionString, x => x.UseVector())
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }
}
