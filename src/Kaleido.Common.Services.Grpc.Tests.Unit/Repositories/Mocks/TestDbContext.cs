using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
         {
             entity.HasKey(e => e.Id);
             entity.Property(e => e.Id).ValueGeneratedNever().HasColumnType("uuid");
             entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
             entity.Property(e => e.Status).IsRequired().HasColumnType("varchar(10)");
             entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
             entity.Property(e => e.Revision).IsRequired().HasColumnType("int");

             entity.HasIndex(e => e.Key);


         });
    }
}