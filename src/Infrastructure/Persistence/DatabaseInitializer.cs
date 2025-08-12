using CryptoWallet.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Infrastructure.Persistence;

/// <summary>
/// Database initializer
/// </summary>
public class DatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IEnumerable<BaseSeeder> _seeders;

    public DatabaseInitializer(
        ApplicationDbContext context,
        ILogger<DatabaseInitializer> logger,
        IEnumerable<BaseSeeder> seeders)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _seeders = seeders ?? throw new ArgumentNullException(nameof(seeders));
    }

    /// <summary>
    /// Initializes the database
    /// </summary>
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Use migrations if they are
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migrations applied successfully");

            // We carry out the sowing of data
            await SeedDataAsync(cancellationToken);
            _logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    /// <summary>
    /// Performs data in the database
    /// </summary>
    private async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seeder in _seeders.OrderBy(s => s.GetType().Name))
        {
            _logger.LogInformation("Seeding {Seeder}...", seeder.GetType().Name);
            await seeder.SeedAsync(_context, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeding {Seeder} completed", seeder.GetType().Name);
        }
    }
}
