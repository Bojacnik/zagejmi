using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Profile;

namespace Zagejmi.Write.Application.Handlers.User;

public sealed class UserCreateProfileCommandHandler : IHandlerRequest<UserCreateProfileCommand, Either<Failure, Guid>>
{
    private const int MaxImageBytes = 2 * 1024 * 1024;

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

        if (!TryParsePersonType(request.ProfileType, out PersonType personType))
        {
            return new FailureArgumentInvalidValue("Unsupported profile type.");
        }

        string? blobKey = null;
        if (!string.IsNullOrWhiteSpace(request.ProfileImageDataUrl))
        {
            Either<Failure, (byte[] Bytes, string ContentType)> parsedImage =
                ParseAndValidateImageDataUrl(request.ProfileImageDataUrl);
            if (parsedImage.IsLeft)
            {
                return parsedImage.LeftAsEnumerable().First();
            }

            (byte[] bytes, string contentType) image = parsedImage.RightAsEnumerable().First();
            blobKey = await this.profileImageStorage.SaveProfileImageAsync(
                image.bytes,
                image.contentType,
                cancellationToken);
        }

        Profile profile = Profile.Create(
            request.UserId,
            personType,
            request.DisplayName.Trim(),
            blobKey);

        await this.eventStore.AppendAsync(
            profile.Id,
            0,
            profile.DomainEvents,
            cancellationToken);

        return profile.Id;
    }

    private static bool TryParsePersonType(string value, out PersonType personType)
    {
        if (string.Equals(value, "anon", StringComparison.OrdinalIgnoreCase))
        {
            personType = PersonType.Anonymous;
            return true;
        }

        return Enum.TryParse(value, true, out personType);
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