using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Configuration.Interfaces;

public interface IKaleidoDbContext<TEntity> where TEntity : BaseEntity
{
    DbSet<TEntity> Items { get; set; }
}