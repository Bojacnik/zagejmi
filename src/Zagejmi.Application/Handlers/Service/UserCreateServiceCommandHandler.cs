using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Services.Events;

namespace Zagejmi.Application.Handlers.Service;

public sealed class UserCreateServiceCommandHandler : IHandlerRequest<UserCreateServiceCommand, (Guid? Result, Failure? Error)>
{
    private const int MaxImageBytes = 3 * 1024 * 1024;
    private const string BlobKeyPrefix = "service-image";

    private static readonly Regex DataUrlRegex = new(
        "^data:(?<contentType>image\\/(png|jpeg|jpg|pjpeg));base64,(?<data>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IEventStore eventStore;
    private readonly IProfileImageStorage imageStorage;

    public UserCreateServiceCommandHandler(IEventStore eventStore, IProfileImageStorage imageStorage)
    {
        this.eventStore = eventStore;
        this.imageStorage = imageStorage;
    }

    public async Task<(Guid? Result, Failure? Error)> Handle(
        UserCreateServiceCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty || request.ProfileId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("UserId and ProfileId are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new FailureArgumentInvalidValue("Service name is required.");
        }

        string? blobKey = null;
        string? imageContentType = null;

        if (!string.IsNullOrWhiteSpace(request.ImageDataUrl))
        {
            Match match = DataUrlRegex.Match(request.ImageDataUrl.Trim());
            if (!match.Success)
            {
                return new FailureArgumentInvalidValue("Service image must be a valid PNG or JPEG data URL.");
            }

            imageContentType = match.Groups["contentType"].Value.ToLowerInvariant();
            string base64 = match.Groups["data"].Value;
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return new FailureArgumentInvalidValue("Service image payload is not valid base64.");
            }

            if (bytes.Length == 0 || bytes.Length > MaxImageBytes)
            {
                return new FailureArgumentInvalidValue("Service image must be between 1 byte and 3 MB.");
            }

            blobKey = await this.imageStorage.SaveProfileImageAsync(
                bytes,
                imageContentType,
                BlobKeyPrefix,
                cancellationToken);
        }

        Guid serviceId = Guid.CreateVersion7();
        AssociateServiceCreatedEvent createdEvent = new(
            serviceId,
            request.ProfileId,
            request.Name.Trim(),
            request.Description?.Trim(),
            request.Category?.Trim(),
            request.Price,
            request.PriceSuffix?.Trim(),
            blobKey,
            imageContentType);

        await this.eventStore.AppendAsync(serviceId, 0, new IDomainEvent[] { createdEvent }, cancellationToken);

        return serviceId;
    }
}