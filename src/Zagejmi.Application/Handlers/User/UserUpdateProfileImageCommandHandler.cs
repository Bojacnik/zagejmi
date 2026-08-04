using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Profile.Events;

namespace Zagejmi.Application.Handlers.User;

public sealed class UserUpdateProfileImageCommandHandler : IHandlerRequest<UserUpdateProfileImageCommand, Either<Failure, Unit>>
{
    private const int MaxImageBytes = 2 * 1024 * 1024;
    private const string BlobKeyPrefix = "profile-image";

    private static readonly Regex DataUrlRegex = new(
        "^data:(?<contentType>image\\/(png|jpeg|jpg|pjpeg));base64,(?<data>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IEventStore eventStore;
    private readonly IProfileImageStorage profileImageStorage;

    public UserUpdateProfileImageCommandHandler(IEventStore eventStore, IProfileImageStorage profileImageStorage)
    {
        this.eventStore = eventStore;
        this.profileImageStorage = profileImageStorage;
    }

    public async Task<Either<Failure, Unit>> Handle(
        UserUpdateProfileImageCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty || request.ProfileId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("UserId and ProfileId are required.");
        }

        Either<Failure, (byte[] Bytes, string ContentType)> parsedImage =
            ParseAndValidateImageDataUrl(request.ProfileImageDataUrl);
        if (parsedImage.IsLeft)
        {
            return parsedImage.LeftAsEnumerable().First();
        }

        (byte[] bytes, string contentType) = parsedImage.RightAsEnumerable().First();

        IReadOnlyList<IDomainEvent> history = await this.eventStore.LoadAsync(request.ProfileId, cancellationToken);
        UserProfileCreatedEvent? profileCreatedEvent = history.OfType<UserProfileCreatedEvent>().FirstOrDefault();
        if (profileCreatedEvent == null)
        {
            return new FailureArgumentInvalidValue("Profile was not found.");
        }

        if (profileCreatedEvent.UserId != request.UserId)
        {
            return new FailureArgumentInvalidValue("Profile does not belong to the current user.");
        }

        string blobKey = await this.profileImageStorage.SaveProfileImageAsync(
            bytes,
            contentType,
            BlobKeyPrefix,
            cancellationToken);

        var updatedEvent = new UserProfileImageUpdatedEvent(request.ProfileId, request.UserId, blobKey, contentType);

        await this.eventStore.AppendAsync(
            request.ProfileId,
            history.Count,
            new IDomainEvent[] { updatedEvent },
            cancellationToken);

        return Unit.Default;
    }

    private static Either<Failure, (byte[] Bytes, string ContentType)> ParseAndValidateImageDataUrl(string dataUrl)
    {
        Match match = DataUrlRegex.Match((dataUrl ?? string.Empty).Trim());
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
