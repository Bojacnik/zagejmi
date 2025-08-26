using Zagejmi.Server.Domain.Community.Goin;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;

namespace Zagejmi.Server.Domain.Events.People;

public sealed class PersonUpdated : IDomainEvent<Person, Guid>
{
    public Guid AggregateId { get; }
    public Guid PersonId { get; }
    public PersonType PersonType { get; }
    public PersonalInformation PersonalInformation { get; }
    public PersonalStatistics PersonalStatistics { get; }
    public List<GoinWallet> Wallets { get; }
    public AssociateProfile? AssociateProfile { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public PersonUpdated(
        Guid aggregateId,
        Guid personId,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallets,
        AssociateProfile? associateProfile)
    {
        PersonId = personId;
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallets = wallets;
        AssociateProfile = associateProfile;
        AggregateId = aggregateId;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserUpdated;
    }
}