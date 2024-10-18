using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaleido.Common.Services.Grpc.Repositories;

public abstract class BaseRepository<T, U> : IBaseRepository<T>
    where T : BaseEntity
    where U : DbContext
{
    protected readonly ILogger<object> _logger;
    protected readonly U _dbContext;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ILogger<object> logger, U dbContext, DbSet<T> dbSet)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dbSet = dbSet;
    }

    public async Task<T?> GetActiveAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get called with Id: {Id}", key);
        // Get item where id is key and state is active
        var entity = await _dbSet.Where(p => p.Key == key && p.Status == EntityStatus.Active).FirstOrDefaultAsync(cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<T>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAll called");
        return await GetAllByStatusAsync(EntityStatus.Active, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAll called");
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllByStatusAsync(EntityStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllByStatus called with status: {Status}", status);
        return await _dbSet.Where(p => p.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity.Key == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(entity.Key));
        }

        if (await ExistsAsync(entity.Key, cancellationToken))
        {
            throw new ArgumentException($"Entity with key {entity.Key} already exists");
        }

        if (entity.Status != EntityStatus.Active)
        {
            throw new ArgumentException($"Entity status must be {EntityStatus.Active}");
        }

        _logger.LogInformation("Creating {EntityName} with key: {Key}", typeof(T).Name, entity.Key);
        var storedEntity = await SaveEntityAsync(entity, cancellationToken);
        _logger.LogInformation("{EntityName} with key: {Key} created", typeof(T).Name, entity.Key);
        return storedEntity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity.Key == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(entity.Key));
        }

        if (!await ExistsAsync(entity.Key, cancellationToken))
        {
            throw new ArgumentException($"Entity with key {entity.Key} does not exist");
        }

        _logger.LogInformation("Updating {EntityName} with key: {Key}", typeof(T).Name, entity.Key);
        var archivedEntity = await UpdateStatusAsync(entity.Key, EntityStatus.Archived, cancellationToken);

        entity.Revision = archivedEntity?.Revision + 1 ?? 1;
        entity.Status = EntityStatus.Active;
        var storedEntity = await SaveEntityAsync(entity, cancellationToken);
        _logger.LogInformation("{EntityName} with key: {Key} updated", typeof(T).Name, entity.Key);
        return storedEntity;
    }

    public async Task<T?> UpdateStatusAsync(Guid key, EntityStatus status, CancellationToken cancellationToken = default)
    {
        var entity = await GetActiveAsync(key, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        _logger.LogInformation("Updating {EntityName} with key: {Key} to status: {Status}", typeof(T).Name, entity.Key, status);
        entity.Status = status;
        var stored = await UpdateEntityAsync(entity, cancellationToken);
        return stored;
    }

    public async Task<T?> DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(key, EntityStatus.Deleted, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRevisions called with Id: {Id}", key);
        return await _dbSet
            .Where(p => p.Key == key)
            .OrderByDescending(p => p.Revision)
            .ToListAsync(cancellationToken);
    }

    public async Task<T?> GetRevisionAsync(Guid key, int revision, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRevision called with Id: {Id} and Revision: {Revision}", key, revision);
        var entity = await _dbSet
            .Where(p => p.Key == key && p.Revision == revision)
            .FirstOrDefaultAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> ExistsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.Key == key, cancellationToken);
    }

    private async Task<T> SaveEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        var storedEntity = await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }

    private async Task<T> UpdateEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        var storedEntity = _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }
}
