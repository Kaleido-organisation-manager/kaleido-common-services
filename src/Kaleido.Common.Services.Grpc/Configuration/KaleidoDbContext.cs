using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kaleido.Common.Services.Grpc.Configuration;

public class KaleidoDbContext<TEntity> : DbContext
where TEntity : class, new()
{
    private readonly IEnumerable<Action<EntityTypeBuilder<TEntity>>> _onModelCreatingMethods = Enumerable.Empty<Action<EntityTypeBuilder<TEntity>>>();

    public DbSet<TEntity> Items { get; set; }

    public KaleidoDbContext(DbContextOptions<KaleidoDbContext<TEntity>> options, IEnumerable<Action<EntityTypeBuilder<TEntity>>> onModelCreatingMethods) :
    base(options)
    {
        _onModelCreatingMethods = onModelCreatingMethods;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        if (_onModelCreatingMethods != null && _onModelCreatingMethods.Any())
        {
            foreach (var onModelCreatingMethod in _onModelCreatingMethods)
            {
                modelBuilder.Entity(onModelCreatingMethod);
            }
        }

    }
}