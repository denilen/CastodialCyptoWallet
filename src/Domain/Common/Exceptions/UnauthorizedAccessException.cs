using System.Runtime.Serialization;

namespace CryptoWallet.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a resource they don't have permission to
/// </summary>
[Serializable]
public class UnauthorizedAccessException : AppException
{
    /// <summary>
    /// Gets the name of the resource that was being accessed
    /// </summary>
    public string? ResourceName { get; }

    /// <summary>
    /// Gets the type of permission that was required
    /// </summary>
    public string? Permission { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAccessException"/> class
    /// </summary>
    public UnauthorizedAccessException()
        : base("Access to the requested resource is not authorized.", "UNAUTHORIZED_ACCESS")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAccessException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public UnauthorizedAccessException(string message)
        : base("UNAUTHORIZED_ACCESS", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAccessException"/> class with resource name and required permission
    /// </summary>
    /// <param name="resourceName">The name of the resource that was being accessed</param>
    /// <param name="permission">The type of permission that was required</param>
    public UnauthorizedAccessException(string resourceName, string permission)
        : base("UNAUTHORIZED_ACCESS",
            $"Access to resource \"{resourceName}\" requires permission: {permission}")
    {
        ResourceName = resourceName;
        Permission = permission;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAccessException"/> class with serialized data
    /// </summary>
    protected UnauthorizedAccessException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ResourceName = info.GetString(nameof(ResourceName));
        Permission = info.GetString(nameof(Permission));
    }

    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceName), ResourceName);
        info.AddValue(nameof(Permission), Permission);
    }
}
