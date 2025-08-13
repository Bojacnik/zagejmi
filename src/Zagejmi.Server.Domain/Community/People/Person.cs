using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Community.People.Associate;

namespace Zagejmi.Server.Domain.Community.People;

public sealed class Person : AggregateRoot<Person, Guid>
{
    #region Person Properties

    public Guid UserId { get; set; }
    public PersonType PersonType { get; set; }
    public PersonalInformation PersonalInformation { get; set; }
    public PersonalStatistics PersonalStatistics { get; set; }
    public List<GoinWallet> Wallets { get; set; }
    public AssociateProfile? AssociateProfile { get; set; }
    public bool Deleted { get; set; }

    #endregion

    public Person(
        Guid id,
        Guid userId,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallets, AssociateProfile? associateProfile) : base(id)
    {
        UserId = userId;
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
