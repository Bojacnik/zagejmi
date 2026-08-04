using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Profile.Events;

namespace Zagejmi.Application.Handlers.User;

public sealed class UserCreateProfileCommandHandler : IHandlerRequest<UserCreateProfileCommand, Either<Failure, Guid>>
{
    private const int MaxImageBytes = 2 * 1024 * 1024;
    private const string BlobKeyPrefix = "profile-image";

    private static readonly Regex DataUrlRegex = new(
        "^data:(?<contentType>image\\/(png|jpeg|jpg|pjpeg));base64,(?<data>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IEventStore eventStore;
    private readonly IProfileImageStorage profileImageStorage;

    public UserCreateProfileCommandHandler(IEventStore eventStore, IProfileImageStorage profileImageStorage)
    {
        this.eventStore = eventStore;
        this.profileImageStorage = profileImageStorage;
    }

    public async Task<Either<Failure, Guid>> Handle(
        UserCreateProfileCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return new FailureArgumentInvalidValue("Display name is required.");
        }

        if (!TryNormalizeProfileType(request.ProfileType, out string profileType))
        {
            return new FailureArgumentInvalidValue("Unsupported profile type.");
        }

        string? blobKey = null;
        string? imageContentType = null;
        if (!string.IsNullOrWhiteSpace(request.ProfileImageDataUrl))
        {
            Either<Failure, (byte[] Bytes, string ContentType)> parsedImage =
                ParseAndValidateImageDataUrl(request.ProfileImageDataUrl);
            if (parsedImage.IsLeft)
            {
                return parsedImage.LeftAsEnumerable().First();
            }

            (byte[] bytes, string contentType) = parsedImage.RightAsEnumerable().First();
            blobKey = await this.profileImageStorage.SaveProfileImageAsync(
                bytes,
                contentType,
                BlobKeyPrefix,
                cancellationToken);
            imageContentType = contentType;
        }

        Guid profileId = Guid.CreateVersion7();
        var createdEvent = new UserProfileCreatedEvent(
            profileId,
            request.UserId,
            request.DisplayName.Trim(),
            profileType,
            blobKey,
            imageContentType);

        await this.eventStore.AppendAsync(profileId, 0, new IDomainEvent[] { createdEvent }, cancellationToken);

        return profileId;
    }

    private static bool TryNormalizeProfileType(string value, out string normalized)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "anon":
            case "anonymous":
                normalized = "Anonymous";
                return true;
            case "customer":
                normalized = "Customer";
                return true;
            case "associate":
                normalized = "Associate";
                return true;
            case "admin":
                normalized = "Admin";
                return true;
            default:
                normalized = string.Empty;
                return false;
        }
    }

    private static Either<Failure, (byte[] Bytes, string ContentType)> ParseAndValidateImageDataUrl(string dataUrl)
    {
        Match match = DataUrlRegex.Match(dataUrl.Trim());
        if (!match.Success)
        {
            return new FailureArgumentInvalidValue("Profile picture must be a valid PNG or JPEG data URL.");
        }

        string contentType = match.Groups["contentType"].Value.ToLowerInvariant();
        string base64 = match.Groups["data"].Value;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return new FailureArgumentInvalidValue("Profile picture payload is not valid base64.");
        }

        if (bytes.Length == 0 || bytes.Length > MaxImageBytes)
        {
            return new FailureArgumentInvalidValue("Profile picture must be between 1 byte and 2 MB.");
        }

        return (bytes, contentType);
    }
}
