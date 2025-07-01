using SharedKernel;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User.Associate;

namespace Zagejmi.Domain.Community.User;

public sealed class Person : AggregateRoot<Guid>
{
    #region Person Properties

    public PersonType PersonType { get; init; }
    public PersonalInformation PersonalInformation { get; init; }
    public PersonalStatistics PersonalStatistics { get; init; }
    public List<GoinWallet> Wallet { get; init; }

    #endregion

    #region Optional Properties

    public AssociateProfile? AssociateProfile { get; init; }

    #endregion

    public Person(
        Guid id,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        List<GoinWallet> wallet,
        AssociateProfile? associateProfile) : base(id)
    {
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallet = wallet;
        AssociateProfile = associateProfile;
    }

    protected override void Apply(IDomainEvent evt)
    {
        switch (evt.EventType)
        {
            
        }
    }
}