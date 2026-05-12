using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ADH.Core.Attributes;
using ADH.Core.Entities;
using ADH.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ADH.Infrastructure.Repositories;

/// <summary>
/// A generic base repository providing standard CRUD operations with ACID transaction support and structured auditing.
/// </summary>
public abstract class BaseRepository<TEntity, TContext> 
    where TEntity : class 
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<TEntity> DbSet;
    protected readonly ICurrentUserService? CurrentUserService;
    protected readonly IAppLogger<TEntity> Logger;
    private readonly bool _isAuditable;

    protected BaseRepository(TContext context, IAppLogger<TEntity> logger, ICurrentUserService? currentUserService = null)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
        Logger = logger;
        CurrentUserService = currentUserService;
        _isAuditable = typeof(TEntity).GetCustomAttribute<AuditableAttribute>() != null;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbSet.FindAsync(id, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (!_isAuditable || Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync(cancellationToken);
            return;
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync(cancellationToken);
            
            await AuditInternalAsync(entity, "Create", cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            Logger.LogBusinessAction(typeof(TEntity).Name, "New", "Created");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            Logger.LogError(ex, "Transaction failed for Add operation on {Entity}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (!_isAuditable || Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
            return;
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
            
            await AuditInternalAsync(entity, "Update", cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            string id = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "Unknown";
            Logger.LogBusinessAction(typeof(TEntity).Name, id, "Updated");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            Logger.LogError(ex, "Transaction failed for Update operation on {Entity}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        TEntity? entity = await DbSet.FindAsync(id, cancellationToken);
        if (entity == null) return;

        if (!_isAuditable)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
            return;
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(cancellationToken);
            
            await AuditInternalAsync(entity, "Delete", cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            Logger.LogBusinessAction(typeof(TEntity).Name, id.ToString(), "Deleted");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Logger.LogError(ex, "Transaction failed for Delete operation on {Entity}", typeof(TEntity).Name);
            throw;
        }
    }

    private async Task AuditInternalAsync(TEntity entity, string action, CancellationToken cancellationToken)
    {
        try
        {
            string entityId = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "Unknown";
            
            AuditLog log = new AuditLog
            {
                EntityName = typeof(TEntity).Name,
                EntityId = entityId,
                Action = action,
                Changes = JsonSerializer.Serialize(entity, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                }),
                UserId = CurrentUserService?.UserId ?? "System",
                Timestamp = DateTime.UtcNow
            };

            await Context.Set<AuditLog>().AddAsync(log, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Audit log internal failure for {Entity}", typeof(TEntity).Name);
            throw;
        }
    }
}
