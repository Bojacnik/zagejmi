using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;

namespace Zagejmi.Server.Domain.Events.People;

public sealed class PersonUpdated : IDomainEvent<Person, Guid>
{
    public Guid PersonId { get; }
    public Guid UserId { get; }
    public PersonType PersonType { get; }
    public PersonalInformation PersonalInformation { get; }
    public PersonalStatistics PersonalStatistics { get; }
    public List<GoinWallet> Wallets { get; }
    public AssociateProfile? AssociateProfile { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public PersonUpdated(
        Guid personId,
        Guid userId,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallets,
        AssociateProfile? associateProfile)
    {
        PersonId = personId;
        UserId = userId;
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallets = wallets;
        AssociateProfile = associateProfile;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserUpdated;
    }

    public Person Apply(Person aggregate)
    {
        aggregate.UserId = UserId;
        aggregate.PersonType = PersonType;
        aggregate.PersonalInformation = PersonalInformation;
        aggregate.PersonalStatistics = PersonalStatistics;
        aggregate.Wallets = Wallets;
        aggregate.AssociateProfile = AssociateProfile;
        return aggregate;
    }
}