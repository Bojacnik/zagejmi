using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.People.Associate;
using Zagejmi.Domain.Entity;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Domain.Community.People.Person;

public sealed class Person : AggregateRoot<Person, Guid>
{
    #region Person Properties

    public User User { get; set; }
    public PersonType PersonType { get; set; }
    public PersonalInformation PersonalInformation { get; set; }
    public PersonalStatistics PersonalStatistics { get; set; }
    public List<GoinWallet> Wallets { get; set; }
    public AssociateProfile? AssociateProfile { get; set; }

    #endregion

    /// <summary>
    /// Private constructor for reconstituting the aggregate from an event stream.
    /// </summary>
    private Person(
        Guid id,
        User user,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallets, AssociateProfile? associateProfile) : base(id)
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
        evt.Apply(this);
    }
}