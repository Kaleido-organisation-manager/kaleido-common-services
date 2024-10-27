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
        CreatedAt = DateTime.UtcNow
    };

    public virtual BaseRevisionBuilder<T> FromRevision(T? revision)
    {
        if (revision == null)
        {
            return this;
        }

        if (revision.Key != Guid.Empty)
        {
            _instance.Key = revision.Key;
        }

        if (revision.EntityId != Guid.Empty)
        {
            _instance.EntityId = revision.EntityId;
        }
        _instance.Revision = revision.Revision;

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

    public T Build()
    {
        return _instance;
    }
}