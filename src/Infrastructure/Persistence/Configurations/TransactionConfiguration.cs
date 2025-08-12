using CryptoWallet.Domain.Transactions;
using CryptoWallet.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration of the essence of Transaction
/// </summary>
public class TransactionConfiguration : BaseAuditableEntityConfiguration<Transaction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
    {
        // Setting restrictions for transaction amounts
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(28, 18);

        // Construction of restrictions for the commission
        builder.Property(t => t.Fee)
            .IsRequired()
            .HasPrecision(28, 18)
            .HasDefaultValue(0);

        // Setting restrictions for the currency of the commission
        builder.Property(t => t.FeeCurrency)
            .IsRequired()
            .HasMaxLength(10);

        // Settings of restrictions for the address of the sender
        builder.Property(t => t.FromAddress)
            .HasMaxLength(128);

        // Destroying restrictions for recipient addresses
        builder.Property(t => t.ToAddress)
            .HasMaxLength(128);

        // Setting restrictions for hash transactions
        builder.Property(t => t.TransactionHash)
            .HasMaxLength(128);

        // Hesh transaction index tuning
        builder.HasIndex(t => t.TransactionHash);

        // Setting restrictions for an external transaction identifier
        builder.Property(t => t.ExternalTransactionId)
            .HasMaxLength(128);

        // Index setup on an external transaction identifier
        builder.HasIndex(t => t.ExternalTransactionId);

        // Setting restrictions for describing
        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        // Setting restrictions for metadata
        builder.Property(t => t.Metadata)
            .HasColumnType("jsonb");

        // Settings of transfers
        builder.Property(t => t.TypeEnum)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.StatusEnum)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Setting up external keys
        builder.HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Configure of communication with a related transaction
        builder.HasOne(t => t.RelatedTransaction)
            .WithMany()
            .HasForeignKey(t => t.RelatedTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Settings of the table
        builder.ToTable("Transactions");
    }
}
