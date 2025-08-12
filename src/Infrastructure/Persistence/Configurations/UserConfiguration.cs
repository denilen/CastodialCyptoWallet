using CryptoWallet.Domain.Users;
using CryptoWallet.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration of the essence of user
/// </summary>
public class UserConfiguration : BaseAuditableEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        // Setting restrictions for email
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Email index tuning
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Hesh Password restrictions settings
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        // Setting restrictions for the name
        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        // Setting restrictions for surname
        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        // Setting up relationships
        builder.HasMany(u => u.Wallets)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Settings of the table
        builder.ToTable("Users");
    }
}
