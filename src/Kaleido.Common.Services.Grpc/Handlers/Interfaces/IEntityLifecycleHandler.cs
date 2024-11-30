using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Handlers.Interfaces;

public interface IEntityLifecycleHandler : IEntityLifecycleHandler<BaseEntity, BaseRevisionEntity>;

public interface IEntityLifecycleHandler<TEntity> : IEntityLifecycleHandler<TEntity, BaseRevisionEntity>
where TEntity : BaseEntity;

public interface IEntityLifecycleHandler<TEntity, TRevision>
where TEntity : BaseEntity
where TRevision : BaseRevisionEntity
{
    public Task<EntityLifeCycleResult<TEntity, TRevision>> CreateAsync(TEntity entity, TRevision? revision = null, CancellationToken cancellationToken = default);
    public Task<EntityLifeCycleResult<TEntity, TRevision>> UpdateAsync(Guid key, TEntity entity, TRevision? revision = null, CancellationToken cancellationToken = default);
    public Task<EntityLifeCycleResult<TEntity, TRevision>> DeleteAsync(Guid key, TRevision? revision = null, CancellationToken cancellationToken = default);
    public Task<EntityLifeCycleResult<TEntity, TRevision>> RestoreAsync(Guid key, TRevision? revision = null, CancellationToken cancellationToken = default);
    public Task<EntityLifeCycleResult<TEntity, TRevision>?> GetAsync(Guid key, int? revision = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> GetAllAsync(Guid? key = null, CancellationToken cancellationToken = default);
    public Task<EntityLifeCycleResult<TEntity, TRevision>?> GetHistoricAsync(Guid key, DateTime pointInTime, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, Guid? key = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TRevision, bool>> revisionPredicate, Guid? key = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAsync(Expression<Func<TEntity, bool>> predicate, Guid? key = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> FindAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TRevision, bool>> revisionPredicate, Guid? key = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<EntityLifeCycleResult<TEntity, TRevision>>> GetAllByStatusAsync(RevisionStatus status, Guid? key = null, CancellationToken cancellationToken = default);
}
