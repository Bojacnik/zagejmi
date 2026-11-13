using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.DeletePlayerService;

/// <summary>
///     Deletes a service of the user's player profile.
/// </summary>
public sealed class DeletePlayerServiceHandler : IHandlerRequest<DeletePlayerServiceCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletePlayerServiceHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    public DeletePlayerServiceHandler(IUserRepository userRepository, IProfileRepository profileRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
    }

    /// <summary>
    ///     Deletes the service.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success, or a failure.</returns>
    public async Task<Result> Handle(DeletePlayerServiceCommand request, CancellationToken cancellationToken = default)
    {
        Result<PlayerService> owned = await PlayerServiceOwnership.GetOwnedAsync(
            this.userRepository,
            this.profileRepository,
            request.UserId,
            request.ServiceId,
            cancellationToken);

        if (owned.IsFailure)
        {
            return Result.Failure(owned.Error!);
        }

        await this.profileRepository.DeletePlayerServiceAsync(request.ServiceId, cancellationToken);
        return Result.Success();
    }
}
