namespace CryptoWallet.Domain.Common;

/// <summary>
/// Base class for entities with audit information (creation/modification tracking)
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <summary>
    /// Identifier of the user who created the entity
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Date and time when the entity was last modified
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; protected set; }

    /// <summary>
    /// Identifier of the user who last modified the entity
    /// </summary>
    public string? LastModifiedBy { get; protected set; }

    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id) : base(id)
    {
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the entity's audit metadata when modified
    /// </summary>
    /// <param name="modifiedBy">Identifier of the user who made the changes (optional)</param>
    public void UpdateAuditFields(string? modifiedBy = null)
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}
