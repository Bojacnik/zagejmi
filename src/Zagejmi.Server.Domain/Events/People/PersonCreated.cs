using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;

namespace Zagejmi.Server.Domain.Events.People;

public sealed class PersonCreated : IDomainEvent<Person, Guid>
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

    public PersonCreated(
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
        EventType = EventTypeDomain.PersonCreated;
    }

    public Person Apply(Person aggregate)
    {
        // The creation event doesn't modify the aggregate state here
        // as the state is set in the aggregate's constructor.
        return aggregate;
    }
}