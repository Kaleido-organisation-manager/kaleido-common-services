using Kaleido.Common.Services.Grpc.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kaleido.Common.Services.Grpc.Models;

public class BaseEntity
{
    public Guid Id { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var entity = (BaseEntity)obj;
        return Id == entity.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public virtual void OnModelCreating(EntityTypeBuilder<BaseRevisionEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever().HasColumnType("uuid");
    }
}