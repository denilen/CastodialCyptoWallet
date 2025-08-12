using Microsoft.EntityFrameworkCore;

namespace CryptoWallet.Infrastructure.Persistence.Seeding;

/// <summary>
/// Basic class for data initializers
/// </summary>
public abstract class BaseSeeder
{
    /// <summary>
    /// Cornses data
    /// </summary>
    /// <param name="context">The context of the database</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public abstract Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks to whether sowing is needed
    /// </summary>
    protected virtual async Task<bool> ShouldSeedAsync<TEntity>(IQueryable<TEntity> query,
                                                                CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return !await query.AnyAsync(cancellationToken);
    }
}
