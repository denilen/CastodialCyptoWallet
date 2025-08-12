using CryptoWallet.Domain.Currencies;
using CryptoWallet.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration of the essence of Cryptocurrency
/// </summary>
public class CryptocurrencyConfiguration : BaseAuditableEntityConfiguration<Cryptocurrency>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Cryptocurrency> builder)
    {
        // Setting restrictions for currency code
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        // Currency code setting
        builder.HasIndex(c => c.Code)
            .IsUnique();

        // Setting restrictions for the name
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Setting restrictions for the number of signs after aim
        builder.Property(c => c.DecimalPlaces)
            .IsRequired()
            .HasDefaultValue(8);

        // Setting restrictions for the flag of activity
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Setting up relationships
        builder.HasMany(c => c.Wallets)
            .WithOne(w => w.Cryptocurrency)
            .HasForeignKey(w => w.CryptocurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Settings of the table
        builder.ToTable("Cryptocurrencies");
    }
}
