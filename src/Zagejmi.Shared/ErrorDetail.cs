namespace Zagejmi.Shared;

/// <summary>
///     Describes why an operation failed.
/// </summary>
/// <param name="Code">A stable, machine-readable error code (e.g. <c>User.NotFound</c>).</param>
/// <param name="Message">A human-readable message that can be shown to the user.</param>
/// <param name="Type">The kind of failure, used e.g. to choose an HTTP status code.</param>
public sealed record ErrorDetail(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Validation)
{
    /// <summary>
    ///     Represents an empty error detail with no code or message.
    /// </summary>
    public static readonly ErrorDetail None =
        new(string.Empty, string.Empty);

    /// <summary>
    ///     Creates a new instance of <see cref="ErrorDetail" /> with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The created <see cref="ErrorDetail" /> instance.</returns>
    public static ErrorDetail Validation(
        string code,
        string message)
    {
        return new ErrorDetail(code, message, ErrorType.Validation);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ErrorDetail" /> representing a "not found" error for the specified entity and
    ///     ID.
    /// </summary>
    /// <param name="entity">The entity name.</param>
    /// <param name="id">The entity ID.</param>
    /// <returns>The created <see cref="ErrorDetail" /> instance.</returns>
    public static ErrorDetail NotFound(
        string entity,
        object id)
    {
        return new ErrorDetail(
            $"{entity}.NotFound",
            $"{entity} with id '{id}' was not found.",
            ErrorType.NotFound);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ErrorDetail" /> representing a "conflict" error with the specified code and
    ///     message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The created <see cref="ErrorDetail" /> instance.</returns>
    public static ErrorDetail Conflict(
        string code,
        string message)
    {
        return new ErrorDetail(code, message, ErrorType.Conflict);
    }
}

/// <summary>
///     The kind of failure described by an <see cref="ErrorDetail" />.
/// </summary>
public enum ErrorType
{
    /// <summary>
    ///     The input was invalid.
    /// </summary>
    Validation,

    /// <summary>
    ///     The requested entity does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation conflicts with the current state (e.g. a duplicate).
    /// </summary>
    Conflict,

    /// <summary>
    ///     The caller is not allowed to perform the operation.
    /// </summary>
    Forbidden,
}
