using SixLabors.ImageSharp;

namespace Zagejmi.Domain.Profile;

public record AssociateService
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? PromoTag { get; set; }

    public Image ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? Category { get; set; }

    public decimal Rating { get; set; }

    public ServiceReview[] Reviews { get; set; }
}