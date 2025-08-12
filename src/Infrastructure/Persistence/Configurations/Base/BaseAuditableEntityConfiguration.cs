using CryptoWallet.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoWallet.Infrastructure.Persistence.Configurations.Base;

/// <summary>
/// Basic configuration for entities with an audit
/// </summary>
/// <typeparam name="TEntity">Type of essence </typeparam>
public abstract class BaseAuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key setting
        builder.HasKey(e => e.Id);

        // ID settings
        builder.HasIndex(e => e.Id);

        // Setting up audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);

        builder.Property(e => e.LastModifiedAt);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(256);

        // Calling the method for setting up specific fields for the essence of the fields
        ConfigureEntity(builder);
    }

    /// <summary>
    /// Method for setting up specific fields for the essence of fields
    /// </summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
