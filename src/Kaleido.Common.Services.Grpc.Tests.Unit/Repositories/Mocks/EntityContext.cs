
using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

public class EntityContext : DbContext, IKaleidoDbContext<BaseEntity>
{
    public DbSet<BaseEntity> Items { get; set; }

    public EntityContext(DbContextOptions<EntityContext> dbContextOptions)
    : base(dbContextOptions)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseEntity>(entity =>
        {
            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}