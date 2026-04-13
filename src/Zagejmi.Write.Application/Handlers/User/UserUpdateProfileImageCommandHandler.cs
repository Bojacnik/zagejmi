using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Profile;

namespace Zagejmi.Write.Application.Handlers.User;

public sealed class
    UserUpdateProfileImageCommandHandler : IHandlerRequest<UserUpdateProfileImageCommand, Either<Failure, Unit>>
{
    private const int MaxImageBytes = 2 * 1024 * 1024;

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

        (byte[] bytes, string contentType) image = parsedImage.RightAsEnumerable().First();
        string blobKey = await this.profileImageStorage.SaveProfileImageAsync(
            image.bytes,
            image.contentType,
            cancellationToken);

        IReadOnlyList<IDomainEvent> history = await this.eventStore.LoadAsync(request.ProfileId, cancellationToken);
        Profile? profile = Profile.FromHistory(request.ProfileId, history);
        if (profile == null)
        {
            return new FailureArgumentInvalidValue("Profile was not found.");
        }

        try
        {
            profile.UpdateProfileImage(request.UserId, blobKey);
        }
        catch (InvalidOperationException ex)
        {
            return new FailureArgumentInvalidValue(ex.Message);
        }

        await this.eventStore.AppendAsync(
            profile.Id,
            history.Count,
            profile.DomainEvents,
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