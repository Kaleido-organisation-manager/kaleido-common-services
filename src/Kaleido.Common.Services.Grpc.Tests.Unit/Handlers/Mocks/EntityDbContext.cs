using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

public class EntityDbContext : DbContext
{
    public DbSet<BaseEntity> Entities { get; set; }
    public DbSet<BaseRevisionEntity> Revisions { get; set; }

    public EntityDbContext(DbContextOptions<EntityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseEntity>(entity =>
         {
             entity.HasKey(e => e.Id);
             entity.Property(e => e.Id).ValueGeneratedNever().HasColumnType("uuid");
         });

        modelBuilder.Entity<BaseRevisionEntity>(entity =>
        {
            entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
            entity.Property(e => e.EntityId).IsRequired().HasColumnType("uuid");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.Revision).IsRequired().HasColumnType("int");
            entity.Property(e => e.Action).IsRequired().HasColumnType("varchar(8)");

            entity.HasIndex(e => e.Key);
        });
    }
}