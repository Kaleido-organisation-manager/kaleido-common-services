using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Builders;

public class BaseRevisionBuilder : BaseRevisionBuilder<BaseRevisionEntity> { }

public class BaseRevisionBuilder<T>
where T : BaseRevisionEntity, new()
{
    private readonly T _instance = new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Status = RevisionStatus.Active
    };

    public virtual BaseRevisionBuilder<T> FromRevision(T? revision)
    {
        if (revision != null)
        {
            _instance.FromRevision(revision);
        }

        return this;
    }

    public BaseRevisionBuilder<T> WithKey(Guid key)
    {
        if (key == Guid.Empty)
        {
            throw new ArgumentException("Provided GUID for revision can not be an Empty GUID");
        }
        _instance.Key = key;

        return this;
    }

    public BaseRevisionBuilder<T> WithAction(RevisionAction action)
    {
        _instance.Action = action;
        return this;
    }

    public BaseRevisionBuilder<T> WithRevision(int revision)
    {
        _instance.Revision = revision;
        return this;
    }

    public BaseRevisionBuilder<T> WithEntityId(Guid entityId)
    {
        _instance.EntityId = entityId;
        return this;
    }

    public BaseRevisionBuilder<T> WithCreatedAt(DateTime createdAt)
    {
        _instance.CreatedAt = createdAt;
        return this;
    }

    public T Build()
    {
        return _instance;
    }
}