using System.Collections.Generic;
using System.Linq;

namespace CryptoWallet.Application.Common.Models;

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public class OperationResult<T> : OperationResult
{
    /// <summary>
    /// The value returned by the operation.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Creates a success result with a value.
    /// </summary>
    public static OperationResult<T> Success(T value)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Status = ResultStatus.Success,
            Value = value
        };
    }

    /// <summary>
    /// Creates a success result with a value and a success message.
    /// </summary>
    public static OperationResult<T> Success(T value, string message)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Status = ResultStatus.Success,
            Value = value,
            SuccessMessage = message
        };
    }

    // Other factory methods can be added as needed
}

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class OperationResult
{
    private readonly List<string> _errors = new();
    private readonly Dictionary<string, string[]> _validationErrors = new();

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; protected set; }

    /// <summary>
    /// Gets the status of the result.
    /// </summary>
    public ResultStatus Status { get; protected set; } = ResultStatus.Ok;

    /// <summary>
    /// Gets the collection of errors that occurred during the operation.
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets the collection of validation errors that occurred during the operation.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors => _validationErrors;

    /// <summary>
    /// Gets the success message, if any.
    /// </summary>
    public string? SuccessMessage { get; protected set; }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static OperationResult Success()
    {
        return new OperationResult { IsSuccess = true, Status = ResultStatus.Success };
    }

    /// <summary>
    /// Creates a success result with a message.
    /// </summary>
    public static OperationResult Success(string message)
    {
        return new OperationResult { IsSuccess = true, Status = ResultStatus.Success, SuccessMessage = message };
    }

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    public static OperationResult Failure(string error)
    {
        var result = new OperationResult { IsSuccess = false, Status = ResultStatus.Error };
        result._errors.Add(error);
        return result;
    }

    /// <summary>
    /// Creates a failure result with the specified error messages.
    /// </summary>
    public static OperationResult Failure(IEnumerable<string> errors)
    {
        var result = new OperationResult { IsSuccess = false, Status = ResultStatus.Error };
        result._errors.AddRange(errors);
        return result;
    }

    /// <summary>
    /// Creates a not found result with the specified error message.
    /// </summary>
    public static OperationResult NotFound(string error = "The requested resource was not found.")
    {
        var result = new OperationResult { IsSuccess = false, Status = ResultStatus.NotFound };
        result._errors.Add(error);
        return result;
    }

    /// <summary>
    /// Creates an unauthorized result with the specified error message.
    /// </summary>
    public static OperationResult Unauthorized(string error = "You are not authorized to access this resource.")
    {
        var result = new OperationResult { IsSuccess = false, Status = ResultStatus.Unauthorized };
        result._errors.Add(error);
        return result;
    }

    /// <summary>
    /// Creates a forbidden result with the specified error message.
    /// </summary>
    public static OperationResult Forbidden(string error = "You do not have permission to access this resource.")
    {
        var result = new OperationResult { IsSuccess = false, Status = ResultStatus.Forbidden };
        result._errors.Add(error);
        return result;
    }

    /// <summary>
    /// Creates an invalid result with the specified validation errors.
    /// </summary>
    public static OperationResult Invalid(Dictionary<string, string[]> validationErrors)
    {
        var result = new OperationResult
        {
            IsSuccess = false,
            Status = ResultStatus.Invalid
        };

        foreach (var error in validationErrors)
        {
            result._validationErrors.Add(error.Key, error.Value);
        }

        return result;
    }

    /// <summary>
    /// Creates an invalid result with a single validation error.
    /// </summary>
    public static OperationResult Invalid(string propertyName, string errorMessage)
    {
        return Invalid(new Dictionary<string, string[]>
        {
            [propertyName] = new[] { errorMessage }
        });
    }
}

/// <summary>
/// Represents the status of a result.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success = 200,

    /// <summary>
    /// The operation completed but there might be a warning.
    /// </summary>
    Ok = 200,

    /// <summary>
    /// The operation failed due to validation errors.
    /// </summary>
    Invalid = 400,

    /// <summary>
    /// The operation failed because the user is not authenticated.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// The operation failed because the user doesn't have permission.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// The operation failed due to an error.
    /// </summary>
    Error = 500
}
