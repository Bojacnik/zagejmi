using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Services.Events;

public sealed class AssociateServiceCreatedEvent : IDomainEvent
{
    public AssociateServiceCreatedEvent(
        Guid aggregateId,
        Guid profileId,
        string name,
        string? description,
        string? category,
        decimal price,
        string? priceSuffix,
        string? imageBlobKey = null,
        string? imageContentType = null)
    {
        this.AggregateId = aggregateId;
        this.ProfileId = profileId;
        this.Name = name;
        this.Description = description;
        this.Category = category;
        this.Price = price;
        this.PriceSuffix = priceSuffix;
        this.ImageBlobKey = imageBlobKey;
        this.ImageContentType = imageContentType;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid ProfileId { get; }

    public string Name { get; }

    public string? Description { get; }

    public string? Category { get; }

    public decimal Price { get; }

    public string? PriceSuffix { get; }

    public string? ImageBlobKey { get; }

    public string? ImageContentType { get; }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion { get; init; }
}