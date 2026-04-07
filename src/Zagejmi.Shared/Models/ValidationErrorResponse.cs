using System.Collections.Generic;

namespace Zagejmi.Shared.Models;

/// <summary>
///     Response model for validation errors.
///     Can be used by any endpoint to return a consistent error response format.
/// </summary>
public sealed class ValidationErrorResponse
{
    /// <summary>
    ///     Gets or sets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; set; } = [];
}