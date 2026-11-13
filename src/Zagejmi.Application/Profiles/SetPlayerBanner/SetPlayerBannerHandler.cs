using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Images;
using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.SetPlayerBanner;

/// <summary>
///     Stores a new banner and assigns it to the user's player profile.
/// </summary>
public sealed class SetPlayerBannerHandler : IHandlerRequest<SetPlayerBannerCommand, Result<Guid>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly ImageUploader imageUploader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetPlayerBannerHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="imageUploader">Processes and stores images.</param>
    public SetPlayerBannerHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        ImageUploader imageUploader)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.imageUploader = imageUploader;
    }

    /// <summary>
    ///     Uploads the banner and assigns it to the player profile.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the stored image, or a failure.</returns>
    public async Task<Result<Guid>> Handle(SetPlayerBannerCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode)
        {
            return Result<Guid>.Failure(ProfileErrors.GhostMode);
        }

        PlayerProfile? player = await this.profileRepository.GetPlayerByUserIdAsync(user.Id, cancellationToken);
        if (player is null)
        {
            return Result<Guid>.Failure(ProfileErrors.PlayerMissing(user.GrammaticalGender));
        }

        Result<Guid> image = await this.imageUploader.UploadAsync(
            request.Content,
            ImageLimits.Banner,
            $"profiles/{player.Id}",
            request.Crop,
            cancellationToken);

        if (image.IsSuccess)
        {
            player.BannerPicture = image.Value;
            await this.profileRepository.UpdatePlayerAsync(player, cancellationToken);
        }

        return image;
    }
}
