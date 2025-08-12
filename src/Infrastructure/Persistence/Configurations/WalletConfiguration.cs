using CryptoWallet.Domain.Wallets;
using CryptoWallet.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration of the essence of Wallet
/// </summary>
public class WalletConfiguration : BaseAuditableEntityConfiguration<Wallet>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Wallet> builder)
    {
        // Setting restrictions for the address of the wallet
        builder.Property(w => w.Address)
            .IsRequired()
            .HasMaxLength(128);

        // Index configuration at the wallet address
        builder.HasIndex(w => w.Address)
            .IsUnique();

        // Balance restrictions setting
        builder.Property(w => w.Balance)
            .IsRequired()
            .HasPrecision(28, 18)
            .HasDefaultValue(0);

        // Setting restrictions for the flag of activity
        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Setting up external keys
        builder.HasOne(w => w.User)
            .WithMany(u => u.Wallets)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(w => w.Cryptocurrency)
            .WithMany(c => c.Wallets)
            .HasForeignKey(w => w.CryptocurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Setting up relationships with transactions
        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Settings of the table
        builder.ToTable("Wallets");
    }
}
