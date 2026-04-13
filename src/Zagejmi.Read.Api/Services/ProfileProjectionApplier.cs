using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Read.Api.Ctx;
using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Services;

/// <summary>
///     Applies profile-related events to read-side projections.
/// </summary>
public sealed class ProfileProjectionApplier
{
    /// <summary>
    ///     Projector identifier used for checkpoint persistence.
    /// </summary>
    public const string ProjectorName = "user-profile-projector";

    private readonly ZagejmiReadContext readContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileProjectionApplier" /> class.
    /// </summary>
    /// <param name="readContext">Read database context.</param>
    public ProfileProjectionApplier(ZagejmiReadContext readContext)
    {
        this.readContext = readContext;
    }

    /// <summary>
    ///     Gets the last processed timestamp for this projector.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Last processed UTC timestamp, or <see langword="null" /> when no checkpoint exists.</returns>
    public async Task<DateTime?> GetCheckpointAsync(CancellationToken cancellationToken)
    {
        ProjectionCheckpoint? checkpoint = await this.readContext.ProjectionCheckpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectorName == ProjectorName, cancellationToken);

        return checkpoint?.LastProcessedAtUtc;
    }

    /// <summary>
    ///     Updates projector checkpoint to the provided timestamp.
    /// </summary>
    /// <param name="timestampUtc">Last processed timestamp in UTC.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous checkpoint update operation.
    /// </returns>
    public async Task SetCheckpointAsync(DateTime timestampUtc, CancellationToken cancellationToken)
    {
        ProjectionCheckpoint? checkpoint = await this.readContext.ProjectionCheckpoints
            .FirstOrDefaultAsync(x => x.ProjectorName == ProjectorName, cancellationToken);

        if (checkpoint == null)
        {
            this.readContext.ProjectionCheckpoints.Add(
                new ProjectionCheckpoint
                {
                    ProjectorName = ProjectorName,
                    LastProcessedAtUtc = timestampUtc
                });
        }
        else
        {
            checkpoint.LastProcessedAtUtc = timestampUtc;
        }

        await this.readContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Applies a single stored event message to profile projections.
    /// </summary>
    /// <param name="message">Stored event message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous projection apply operation.
    /// </returns>
    public async Task ApplyAsync(StoredEventMessage message, CancellationToken cancellationToken)
    {
        if (message.EventType.EndsWith("UserProfileCreatedEvent", StringComparison.Ordinal))
        {
            await this.ApplyUserProfileCreatedAsync(message, cancellationToken);
            return;
        }

        if (message.EventType.EndsWith("UserProfileImageUpdatedEvent", StringComparison.Ordinal))
        {
            await this.ApplyUserProfileImageUpdatedAsync(message, cancellationToken);
        }
    }

    private static bool TryGetGuid(JsonElement root, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        if (!TryGetPropertyIgnoreCase(root, propertyName, out JsonElement element))
        {
            return false;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => Guid.TryParse(element.GetString(), out value),
            _ => false
        };
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out JsonElement element))
        {
            return null;
        }

        return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
    }

    private static string ResolvePersonType(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "PersonType", out JsonElement element))
        {
            return "Anonymous";
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "Anonymous",
            JsonValueKind.Number when element.TryGetInt32(out int personTypeValue) => personTypeValue switch
            {
                1 => "Customer",
                2 => "Associate",
                3 => "Admin",
                _ => "Anonymous"
            },
            _ => "Anonymous"
        };
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement root, string propertyName, out JsonElement value)
    {
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private async Task ApplyUserProfileCreatedAsync(StoredEventMessage message, CancellationToken cancellationToken)
    {
        using JsonDocument doc = JsonDocument.Parse(message.Data);
        JsonElement root = doc.RootElement;

        if (!TryGetGuid(root, "UserId", out Guid userId))
        {
            return;
        }

        string displayName = TryGetString(root, "DisplayName") ?? string.Empty;
        string profileType = ResolvePersonType(root);
        string? blobKey = TryGetString(root, "ProfileImageBlobKey");

        UserProfileProjection? existing = await this.readContext.UserProfiles
            .FirstOrDefaultAsync(x => x.ProfileId == message.AggregateId, cancellationToken);

        if (existing == null)
        {
            this.readContext.UserProfiles.Add(
                new UserProfileProjection
                {
                    ProfileId = message.AggregateId,
                    UserId = userId,
                    DisplayName = displayName,
                    ProfileType = profileType,
                    ProfileImageBlobKey = blobKey,
                    UpdatedAtUtc = message.TimestampUtc
                });
        }
        else
        {
            existing.UserId = userId;
            existing.DisplayName = displayName;
            existing.ProfileType = profileType;
            existing.ProfileImageBlobKey = blobKey;
            existing.UpdatedAtUtc = message.TimestampUtc;
        }

        await this.readContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyUserProfileImageUpdatedAsync(
        StoredEventMessage message,
        CancellationToken cancellationToken)
    {
        using JsonDocument doc = JsonDocument.Parse(message.Data);
        JsonElement root = doc.RootElement;

        string? blobKey = TryGetString(root, "ProfileImageBlobKey");
        if (string.IsNullOrWhiteSpace(blobKey))
        {
            return;
        }

        UserProfileProjection? existing = await this.readContext.UserProfiles
            .FirstOrDefaultAsync(x => x.ProfileId == message.AggregateId, cancellationToken);

        if (existing == null)
        {
            return;
        }

        existing.ProfileImageBlobKey = blobKey;
        existing.UpdatedAtUtc = message.TimestampUtc;

        await this.readContext.SaveChangesAsync(cancellationToken);
    }
}