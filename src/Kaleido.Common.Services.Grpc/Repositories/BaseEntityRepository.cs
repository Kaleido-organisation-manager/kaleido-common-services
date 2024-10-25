using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Repositories;

public class BaseEntityRepository : BaseEntityRepository<BaseEntity>, IBaseEntityRepository
{
    public BaseEntityRepository(DbSet<BaseEntity> dbSet, KaleidoDbContext<BaseEntity> context) : base(dbSet, context)
    {
    }
}

public class BaseEntityRepository<TEntity> : IBaseEntityRepository<TEntity>
where TEntity : BaseEntity, new()
{
    protected readonly DbSet<TEntity> DbSet;
    protected readonly KaleidoDbContext<TEntity> Context;

    public BaseEntityRepository(
        DbSet<TEntity> dbSet,
        KaleidoDbContext<TEntity> context
    )
    {
        DbSet = dbSet;
        Context = context;
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity can not be null");
        }
        entity.Id = Guid.NewGuid();
        var storedEntity = await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return storedEntity.Entity;
    }

    public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}