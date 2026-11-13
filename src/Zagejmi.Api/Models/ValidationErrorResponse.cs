using System.Collections.Generic;

namespace Zagejmi.Api.Models;

/// <summary>
///     Error payload returned by the API for validation failures.
/// </summary>
public sealed class ValidationErrorResponse
{
    /// <summary>
    ///     Gets the validation error messages.
    /// </summary>
    public List<string> Errors { get; init; } = [];
}
