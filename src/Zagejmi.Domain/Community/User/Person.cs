using SharedKernel;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User.Associate;

namespace Zagejmi.Domain.Community.User;

public sealed class Person(
    PersonalInfo personalInfo,
    PersonalStatistics personalStatistics,
    GoinWallet wallet,
    PersonType personType,
    AssociateProfile? associateProfile,
    ulong id) : Entity<IPersonEvent>
{
    #region Entity Properties

    public override ulong Id { get; } = id;
    protected override ulong Version { get; set; }

    #endregion

    #region Person Properties

    public PersonalInfo PersonalInfo { get; } = personalInfo;
    public PersonalStatistics PersonalStatistics { get; } = personalStatistics;
    public GoinWallet Wallet { get; } = wallet;
    public PersonType PersonType { get; } = personType;
    
    #endregion
    
    #region Optional Association Properties

    public AssociateProfile? AssociateProfile { get; } = associateProfile;

    #endregion
    
}