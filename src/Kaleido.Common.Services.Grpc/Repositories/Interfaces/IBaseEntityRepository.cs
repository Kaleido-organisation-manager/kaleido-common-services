using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Repositories.Interfaces;

public interface IBaseEntityRepository : IBaseEntityRepository<BaseEntity>;

public interface IBaseEntityRepository<T>
where T : BaseEntity
{
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    public Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}