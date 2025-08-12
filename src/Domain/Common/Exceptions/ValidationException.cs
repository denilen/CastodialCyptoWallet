using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CryptoWallet.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
[Serializable]
public class ValidationException : AppException
{
    /// <summary>
    /// Gets the validation errors
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// </summary>
    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with serialized data
    /// </summary>
    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Errors = (IDictionary<string, string[]>)info.GetValue(nameof(Errors), typeof(Dictionary<string, string[]>))!;
    }

    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Errors), Errors, typeof(Dictionary<string, string[]>));
    }
}
