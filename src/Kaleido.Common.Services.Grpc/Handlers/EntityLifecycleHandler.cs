using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;

namespace Kaleido.Common.Services.Grpc.Handlers;

public class EntityLifeCycleHandler : EntityLifeCycleHandler<BaseEntity, BaseRevisionEntity, BaseRevisionBuilder>, IEntityLifecycleHandler
{
    public EntityLifeCycleHandler(
        IBaseEntityRepository entityRepository,
        IBaseRevisionRepository baseRevisionRepository
    ) : base(entityRepository, baseRevisionRepository) { }
}

public class EntityLifeCycleHandler<TEntity> : EntityLifeCycleHandler<TEntity, BaseRevisionEntity, BaseRevisionBuilder>, IEntityLifecycleHandler<TEntity>
where TEntity : BaseEntity, new()
{
    public EntityLifeCycleHandler(
        IBaseEntityRepository<TEntity> entityRepository,
        IBaseRevisionRepository baseRevisionRepository
    ) : base(entityRepository, baseRevisionRepository) { }
}

public class EntityLifeCycleHandler<TEntity, TRevision> : EntityLifeCycleHandler<TEntity, TRevision, BaseRevisionBuilder<TRevision>>, IEntityLifecycleHandler<TEntity, TRevision>
where TEntity : BaseEntity, new()
where TRevision : BaseRevisionEntity, new()
{
    public EntityLifeCycleHandler(
    IBaseEntityRepository<TEntity> entityRepository,
    IBaseRevisionRepository<TRevision> baseRevisionRepository
) : base(entityRepository, baseRevisionRepository) { }
}

public class EntityLifeCycleHandler<TEntity, TRevision, TBuilder> : IEntityLifecycleHandler<TEntity, TRevision>
where TEntity : BaseEntity, new()
where TRevision : BaseRevisionEntity, new()
where TBuilder : BaseRevisionBuilder<TRevision>, new()
{
    protected readonly IBaseEntityRepository<TEntity> EntityRepository;
    protected readonly IBaseRevisionRepository<TRevision, TBuilder> RevisionRepository;

    public EntityLifeCycleHandler(
        IBaseEntityRepository<TEntity> entityRepository,
        IBaseRevisionRepository<TRevision, TBuilder> revisionRepository
    )
    {
        EntityRepository = entityRepository;
        RevisionRepository = revisionRepository;
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
        }

        var resultEntity = await EntityRepository.CreateAsync(entity, cancellationToken);
        var resultRevision = await RevisionRepository.CreateAsync(resultEntity.Id, cancellationToken: cancellationToken);

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Entity = resultEntity,
            Revision = resultRevision
        };
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>> DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var revision = await RevisionRepository.DeleteAsync(key, cancellationToken: cancellationToken);
        var entity = await EntityRepository.GetAsync(revision.EntityId, cancellationToken: cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException($"Failed to retrieve the entity associated with the provided revision key '{key}'. Ensure that the entity exists and is linked correctly in the database.");
        }

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Entity = entity,
            Revision = revision
        };
    }

    public virtual async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, Guid? key = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<TRevision> revisions;

        if (key.HasValue)
        {
            revisions = await RevisionRepository.GetAllAsync(key.Value, cancellationToken);
            if (!revisions.Any())
            {
                throw new RevisionNotFoundException($"No revisions found with key {key}");
            }
        }
        else
        {
            revisions = Enumerable.Empty<TRevision>();
        }

        var entityIds = revisions.Select(r => r.EntityId).Distinct().ToList();

        var entities = key.HasValue
            ? await EntityRepository.FindAllAsync(e => entityIds.Contains(e.Id) && predicate.Compile()(e), cancellationToken)
            : await EntityRepository.FindAllAsync(predicate, cancellationToken);

        return await FormatFilterAllResult(key, entities, revisions, predicate, cancellationToken);
    }

    public virtual async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TRevision, bool>> revisionPredicate, Guid? key = null, CancellationToken cancellationToken = default)
    {
        var revisions = await RevisionRepository.FindAllAsync(revisionPredicate, key, cancellationToken);

        var entityIds = revisions.Select(r => r.EntityId).Distinct().ToList();

        if (!entityIds.Any())
        {
            return new List<EntityLifeCycleResult<TEntity, TRevision>>();
        }

        var entities = key.HasValue
            ? await EntityRepository.FindAllAsync(e => entityIds.Contains(e.Id) && predicate.Compile()(e), cancellationToken)
            : await EntityRepository.FindAllAsync(predicate, cancellationToken);

        return await FormatFilterAllResult(key, entities, revisions, predicate, cancellationToken);
    }

    private async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FormatFilterAllResult(Guid? key, IEnumerable<TEntity> entities, IEnumerable<TRevision> revisions, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = new List<EntityLifeCycleResult<TEntity, TRevision>>();

        foreach (var entity in entities)
        {
            var entityRevisions = revisions.Where(r => r.EntityId == entity.Id);
            if (!entityRevisions.Any())
            {
                var databaseRevisions = await RevisionRepository.GetAllByEntityIdAsync(entity.Id, cancellationToken: cancellationToken);
                if (databaseRevisions.Any())
                {
                    var latestRevision = await RevisionRepository.GetAsync(databaseRevisions.First().Key, cancellationToken: cancellationToken);
                    entityRevisions =
                        latestRevision != null && latestRevision.Action != RevisionAction.Deleted && latestRevision.EntityId == entity.Id
                        ? new List<TRevision>() { latestRevision }
                        : new List<TRevision>();
                }
                else
                {
                    entityRevisions = Enumerable.Empty<TRevision>();
                }
            }

            results.AddRange(entityRevisions
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new EntityLifeCycleResult<TEntity, TRevision>
                {
                    Revision = r,
                    Entity = entity
                })
            );
        }

        return key.HasValue ? results : results.GroupBy(e => e.Key).Select(g => g.OrderByDescending(e => e.Revision.Revision).First());
    }

    public virtual async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAsync(Expression<Func<TEntity, bool>> predicate, Guid? key = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<TRevision> revisions;

        if (key.HasValue)
        {
            revisions = await RevisionRepository.FindAllAsync(r => r.Action != RevisionAction.Deleted, key.Value, cancellationToken);
        }
        else
        {
            revisions = Enumerable.Empty<TRevision>();
        }

        var entityIds = revisions.Select(r => r.EntityId).Distinct().ToList();

        if (!entityIds.Any())
        {
            return new List<EntityLifeCycleResult<TEntity, TRevision>>();
        }

        var entity = key.HasValue
            ? await EntityRepository.FindAsync(e => entityIds.Contains(e.Id) && predicate.Compile()(e), cancellationToken)
            : await EntityRepository.FindAsync(predicate, cancellationToken);

        if (entity == null)
        {
            return Enumerable.Empty<EntityLifeCycleResult<TEntity, TRevision>>();
        }

        var entityRevisions = key.HasValue
            ? revisions.Where(r => r.EntityId == entity.Id)
            : await RevisionRepository.GetAllByEntityIdAsync(entity.Id);

        if (!entityRevisions.Any())
        {
            throw new RevisionNotFoundException($"Could not resolve any revisions for entity with id {entity.Id}");
        }

        return entityRevisions
            .OrderByDescending(x => x.CreatedAt)
            .Select(r => new EntityLifeCycleResult<TEntity, TRevision>
            {
                Revision = r,
                Entity = entity
            });
    }

    public virtual async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TRevision, bool>> revisionPredicate, Guid? key = null, CancellationToken cancellationToken = default)
    {
        var revisions = await RevisionRepository.FindAllAsync(revisionPredicate, key, cancellationToken);

        var entityIds = revisions.Select(r => r.EntityId).Distinct().ToList();

        if (!entityIds.Any())
        {
            return new List<EntityLifeCycleResult<TEntity, TRevision>>();
        }

        var entity = key.HasValue
            ? await EntityRepository.FindAsync(e => entityIds.Contains(e.Id) && predicate.Compile()(e), cancellationToken)
            : await EntityRepository.FindAsync(predicate, cancellationToken);

        if (entity == null)
        {
            return Enumerable.Empty<EntityLifeCycleResult<TEntity, TRevision>>();
        }

        var entityRevisions = key.HasValue
            ? revisions.Where(r => r.EntityId == entity.Id)
            : await RevisionRepository.GetAllByEntityIdAsync(entity.Id);

        if (!entityRevisions.Any())
        {
            throw new RevisionNotFoundException($"Could not resolve any revisions for entity with id {entity.Id}");
        }

        return entityRevisions
            .OrderByDescending(x => x.CreatedAt)
            .Select(r => new EntityLifeCycleResult<TEntity, TRevision>
            {
                Revision = r,
                Entity = entity
            });
    }

    public virtual async Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> GetAllAsync(Guid? key = null, CancellationToken cancellationToken = default)
    {
        var revisions = Enumerable.Empty<TRevision>();
        var entities = Enumerable.Empty<TEntity>();
        if (key != null && key != Guid.Empty)
        {
            revisions = await RevisionRepository.GetAllAsync((Guid)key, cancellationToken);

            var entityIds = revisions.Select(r => r.EntityId).ToList();
            entities = await EntityRepository.FindAllAsync(e => entityIds.Contains(e.Id), cancellationToken);

            return revisions.Select(r => new EntityLifeCycleResult<TEntity, TRevision>
            {
                Revision = r,
                Entity = entities.First(e => e.Id == r.EntityId)
            });
        }

        revisions = await RevisionRepository.GetAllAsync(cancellationToken: cancellationToken);

        var revisionEntityIds = revisions.Select(r => r.EntityId).Distinct();
        entities = await EntityRepository.FindAllAsync(e => revisionEntityIds.Contains(e.Id), cancellationToken: cancellationToken);

        var result = new List<EntityLifeCycleResult<TEntity, TRevision>>();

        foreach (var revision in revisions)
        {
            var entity = entities.FirstOrDefault(e => e.Id == revision.EntityId);
            if (entity == null)
            {
                throw new EntityNotFoundException($"Could not resolve entity for latest revision {revision.Key}");
            }
            result.Add(new EntityLifeCycleResult<TEntity, TRevision>
            {
                Revision = revision,
                Entity = entity
            });
        }

        return result;
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>?> GetAsync(Guid key, int? revision = null, CancellationToken cancellationToken = default)
    {
        var resultRevision = await RevisionRepository.GetAsync(key, revision, cancellationToken);

        if (resultRevision == null)
        {
            return null;
        }

        var resultEntity = await EntityRepository.GetAsync(resultRevision.EntityId);

        if (resultEntity == null)
        {
            throw new EntityNotFoundException("Could not resolve the entity from the revision");
        }

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Revision = resultRevision,
            Entity = resultEntity
        };
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>?> GetHistoricAsync(Guid key, DateTime pointInTime, CancellationToken cancellationToken = default)
    {
        var revision = await RevisionRepository.GetHistoricAsync(key, pointInTime, cancellationToken);

        if (revision == null)
        {
            return null;
        }

        var entity = await EntityRepository.GetAsync(revision.EntityId, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException("Could not resolve the entity from the revision");
        }

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Revision = revision,
            Entity = entity
        };
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>> RestoreAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var revision = await RevisionRepository.RestoreAsync(key, cancellationToken: cancellationToken);
        var entity = await EntityRepository.GetAsync(revision.EntityId, cancellationToken);

        if (entity == null)
        {
            throw new EntityNotFoundException("Could not resolve the entity from the revision");
        }

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Revision = revision,
            Entity = entity
        };
    }

    public virtual async Task<EntityLifeCycleResult<TEntity, TRevision>> UpdateAsync(Guid key, TEntity entity, CancellationToken cancellationToken = default)
    {
        var latestRevision = await RevisionRepository.GetAsync(key, cancellationToken: cancellationToken);

        if (latestRevision == null)
        {
            throw new RevisionNotFoundException($"No revision found with key '{key}'");
        }

        var storedEntity = await EntityRepository.GetAsync(latestRevision.EntityId, cancellationToken: cancellationToken);

        if (storedEntity == null)
        {
            throw new EntityNotFoundException($"Entity with ID '{latestRevision.EntityId}' not found");
        }

        if (storedEntity.Equals(entity))
        {
            throw new NotModifiedException("No changes detected between the existing entity and the provided updated entity. Update operation requires at least one modified field.");
        }

        await RevisionRepository.ValidateUpdateAsync(key, cancellationToken: cancellationToken);

        var updatedEntity = await EntityRepository.CreateAsync(entity, cancellationToken);
        var updateRevision = await RevisionRepository.UpdateAsync(latestRevision.Key, updatedEntity.Id, cancellationToken: cancellationToken);

        return new EntityLifeCycleResult<TEntity, TRevision>
        {
            Entity = updatedEntity,
            Revision = updateRevision
        };
    }
}
