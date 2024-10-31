using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

public class EntityRevisionContext : DbContext, IKaleidoDbContext<BaseRevisionEntity>
{
    public DbSet<BaseRevisionEntity> Items { get; set; }

    public EntityRevisionContext(DbContextOptions<EntityRevisionContext> dbContextOptions)
    : base(dbContextOptions)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseRevisionEntity>(entity =>
        {
            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
        });
    }
}