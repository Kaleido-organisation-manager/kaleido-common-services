using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Repositories.Interfaces;

public interface IBaseRevisionRepository : IBaseRevisionRepository<BaseRevisionEntity>;

public interface IBaseRevisionRepository<T>
where T : BaseRevisionEntity
{
    Task<T> CreateAsync(Guid entityId, T? revision = null, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(Guid revisionKey, Guid entityId, T? revision = null, CancellationToken cancellationToken = default);
    Task<T> DeleteAsync(Guid revisionKey, Guid? entityId = null, T? revision = null, CancellationToken cancellationToken = default);
    Task<T> RestoreAsync(Guid revisionKey, Guid? entityId = null, T? revision = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(Guid revisionKey, int? revision = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(Guid revisionKey, CancellationToken cancellationToken = default);
    Task<T?> GetHistoricAsync(Guid revisionKey, DateTime pointInTime, CancellationToken cancellationToken = default);
}