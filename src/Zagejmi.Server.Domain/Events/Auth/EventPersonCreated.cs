using Zagejmi.Server.Domain.Community.Goin;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed record EventPersonCreated(
    Guid PersonId,
    Guid UserId,
    PersonType PersonType,
    PersonalInformation PersonalInformation,
    PersonalStatistics PersonalStatistics,
    List<GoinWallet> Wallets,
    AssociateProfile? AssociateProfile
) : IDomainEvent<Person, Guid>
{
    public Guid AggregateId => PersonId;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public EventTypeDomain EventType { get; } = EventTypeDomain.PersonCreated;
}