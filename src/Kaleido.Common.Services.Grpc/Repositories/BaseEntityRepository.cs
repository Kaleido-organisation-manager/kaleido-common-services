using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Repositories;

public class BaseEntityRepository<EntityContext> : BaseEntityRepository<BaseEntity, EntityContext>, IBaseEntityRepository
where EntityContext : DbContext, IKaleidoDbContext<BaseEntity>
{
    public BaseEntityRepository(DbSet<BaseEntity> dbSet, EntityContext context) : base(dbSet, context)
    {
    }
}

public class BaseEntityRepository<TEntity, EntityContext> : IBaseEntityRepository<TEntity>
where TEntity : BaseEntity
where EntityContext : DbContext, IKaleidoDbContext<TEntity>
{
    protected readonly DbSet<TEntity> DbSet;
    protected readonly EntityContext Context;

    public BaseEntityRepository(
        DbSet<TEntity> dbSet,
        EntityContext context
    )
    {
        DbSet = dbSet;
        Context = context;
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
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

    public virtual async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}