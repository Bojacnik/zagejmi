using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.SetDefaultProfile;

/// <summary>
///     Sets the profile that becomes active when the user signs in.
/// </summary>
public sealed class SetDefaultProfileHandler : IHandlerRequest<SetDefaultProfileCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetDefaultProfileHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    public SetDefaultProfileHandler(IUserRepository userRepository, IProfileRepository profileRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
    }

    /// <summary>
    ///     Stores the default profile. The user must own a profile of that type and must not be in ghost mode.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success, or a failure describing why the default could not be set.</returns>
    public async Task<Result> Handle(SetDefaultProfileCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode)
        {
            return Result.Failure(new ErrorDetail(
                "User.GhostMode",
                "V režimu ducha můžete web pouze prohlížet. Pro tuto akci režim ducha vypněte.",
                ErrorType.Forbidden));
        }

        if (request.Profile == ProfileType.Player
            && await this.profileRepository.GetPlayerByUserIdAsync(user.Id, cancellationToken) is null)
        {
            return Result.Failure(ErrorDetail.Validation(
                "User.DefaultProfileMissing",
                user.GrammaticalGender == GrammaticalGender.Feminine
                    ? "Nejdříve si vytvořte profil hráčky."
                    : "Nejdříve si vytvořte profil hráče."));
        }

        if (user.DefaultProfile != request.Profile)
        {
            user.DefaultProfile = request.Profile;
            await this.userRepository.UpdateAsync(user, cancellationToken);
        }

        return Result.Success();
    }
}
