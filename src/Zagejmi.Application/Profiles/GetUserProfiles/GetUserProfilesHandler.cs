using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customer;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.GetUserProfiles;

/// <summary>
///     Returns the customer and player profiles of a user together with their addressing and ghost-mode settings.
/// </summary>
public sealed class GetUserProfilesHandler : IHandlerRequest<Guid, Result<UserProfilesView>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetUserProfilesHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    public GetUserProfilesHandler(IUserRepository userRepository, IProfileRepository profileRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
    }

    /// <inheritdoc />
    public async Task<Result<UserProfilesView>> Handle(Guid request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request, cancellationToken);
        if (user is null)
        {
            return Result<UserProfilesView>.Failure(ErrorDetail.NotFound(nameof(User), request));
        }

        CustomerProfile? customer = await this.profileRepository.GetCustomerByUserIdAsync(request, cancellationToken);
        PlayerProfile? player = await this.profileRepository.GetPlayerByUserIdAsync(request, cancellationToken);

        return new UserProfilesView(
            user.Id,
            user.GrammaticalGender,
            user.IsGhostMode,
            user.DefaultProfile,
            customer is null ? null : new ProfileSummary(customer.Id, customer.DisplayName, customer.CreatedAt, customer.ProfilePicture),
            player is null ? null : new ProfileSummary(player.Id, player.DisplayName, player.CreatedAt, player.CardProfilePicture));
    }
}
