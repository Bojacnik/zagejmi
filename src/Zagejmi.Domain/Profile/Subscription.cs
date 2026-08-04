using System;
using System.Collections.Generic;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Profile;

public record Subscription : ValueObject
{
    public Guid ProfileId { get; set; }

    public DateTimeOffset StartDate { get; set; }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.ProfileId;
        yield return this.StartDate;
    }
}