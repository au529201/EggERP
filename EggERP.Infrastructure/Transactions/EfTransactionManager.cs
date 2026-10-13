
using EggERP.Application.Transactions;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Transactions;

public class EfTransactionManager : ITransactionManager
{
    private readonly EggERPDbContext _dbContext;

    public EfTransactionManager(EggERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Func<Task> operation)
    {
        // An outer operation already owns the transaction.
        // Reuse it without committing or rolling it back here.
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await operation();
            return;
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
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
