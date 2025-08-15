using System.Runtime.Serialization;

namespace CryptoWallet.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
[Serializable]
public class NotFoundException : AppException
{
    /// <summary>
    /// Gets the name of the entity that was not found
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Gets the ID of the entity that was not found
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// </summary>
    public NotFoundException() : base("The requested resource was not found.", "RESOURCE_NOT_FOUND")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public NotFoundException(string message) : base("RESOURCE_NOT_FOUND", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with entity name and ID
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found</param>
    /// <param name="entityId">The ID of the entity that was not found</param>
    public NotFoundException(string entityName, object entityId)
        : base("RESOURCE_NOT_FOUND",
            $"Entity \"{entityName}\" with ID \"{entityId}\" was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with serialized data
    /// </summary>
    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        EntityName = info.GetString(nameof(EntityName));
        EntityId = info.GetValue(nameof(EntityId), typeof(object));
    }

    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EntityName), EntityName);
        info.AddValue(nameof(EntityId), EntityId, typeof(object));
    }
}
