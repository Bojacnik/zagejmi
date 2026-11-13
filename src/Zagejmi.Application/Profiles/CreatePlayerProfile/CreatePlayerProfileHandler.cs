using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.CreatePlayerProfile;

/// <summary>
///     Creates the player profile of a user. A user can own at most one player profile.
/// </summary>
public sealed class CreatePlayerProfileHandler : IHandlerRequest<CreatePlayerProfileCommand, Result<Guid>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreatePlayerProfileHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    public CreatePlayerProfileHandler(IUserRepository userRepository, IProfileRepository profileRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(
        CreatePlayerProfileCommand request,
        CancellationToken cancellationToken = default)
    {
        string displayName = request.DisplayName.Trim();
        if (displayName.Length is < 3 or > 64)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "Profile.DisplayNameLength",
                "Přezdívka musí mít 3 až 64 znaků."));
        }

        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode)
        {
            return Result<Guid>.Failure(new ErrorDetail(
                "User.GhostMode",
                "V režimu ducha můžete web pouze prohlížet. Pro tuto akci režim ducha vypněte.",
                ErrorType.Forbidden));
        }

        if (await this.profileRepository.GetPlayerByUserIdAsync(request.UserId, cancellationToken) is not null)
        {
            string label = user.GrammaticalGender == GrammaticalGender.Feminine ? "hráčky" : "hráče";
            return Result<Guid>.Failure(ErrorDetail.Conflict(
                "Profile.PlayerAlreadyExists",
                $"Profil {label} už máte."));
        }

        if (await this.profileRepository.IsDisplayNameTakenAsync(displayName, cancellationToken))
        {
            return Result<Guid>.Failure(ErrorDetail.Conflict(
                "Profile.DisplayNameTaken",
                "Tato přezdívka je už obsazená."));
        }

        PlayerProfile profile = new(Guid.CreateVersion7(), request.UserId, displayName)
        {
            Description = request.Description?.Trim(),
        };

        await this.profileRepository.AddPlayerAsync(profile, cancellationToken);
        return profile.Id;
    }
}
