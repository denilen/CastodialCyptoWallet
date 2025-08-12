using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using Microsoft.EntityFrameworkCore;

namespace CryptoWallet.Infrastructure.Persistence;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Users
    public DbSet<User> Users => Set<User>();

    // Cryptocurrencies
    public DbSet<Cryptocurrency> Cryptocurrencies => Set<Cryptocurrency>();

    // Fiat currencies
    public DbSet<FiatCurrency> FiatCurrencies => Set<FiatCurrency>();

    // Wallets
    public DbSet<Wallet> Wallets => Set<Wallet>();

    // Transactions
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // We use all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Setting up accuracy of decimal fields
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(28);
            property.SetScale(18);
        }

        // Index tuning
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Cryptocurrency>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<FiatCurrency>()
            .HasIndex(f => f.Code)
            .IsUnique();

        modelBuilder.Entity<Wallet>()
            .HasIndex(w => w.Address)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.TransactionHash)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.ExternalTransactionId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
                                               CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        var currentTime = DateTimeOffset.UtcNow;
        var currentUser = "system"; // In the real application, there will be ID of the current user

        foreach (var entry in entries)
        {
            var entity = (AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = currentTime;
                entity.CreatedBy = currentUser;
            }
            else
            {
                entity.LastModifiedAt = currentTime;
                entity.LastModifiedBy = currentUser;
            }
        }
    }
}
