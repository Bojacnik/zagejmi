using System;

using Zagejmi.Domain.Profiles.Players;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Database row of a review of one completed order (profiles.ServiceReviews). See <see cref="ServiceReview" />.
/// </summary>
public sealed class ServiceReviewRecord
{
    /// <summary>Gets or sets <see cref="ServiceReview.Id" />.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.OrderId" /> (unique; the order lives in wallets.Orders).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.ServiceId" /> (no foreign key: reviews outlive a removed service).</summary>
    public Guid ServiceId { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.ServiceName" />.</summary>
    public required string ServiceName { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.PlayerProfileId" />.</summary>
    public Guid PlayerProfileId { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.AuthorProfileId" />.</summary>
    public Guid AuthorProfileId { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.Rating" />.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.Comment" />.</summary>
    public string? Comment { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.CreatedAt" />.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets <see cref="ServiceReview.UpdatedAt" />.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
