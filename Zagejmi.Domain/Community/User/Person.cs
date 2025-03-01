using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Community.User;

public class Person(
    PersonalInfo personalInfo,
    PersonalStatistics personalStatistics,
    GoinWallet wallet,
    PersonType personType,
    ulong id) : Entity<IPersonEvent>
{
    public override ulong Id { get; } = id;
    public PersonalInfo PersonalInfo { get; } = personalInfo;
    public PersonalStatistics PersonalStatistics { get; } = personalStatistics;
    public GoinWallet Wallet { get; } = wallet;
    public PersonType PersonType { get; } = personType;
    protected override ulong Version { get; set; }
}