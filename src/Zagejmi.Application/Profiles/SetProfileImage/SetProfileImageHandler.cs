using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Images;
using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.SetProfileImage;

/// <summary>
///     Stores a new profile picture and assigns it to the user's customer or player profile.
/// </summary>
public sealed class SetProfileImageHandler : IHandlerRequest<SetProfileImageCommand, Result<Guid>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly ImageUploader imageUploader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetProfileImageHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="imageUploader">Processes and stores images.</param>
    public SetProfileImageHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        ImageUploader imageUploader)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.imageUploader = imageUploader;
    }

    /// <summary>
    ///     Uploads the picture and assigns it to the profile.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the stored image, or a failure.</returns>
    public async Task<Result<Guid>> Handle(SetProfileImageCommand request, CancellationToken cancellationToken = default)
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

        if (request.Profile == ProfileType.Player)
        {
            PlayerProfile? player = await this.profileRepository.GetPlayerByUserIdAsync(user.Id, cancellationToken);
            if (player is null)
            {
                return Result<Guid>.Failure(ProfileErrors.PlayerMissing(user.GrammaticalGender));
            }

            Result<Guid> image = await this.imageUploader.UploadAsync(
                request.Content,
                ImageLimits.ProfilePicture,
                $"profiles/{player.Id}",
                request.Crop,
                cancellationToken);

            if (image.IsSuccess)
            {
                player.CardProfilePicture = image.Value;
                await this.profileRepository.UpdatePlayerAsync(player, cancellationToken);
            }

            return image;
        }

        CustomerProfile? customer = await this.profileRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken);
        if (customer is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound("Customer", user.Id));
        }

        Result<Guid> customerImage = await this.imageUploader.UploadAsync(
            request.Content,
            ImageLimits.ProfilePicture,
            $"profiles/{customer.Id}",
            request.Crop,
            cancellationToken);

        if (customerImage.IsSuccess)
        {
            customer.ProfilePicture = customerImage.Value;
            await this.profileRepository.UpdateCustomerAsync(customer, cancellationToken);
        }

        return customerImage;
    }
}
