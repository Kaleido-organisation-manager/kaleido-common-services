using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Kaleido.Common.Services.Grpc.Repositories;

public class BaseRevisionRepository<TRevision> : BaseRevisionRepository<TRevision, BaseRevisionBuilder<TRevision>>
where TRevision : BaseRevisionEntity, new()
{
    public BaseRevisionRepository(
    DbSet<TRevision> dbSet,
    DbContext context,
    ILogger<TRevision> logger
) : base(dbSet, context, logger)
    { }
}

public class BaseRevisionRepository : BaseRevisionRepository<BaseRevisionEntity, BaseRevisionBuilder>, IBaseRevisionRepository
{

    public BaseRevisionRepository(
        DbSet<BaseRevisionEntity> dbSet,
        DbContext context,
        ILogger<BaseRevisionRepository> logger
    ) : base(dbSet, context, logger)
    { }
}

public class BaseRevisionRepository<TRevisionEntity, TBuilder> : IBaseRevisionRepository<TRevisionEntity, TBuilder>
where TRevisionEntity : BaseRevisionEntity, new()
where TBuilder : BaseRevisionBuilder<TRevisionEntity>, new()
{
    protected readonly DbSet<TRevisionEntity> DbSet;
    protected readonly DbContext Context;
    protected readonly ILogger Logger;

    public BaseRevisionRepository(
        DbSet<TRevisionEntity> dbSet,
        DbContext context,
        ILogger logger
    )
    {
        DbSet = dbSet;
        Context = context;
        Logger = logger;
    }

    public async Task<TRevisionEntity> CreateAsync(Guid entityId, TRevisionEntity? revision = null, CancellationToken cancellationToken = default)
    {
        var revisionBuilder = InitializeRevisionBuilder(revision);
        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId, RevisionAction.Created, 1);
        return await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
    }

    public async Task<TRevisionEntity> DeleteAsync(Guid revisionKey, Guid? entityId = null, TRevisionEntity? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        ValidateDeleteOperation(previousRevision, entityId);

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId ?? previousRevision.EntityId, RevisionAction.Deleted, previousRevision.Revision + 1, revision);
        return await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
    }

    public async Task<IEnumerable<TRevisionEntity>> GetAllAsync(Guid? revisionKey = null, CancellationToken cancellationToken = default)
    {
        if (revisionKey != null && revisionKey != Guid.Empty)
        {
            return await DbSet.Where(x => x.Key == revisionKey).ToListAsync(cancellationToken);
        }
        return await DbSet
            .GroupBy(r => r.Key)
            .Select(g => g.OrderByDescending(r => r.Revision).First())
            .ToListAsync(cancellationToken);
    }

    public async Task<TRevisionEntity?> GetAsync(Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default)
    {
        if (revisionKey == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(revisionKey));
        }

        if (revision != null)
        {
            return await DbSet.Where(x => x.Key == revisionKey).FirstOrDefaultAsync(x => x.Revision == revision, cancellationToken);
        }
        return await DbSet.Where(x => x.Key == revisionKey).OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TRevisionEntity?> GetHistoricAsync(Guid revisionKey, DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        return DbSet.Where(x => x.Key == revisionKey).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.CreatedAt <= pointInTime, cancellationToken);
    }

    public async Task<TRevisionEntity> RestoreAsync(Guid revisionKey, Guid? entityId = null, TRevisionEntity? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        if (previousRevision.Action != RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Cannot restore a revision that has not been deleted.");
        }

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId ?? previousRevision.EntityId, RevisionAction.Restored, previousRevision.Revision + 1, revision);
        return await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
    }

    public async Task<TRevisionEntity> UpdateAsync(Guid revisionKey, Guid entityId, TRevisionEntity? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await ValidateUpdateAsync(revisionKey, entityId, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        if (previousRevision.Action == RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Cannot update a deleted revision.");
        }

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId, RevisionAction.Updated, previousRevision.Revision + 1, revision);
        return await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
    }

    public async Task<TRevisionEntity> ValidateUpdateAsync(Guid revisionKey, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);

        if (previousRevision.Action == RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Cannot update a deleted revision.");
        }

        if (entityId != null && previousRevision.EntityId == entityId)
        {
            throw new InvalidOperationException("Update revision for this entity already exists");
        }

        return previousRevision;
    }

    public async Task<IEnumerable<TRevisionEntity>> GetAllByEntityIdAsync(Guid entityId, Guid? revisionKey = null, CancellationToken cancellationToken = default)
    {
        if (revisionKey != null)
        {
            var revisions = await GetAllAsync((Guid)revisionKey, cancellationToken);
            return revisions.Where(r => r.EntityId == entityId);
        }
        return await DbSet.Where(r => r.EntityId == entityId).ToListAsync(cancellationToken);
    }

    private async Task<TRevisionEntity> SaveEntityAsync(TRevisionEntity entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        var storedEntity = await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }

    private TBuilder InitializeRevisionBuilder(TRevisionEntity? revision)
    {
        var revisionBuilder = new TBuilder();
        if (revision != null)
        {
            revisionBuilder = (TBuilder)revisionBuilder.FromRevision(revision);
        }
        if (revisionBuilder.Build().Key == Guid.Empty)
        {
            revisionBuilder = (TBuilder)revisionBuilder.WithKey(Guid.NewGuid());
        }
        return revisionBuilder;
    }

    private TBuilder ConfigureRevisionBuilder(TBuilder revisionBuilder, Guid entityId, RevisionAction action, int revision, TRevisionEntity? revisionEntity = null)
    {
        if (entityId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(entityId), "An entity ID is required for creating or updating revisions");
        }

        return (TBuilder)revisionBuilder
            .FromRevision(revisionEntity)
            .WithAction(action)
            .WithEntityId(entityId)
            .WithRevision(revision);
    }

    private async Task<TRevisionEntity> GetRevisionOrThrow(Guid revisionKey, CancellationToken cancellationToken)
    {
        var revision = await GetAsync(revisionKey, cancellationToken: cancellationToken);
        if (revision == null)
        {
            throw new ArgumentNullException($"The specified revision with key {revisionKey} does not exist.");
        }
        return revision;
    }

    private void ValidateDeleteOperation(TRevisionEntity previousRevision, Guid? entityId)
    {
        if (previousRevision.Action == RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Delete operation is not allowed on a previously deleted revision.");
        }

        if (entityId == null && (previousRevision.EntityId == Guid.Empty))
        {
            throw new ArgumentException("Entity ID is required when the previous revision does not have one.", nameof(entityId));
        }
    }
}
