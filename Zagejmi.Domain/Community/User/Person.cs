using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Community.User;

public class Person(
    PersonalInfo PersonalInfo,
    PersonalStatistics PersonalStatistics,
    GoinWallet Wallet,
    PersonType PersonType,
    ulong id) : Entity<IPersonEvent>
{
    public override ulong Id { get; } = id;
    protected override ulong Version { get; set; }
}