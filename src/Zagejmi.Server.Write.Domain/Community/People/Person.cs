using Zagejmi.Server.Write.Domain.Auth;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People.Associate;
using Zagejmi.Server.Write.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Community.People;

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