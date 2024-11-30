using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Repositories.Interfaces;

public interface IBaseRevisionRepository<TRevision> : IBaseRevisionRepository<TRevision, BaseRevisionBuilder<TRevision>>
where TRevision : BaseRevisionEntity, new();

public interface IBaseRevisionRepository : IBaseRevisionRepository<BaseRevisionEntity, BaseRevisionBuilder>;

public interface IBaseRevisionRepository<T, TBuilder>
where T : BaseRevisionEntity, new()
where TBuilder : BaseRevisionBuilder<T>, new()
{
    Task<T> CreateAsync(Guid entityId, T? revision = null, CancellationToken cancellationToken = default);
    public Task<T> UpdateAsync(Guid revisionKey, Guid entityId, T? revision = null, CancellationToken cancellationToken = default);
    public Task<T> ValidateUpdateAsync(Guid revisionKey, Guid? entityId = null, CancellationToken cancellationToken = default);
    public Task<T> DeleteAsync(Guid revisionKey, Guid? entityId = null, T? revision = null, CancellationToken cancellationToken = default);
    public Task<T> RestoreAsync(Guid revisionKey, Guid? entityId = null, T? revision = null, CancellationToken cancellationToken = default);
    public Task<T?> GetAsync(Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default);
    public Task<T?> FindAsync(Expression<Func<T, bool>> predicate, Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetAllAsync(Guid? revisionKey = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, Guid? revisionKey = null, CancellationToken cancellationToken = default);
    public Task<T?> GetHistoricAsync(Guid revisionKey, DateTime pointInTime, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetAllByEntityIdAsync(Guid entityId, Guid? revisionKey = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetAllByStatusAsync(RevisionStatus status, Guid? revisionKey = null, CancellationToken cancellationToken = default);
}