using System;

namespace Zagejmi.Domain.Profile;

public record ServiceReview
{
    public string? Comment { get; set; }

    public Guid? ProfileId { get; set; }

    public DateTimeOffset Date { get; set; }

    public decimal Rating { get; set; }
}