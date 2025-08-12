using CryptoWallet.Domain.Currencies;
using CryptoWallet.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration of the essence of Fiatcurrency
/// </summary>
public class FiatCurrencyConfiguration : BaseAuditableEntityConfiguration<FiatCurrency>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FiatCurrency> builder)
    {
        // Setting restrictions for currency code
        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(3);

        // Currency code setting
        builder.HasIndex(f => f.Code)
            .IsUnique();

        // Setting restrictions for the name
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Setting restrictions for a currency symbol
        builder.Property(f => f.Symbol)
            .IsRequired()
            .HasMaxLength(5);

        // Setting restrictions for the number of signs after aim
        builder.Property(f => f.DecimalPlaces)
            .IsRequired()
            .HasDefaultValue(2);

        // Setting restrictions for the flag of activity
        builder.Property(f => f.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Settings of the table
        builder.ToTable("FiatCurrencies");
    }
}
