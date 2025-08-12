using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using CryptoWallet.Infrastructure.Persistence;
using CryptoWallet.Infrastructure.Persistence.Repositories;
using CryptoWallet.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependencies container
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Base registration registration
        services.Configure<DatabaseConfig>(configuration.GetSection("Database"));

        // Registration of database context
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var config = configuration.GetSection("Database").Get<DatabaseConfig>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(
                config.GetConnectionString(),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });

            if (config.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            options.UseLoggerFactory(loggerFactory);
        });

        // Registration of repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Registration of services
        services.Scan(scan => scan
            .FromAssemblyOf<ApplicationDbContext>()
            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Registration of data initializers
        services.AddScoped<BaseSeeder, CryptocurrencySeeder>();
        services.AddScoped<BaseSeeder, FiatCurrencySeeder>();

        // Registration of the database initializer
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    /// <summary>
    /// Initializes the database
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider,
                                                     CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeDatabaseAsync(cancellationToken);
    }
}
