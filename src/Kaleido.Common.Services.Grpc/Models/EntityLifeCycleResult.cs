namespace Kaleido.Common.Services.Grpc.Models;

public class EntityLifeCycleResult<TEntity> : EntityLifeCycleResult<TEntity, BaseRevisionEntity> where TEntity : BaseEntity;

public class EntityLifeCycleResult<TEntity, TRevision>
where TEntity : BaseEntity
where TRevision : BaseRevisionEntity
{
    public required TEntity Entity { get; set; }
    public required TRevision Revision { get; set; }
    public Guid Key => Revision.Key;
}