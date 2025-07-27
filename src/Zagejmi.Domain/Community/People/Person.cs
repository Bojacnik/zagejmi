using SharedKernel;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.People.Associate;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Domain.Community.People;

public sealed record Person : AggregateRoot<Person, Guid>
{
    #region Person Properties

    public User User { get; init; }
    public PersonType PersonType { get; set; }
    public PersonalInformation PersonalInformation { get; init; }
    public PersonalStatistics PersonalStatistics { get; init; }
    public List<GoinWallet> Wallets { get; init; }

    #endregion

    #region Optional Properties

    public AssociateProfile? AssociateProfile { get; init; }

    #endregion

    public Person(
        Guid id,
        User user,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallets,
        AssociateProfile? associateProfile) : base(id)
    {
        User = user;
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallets = wallets;
        AssociateProfile = associateProfile;
    }

    protected override void Apply(IDomainEvent<Person, Guid> evt)
    {
        switch (evt.EventType)
        {
            case EventTypeDomain.PersonCreated:
                EventPersonCreated eventPersonCreated = (EventPersonCreated)evt;
                if (eventPersonCreated.AggregateId == Id)
                {
                    throw new Exception("Person with this ID already exists");
                }

                eventPersonCreated.Apply(this);
                break;
            case EventTypeDomain.PersonUpdated:
                EventPersonUpdated eventPersonUpdated = (EventPersonUpdated)evt;
                if (eventPersonUpdated.AggregateId == Id)
                {
                    eventPersonUpdated.Apply(this);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}