using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Repositories;

public class BaseRevisionRepository<TRevision, RevisionContext> : BaseRevisionRepository<TRevision, BaseRevisionBuilder<TRevision>, RevisionContext>, IBaseRevisionRepository<TRevision>
where TRevision : BaseRevisionEntity, new()
where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
{
    public BaseRevisionRepository(
    DbSet<TRevision> dbSet,
    RevisionContext context
) : base(dbSet, context)
    { }
}

public class BaseRevisionRepository<RevisionContext> : BaseRevisionRepository<BaseRevisionEntity, BaseRevisionBuilder, RevisionContext>, IBaseRevisionRepository
where RevisionContext : DbContext, IKaleidoDbContext<BaseRevisionEntity>
{

    public BaseRevisionRepository(
        DbSet<BaseRevisionEntity> dbSet,
        RevisionContext context
    ) : base(dbSet, context)
    { }
}

public class BaseRevisionRepository<TRevision, TBuilder, RevisionContext> : IBaseRevisionRepository<TRevision, TBuilder>
where TRevision : BaseRevisionEntity, new()
where TBuilder : BaseRevisionBuilder<TRevision>, new()
where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
{
    protected readonly DbSet<TRevision> DbSet;
    protected readonly RevisionContext Context;

    public BaseRevisionRepository(
        DbSet<TRevision> dbSet,
        RevisionContext context
    )
    {
        DbSet = dbSet;
        Context = context;
    }

    public virtual async Task<TRevision> CreateAsync(Guid entityId, TRevision? revision = null, CancellationToken cancellationToken = default)
    {
        var revisionBuilder = InitializeRevisionBuilder();
        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId, RevisionAction.Created, 1, revision);
        var newRevision = await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
        return newRevision;
    }

    public virtual async Task<TRevision> DeleteAsync(Guid revisionKey, Guid? entityId = null, TRevision? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        ValidateDeleteOperation(previousRevision, entityId);

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId ?? previousRevision.EntityId, RevisionAction.Deleted, previousRevision.Revision + 1, revision);
        var newRevision = await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
        await MarkRevisionAsArchived(previousRevision, cancellationToken);
        return newRevision;
    }

    public virtual async Task<IEnumerable<TRevision>> GetAllAsync(Guid? revisionKey = null, CancellationToken cancellationToken = default)
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

    public virtual async Task<TRevision?> GetAsync(Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default)
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

    public virtual Task<TRevision?> GetHistoricAsync(Guid revisionKey, DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        return DbSet.Where(x => x.Key == revisionKey).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.CreatedAt <= pointInTime, cancellationToken);
    }

    public virtual async Task<TRevision> RestoreAsync(Guid revisionKey, Guid? entityId = null, TRevision? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        if (previousRevision.Action != RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Cannot restore a revision that has not been deleted.");
        }

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId ?? previousRevision.EntityId, RevisionAction.Restored, previousRevision.Revision + 1, revision);
        var newRevision = await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
        await MarkRevisionAsArchived(previousRevision, cancellationToken);
        return newRevision;
    }

    public virtual async Task<TRevision> UpdateAsync(Guid revisionKey, Guid entityId, TRevision? revision = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await ValidateUpdateAsync(revisionKey, entityId, cancellationToken);
        var revisionBuilder = InitializeRevisionBuilder(previousRevision);

        revisionBuilder = ConfigureRevisionBuilder(revisionBuilder, entityId, RevisionAction.Updated, previousRevision.Revision + 1, revision);
        var newRevision = await SaveEntityAsync(revisionBuilder.Build(), cancellationToken);
        await MarkRevisionAsArchived(previousRevision, cancellationToken);
        return newRevision;
    }

    public virtual async Task<TRevision> ValidateUpdateAsync(Guid revisionKey, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        var previousRevision = await GetRevisionOrThrow(revisionKey, cancellationToken);

        if (previousRevision.Action == RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Cannot update a deleted revision.");
        }

        if (entityId != null && previousRevision.EntityId == entityId)
        {
            throw new NotModifiedException("Update revision for this entity already exists");
        }

        return previousRevision;
    }

    public virtual async Task<IEnumerable<TRevision>> GetAllByEntityIdAsync(Guid entityId, Guid? revisionKey = null, CancellationToken cancellationToken = default)
    {
        if (revisionKey != null)
        {
            var revisions = await GetAllAsync((Guid)revisionKey, cancellationToken);
            return revisions.Where(r => r.EntityId == entityId);
        }
        return await DbSet.Where(r => r.EntityId == entityId).ToListAsync(cancellationToken);
    }

    public virtual async Task<TRevision?> FindAsync(Expression<Func<TRevision, bool>> predicate, Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default)
    {
        if (revisionKey == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(revisionKey));
        }

        return await DbSet.Where(r => r.Key == revisionKey).Where(predicate).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<TRevision>> FindAllAsync(Expression<Func<TRevision, bool>> predicate, Guid? revisionKey = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TRevision> query = DbSet;
        if (revisionKey != null && revisionKey != Guid.Empty)
        {
            query = query.Where(r => r.Key == revisionKey);
        }
        return await query.Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<TRevision>> GetAllByStatusAsync(RevisionStatus status, Guid? revisionKey = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TRevision> query = DbSet;
        if (revisionKey != null && revisionKey != Guid.Empty)
        {
            query = query.Where(r => r.Key == revisionKey);
        }
        return await query.Where(r => r.Status == status).ToListAsync(cancellationToken);
    }

    private async Task<TRevision> SaveEntityAsync(TRevision entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        var storedEntity = await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }

    private TBuilder InitializeRevisionBuilder(TRevision? revision = null)
    {
        var revisionBuilder = new TBuilder();
        if (revision != null)
        {
            revisionBuilder = (TBuilder)revisionBuilder.FromRevision(revision);
            revisionBuilder = (TBuilder)revisionBuilder.WithCreatedAt(DateTime.UtcNow);
        }
        if (revisionBuilder.Build().Key == Guid.Empty)
        {
            revisionBuilder = (TBuilder)revisionBuilder.WithKey(Guid.NewGuid());
        }
        return revisionBuilder;
    }

    private TBuilder ConfigureRevisionBuilder(TBuilder revisionBuilder, Guid entityId, RevisionAction action, int revision, TRevision? revisionEntity = null)
    {
        if (entityId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(entityId), "An entity ID is required for creating or updating revisions");
        }

        if ((revisionEntity == null || revisionEntity.CreatedAt == default) && revisionBuilder.Build().CreatedAt == default)
        {
            revisionBuilder = (TBuilder)revisionBuilder.WithCreatedAt(DateTime.UtcNow);
        }

        return (TBuilder)revisionBuilder
            .FromRevision(revisionEntity)
            .WithAction(action)
            .WithEntityId(entityId)
            .WithRevision(revision);
    }

    private async Task<TRevision> GetRevisionOrThrow(Guid revisionKey, CancellationToken cancellationToken)
    {
        var revision = await GetAsync(revisionKey, cancellationToken: cancellationToken);
        if (revision == null)
        {
            throw new RevisionNotFoundException($"The specified revision with key {revisionKey} does not exist.");
        }
        return revision;
    }

    private void ValidateDeleteOperation(TRevision previousRevision, Guid? entityId)
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

    private async Task<TRevision> MarkRevisionAsArchived(TRevision revision, CancellationToken cancellationToken)
    {
        revision.Status = RevisionStatus.Archived;
        return await UpdateEntityAsync(revision, cancellationToken);
    }

    private async Task<TRevision> UpdateEntityAsync(TRevision revision, CancellationToken cancellationToken)
    {
        var storedEntity = DbSet.Update(revision);
        await Context.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }
}
