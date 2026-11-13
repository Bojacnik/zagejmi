using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;

namespace Zagejmi.Application.Auth.Users.DeleteUser;

/// <summary>
///     Deletes a user account.
/// </summary>
public sealed class DeleteUserHandler : IHandlerRequest<DeleteUserCommand, Guid>
{
    private readonly IUserRepository userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteUserHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public DeleteUserHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        await this.userRepository.RemoveAsync(request.Id, cancellationToken);
        return request.Id;
    }
}
