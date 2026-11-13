using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Shared;

namespace Zagejmi.Api.Extensions;

/// <summary>
///     Maps <see cref="ErrorDetail" /> failures to HTTP responses.
/// </summary>
public static class ResultActionExtensions
{
    /// <summary>
    ///     Creates an error response whose status code matches <see cref="ErrorDetail.Type" />. The body has the shape
    ///     <c>{ "code": "...", "errors": ["..."] }</c>, which the Blazor pages already read.
    /// </summary>
    /// <param name="controller">The controller producing the response.</param>
    /// <param name="error">The error to report.</param>
    /// <returns>The error response.</returns>
    public static ObjectResult Error(this ControllerBase controller, ErrorDetail error)
    {
        int status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        return controller.StatusCode(status, new { code = error.Code, errors = new[] { error.Message } });
    }
}
