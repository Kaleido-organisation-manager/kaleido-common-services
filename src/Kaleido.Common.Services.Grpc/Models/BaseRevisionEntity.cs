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

    public override void OnModelCreating(EntityTypeBuilder<BaseRevisionEntity> entity)
    {
        base.OnModelCreating(entity);

        entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
        entity.Property(e => e.EntityId).IsRequired().HasColumnType("uuid");
        entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
        entity.Property(e => e.Revision).IsRequired().HasColumnType("int");
        entity.Property(e => e.Action).IsRequired().HasColumnType("varchar(8)");

        entity.HasIndex(e => e.Key);
    }
}
