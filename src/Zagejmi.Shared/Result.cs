using System;

namespace Zagejmi.Shared;

/// <summary>
///     Represents the result of an operation, which can either be successful or failed.
/// </summary>
public class Result
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result" /> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error details, if the operation failed.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when the result is successful but contains an error, or when the result is
    ///     failed but does not contain an error.
    /// </exception>
    protected Result(bool isSuccess, ErrorDetail? error)
    {
        switch (isSuccess)
        {
            case true when error is not null:
                throw new ArgumentException("Successful result cannot contain an error.");
            case false when error is null:
                throw new ArgumentException("Failed result must contain an error.");
            default:
                this.IsSuccess = isSuccess;
                this.Error = error;
                break;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !this.IsSuccess;

    /// <summary>
    ///     Gets the error details if the operation failed; otherwise, returns null.
    /// </summary>
    public ErrorDetail? Error { get; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result" /> instance.</returns>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    ///     Creates a failed result.
    /// </summary>
    /// <param name="errorDetail">The error details.</param>
    /// <returns>A failed <see cref="Result" /> instance.</returns>
    public static Result Failure(ErrorDetail errorDetail)
    {
        return new Result(false, errorDetail);
    }

    /// <summary>
    ///     Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value associated with the successful result.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>A successful <see cref="Result{T}" /> instance.</returns>
    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, null);
    }

    /// <summary>
    ///     Creates a failed result with a value.
    /// </summary>
    /// <param name="errorDetail">The error details.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>A failed <see cref="Result{T}" /> instance.</returns>
    public static Result<T> Failure<T>(ErrorDetail errorDetail)
    {
        return new Result<T>(default, false, errorDetail);
    }
}

/// <summary>
///     Represents the result of an operation that can either be successful or failed, and contains a value of type
///     <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of the value associated with the result.</typeparam>
public sealed class Result<T> : Result
{
    internal Result(
        T? value,
        bool isSuccess,
        ErrorDetail? error)
        : base(isSuccess, error)
    {
        this.Value = value;
    }

    /// <summary>
    ///     Gets the value associated with the result if the operation was successful; otherwise, throws an
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    public T Value =>
        this.IsSuccess
            ? field!
            : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    ///     Wraps a value in a successful <see cref="Result{T}" />.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>
    ///     Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value associated with the successful result.</param>
    /// <returns>A successful <see cref="Result{T}" /> instance.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, null);
    }

    /// <summary>
    ///     Creates a failed result with a value.
    /// </summary>
    /// <param name="errorDetail">The error details.</param>
    /// <returns>A failed <see cref="Result{T}" /> instance.</returns>
    public static new Result<T> Failure(ErrorDetail errorDetail)
    {
        return new Result<T>(default, false, errorDetail);
    }
}
