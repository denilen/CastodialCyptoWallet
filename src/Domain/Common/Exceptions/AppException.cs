using System.Globalization;
using System.Runtime.Serialization;

namespace CryptoWallet.Domain.Common.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions
/// </summary>
[Serializable]
public class AppException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets additional data associated with this exception
    /// </summary>
    public IDictionary<string, object>? DataDictionary { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class
    /// </summary>
    public AppException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public AppException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class with a specified error code and message
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The message that describes the error</param>
    public AppException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class with a specified error code, message and additional data
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="data">Additional data to include with the exception</param>
    public AppException(string errorCode, string message, IDictionary<string, object>? data) : base(message)
    {
        ErrorCode = errorCode;
        DataDictionary = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class with serialized data
    /// </summary>
    protected AppException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? string.Empty;
    }

    /// <summary>
    /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode, typeof(string));
    }

    /// <summary>
    /// Creates and returns a string representation of the current exception
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrEmpty(ErrorCode)
            ? base.ToString()
            : string.Format(CultureInfo.InvariantCulture, "{0} (Code: {1})", base.ToString(), ErrorCode);
    }
}
