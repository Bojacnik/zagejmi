using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Write.Application.Abstractions;

/// <summary>
///     Defines a contract for handling requests (commands or queries) in the application.
///     This interface supports the CQRS pattern by providing a generic way to handle different request types and return
///     different response types.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle (typically a command or query).</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface IHandlerRequest<in TRequest, TResponse>
{
    /// <summary>
    ///     Asynchronously handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}