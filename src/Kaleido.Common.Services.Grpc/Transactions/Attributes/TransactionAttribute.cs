using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kaleido.Common.Services.Grpc.Transactions.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TransactionAttribute : Attribute
{
    public async Task ExecuteInTransactionAsync(IEnumerable<DbContext> contexts, Func<Task> operation)
    {
        var strategy = contexts.First().Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await contexts.First().Database.BeginTransactionAsync();
            try
            {
                foreach (var context in contexts.Skip(1))
                {
                    await context.Database.UseTransactionAsync(transaction.GetDbTransaction());
                }

                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}