using Kaleido.Common.Services.Grpc.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kaleido.Common.Services.Grpc.Models;

public class BaseRevisionEntity : BaseEntity
{
    public Guid Key { get; set; }
    public Guid EntityId { get; set; }
    public int Revision { get; set; }
    public RevisionAction Action { get; set; }
    public DateTime CreatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }

        var revision = (BaseRevisionEntity)obj;

        return revision.Key == Key && revision.EntityId == EntityId
            && revision.Revision == Revision && revision.Action == Action
            && revision.CreatedAt.Equals(CreatedAt);
    }

    public override int GetHashCode()
    {
        // return base.GetHashCode();
        return HashCode.Combine(base.GetHashCode(), Key, EntityId, Revision, Action, CreatedAt);
    }

    public BaseRevisionEntity FromRevision(BaseRevisionEntity revision)
    {
        Key = revision.Key != default ? revision.Key : Key;
        EntityId = revision.EntityId != default ? revision.EntityId : EntityId;
        Revision = revision.Revision != default ? revision.Revision : Revision;
        Action = revision.Action != default ? revision.Action : Action;
        CreatedAt = revision.CreatedAt != default ? revision.CreatedAt : CreatedAt;
        Id = revision.Id != default ? revision.Id : Id;

        return this;
    }
}
