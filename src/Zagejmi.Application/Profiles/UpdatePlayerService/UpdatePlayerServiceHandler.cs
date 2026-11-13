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

namespace Zagejmi.Application.Profiles.UpdatePlayerService;

/// <summary>
///     Edits a service of the user's player profile.
/// </summary>
public sealed class UpdatePlayerServiceHandler : IHandlerRequest<UpdatePlayerServiceCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly ImageUploader imageUploader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdatePlayerServiceHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="imageUploader">Processes and stores images.</param>
    public UpdatePlayerServiceHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        ImageUploader imageUploader)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.imageUploader = imageUploader;
    }

    /// <summary>
    ///     Validates and saves the changes.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success, or a failure.</returns>
    public async Task<Result> Handle(UpdatePlayerServiceCommand request, CancellationToken cancellationToken = default)
    {
        Result<ServiceFields> fields = PlayerServiceRules.Validate(
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.PriceUnit);

        if (fields.IsFailure)
        {
            return Result.Failure(fields.Error!);
        }

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

        PlayerService service = owned.Value;

        if (request.NewImage is not null)
        {
            Result<Guid> image = await this.imageUploader.UploadAsync(
                request.NewImage,
                ImageLimits.Service,
                $"profiles/{service.PlayerProfileId}/services",
                request.ImageCrop,
                cancellationToken);

            if (image.IsFailure)
            {
                return Result.Failure(image.Error!);
            }

            service.ImageId = image.Value;
        }
        else if (request.RemoveImage)
        {
            service.ImageId = null;
        }

        service.Name = fields.Value.Name;
        service.Description = fields.Value.Description;
        service.Category = fields.Value.Category;
        service.Price = fields.Value.Price;
        service.PriceUnit = fields.Value.PriceUnit;

        await this.profileRepository.UpdatePlayerServiceAsync(service, cancellationToken);
        return Result.Success();
    }
}
