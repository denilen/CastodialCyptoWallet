using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Infrastructure.Persistence;
using CryptoWallet.Infrastructure.Persistence.Repositories;
using CryptoWallet.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        // Register DatabaseConfig
        var databaseConfig = new DatabaseConfig
        {
            Host = configuration["Database:Host"] ?? "localhost",
            Port = int.TryParse(configuration["Database:Port"], out var port) ? port : 5432,
            Database = configuration["Database:Database"] ?? "ccw",
            Username = configuration["Database:Username"] ?? "ccw",
            Password = configuration["Database:Password"] ?? "ccw",
            MaxPoolSize = int.TryParse(configuration["Database:MaxPoolSize"], out var maxPoolSize) ? maxPoolSize : 100,
            EnableSensitiveDataLogging = bool.TryParse(configuration["Database:EnableSensitiveDataLogging"], out var enableSensitiveDataLogging) && enableSensitiveDataLogging
        };
        services.AddSingleton(databaseConfig);

        // Registration of database context
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<DatabaseConfig>();
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

        // Register logger service
        services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));

        // Register services by convention (classes ending with "Service")
        var serviceTypes = typeof(ApplicationDbContext).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Service") && 
                       t.IsClass && 
                       !t.IsAbstract && 
                       t.GetInterfaces().Any())
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{serviceType.Name}");
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, serviceType);
            }
        }

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
