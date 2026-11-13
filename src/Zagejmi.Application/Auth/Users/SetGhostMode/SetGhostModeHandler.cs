using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.SetGhostMode;

/// <summary>
///     Turns ghost mode on or off. Available to every user.
/// </summary>
public sealed class SetGhostModeHandler : IHandlerRequest<SetGhostModeCommand, Result>
{
    private readonly IUserRepository userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetGhostModeHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public SetGhostModeHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SetGhostModeCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode != request.Enabled)
        {
            user.IsGhostMode = request.Enabled;
            await this.userRepository.UpdateAsync(user, cancellationToken);
        }

        return Result.Success();
    }
}
