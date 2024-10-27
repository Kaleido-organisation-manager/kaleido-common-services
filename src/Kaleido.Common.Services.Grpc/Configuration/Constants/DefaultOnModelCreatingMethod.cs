using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kaleido.Common.Services.Grpc.Configuration.Constants;

public static class DefaultOnModelCreatingMethod
{

    public static void ForBaseRevisionEntity<TRevision>(EntityTypeBuilder<TRevision> entity)
    where TRevision : BaseRevisionEntity
    {
        entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
        entity.Property(e => e.EntityId).IsRequired().HasColumnType("uuid");
        entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
        entity.Property(e => e.Revision).IsRequired().HasColumnType("int");
        entity.Property(e => e.Action).IsRequired().HasColumnType("varchar(8)");

        entity.HasIndex(e => e.Key);
    }

    public static void ForBaseEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
    where TEntity : BaseEntity
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever().HasColumnType("uuid");
    }
}