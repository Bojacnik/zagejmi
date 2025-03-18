using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User.Associate;

namespace Zagejmi.Domain.Community.User;

public class Person(
    PersonalInfo personalInfo,
    PersonalStatistics personalStatistics,
    GoinWallet wallet,
    PersonType personType,
    AssociateProfile? associateProfile,
    ulong id) : Entity<IPersonEvent>
{
    public override ulong Id { get; } = id;
    public PersonalInfo PersonalInfo { get; } = personalInfo;
    public PersonalStatistics PersonalStatistics { get; } = personalStatistics;
    public GoinWallet Wallet { get; } = wallet;
    public PersonType PersonType { get; } = personType;
    
    public AssociateProfile? AssociateProfile { get; } = associateProfile;
    
    protected override ulong Version { get; set; }
}