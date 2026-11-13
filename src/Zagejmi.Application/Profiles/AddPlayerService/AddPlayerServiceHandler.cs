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

namespace Zagejmi.Application.Profiles.AddPlayerService;

/// <summary>
///     Adds a service to the user's player profile.
/// </summary>
public sealed class AddPlayerServiceHandler : IHandlerRequest<AddPlayerServiceCommand, Result<Guid>>
{
    /// <summary>
    ///     The most services one player can offer.
    /// </summary>
    public const int MaxServicesPerPlayer = PlayerServiceRules.MaxServicesPerPlayer;

    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly ImageUploader imageUploader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddPlayerServiceHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="imageUploader">Processes and stores images.</param>
    public AddPlayerServiceHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        ImageUploader imageUploader)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.imageUploader = imageUploader;
    }

    /// <summary>
    ///     Validates and stores the service.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new service ID, or a failure.</returns>
    public async Task<Result<Guid>> Handle(AddPlayerServiceCommand request, CancellationToken cancellationToken = default)
    {
        Result<ServiceFields> fields = PlayerServiceRules.Validate(
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.PriceUnit,
            request.GameKey,
            request.RankKey,
            request.PlayStyles);

        if (fields.IsFailure)
        {
            return Result<Guid>.Failure(fields.Error!);
        }

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

        if (await this.profileRepository.CountPlayerServicesAsync(player.Id, cancellationToken) >= MaxServicesPerPlayer)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "Service.Limit",
                $"Můžete nabízet nejvýše {MaxServicesPerPlayer} služeb."));
        }

        Guid serviceId = Guid.CreateVersion7();
        Guid? imageId = null;

        if (request.Image is not null)
        {
            Result<Guid> image = await this.imageUploader.UploadAsync(
                request.Image,
                ImageLimits.Service,
                $"profiles/{player.Id}/services",
                request.ImageCrop,
                cancellationToken);

            if (image.IsFailure)
            {
                return image;
            }

            imageId = image.Value;
        }

        await this.profileRepository.AddPlayerServiceAsync(
            new PlayerService
            {
                Id = serviceId,
                PlayerProfileId = player.Id,
                Name = fields.Value.Name,
                Description = fields.Value.Description,
                Category = fields.Value.Category,
                GameKey = fields.Value.Game.GameKey,
                RankKey = fields.Value.Game.RankKey,
                PlayStyles = fields.Value.Game.PlayStyles,
                Price = fields.Value.Price,
                PriceUnit = fields.Value.PriceUnit,
                ImageId = imageId,
            },
            cancellationToken);

        return serviceId;
    }
}
