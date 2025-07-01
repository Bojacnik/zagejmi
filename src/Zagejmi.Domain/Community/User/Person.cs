using SharedKernel;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User.Associate;

namespace Zagejmi.Domain.Community.User;

public sealed class Person : Entity<ulong>
{
    #region Person Properties

    public PersonType PersonType { get; init; }
    public PersonalInformation PersonalInformation { get; init; }
    public PersonalStatistics PersonalStatistics { get; init; }
    public GoinWallet[] Wallet { get; init; }

    #endregion

    #region Optional Properties

    public AssociateProfile? AssociateProfile { get; init; }

    #endregion

    public Person(
        ulong id,
        PersonType personType,
        PersonalInformation personalInformation,
        PersonalStatistics personalStatistics,
        GoinWallet[] wallet,
        AssociateProfile? associateProfile) : base(id)
    {
        PersonType = personType;
        PersonalInformation = personalInformation;
        PersonalStatistics = personalStatistics;
        Wallet = wallet;
        AssociateProfile = associateProfile;
    }
}