using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Infrastructure.Models;

[Table("Person")]
public class PersonModel
{
    [Key] public ulong Id { get; init; }

    public PersonType PersonType { get; init; }

    [ForeignKey("PersonalInfo")] public PersonalInformationModel? PersonalInfo { get; init; }

    public ulong? PersonalInformationId { get; init; }

    [ForeignKey("PersonStats")] public PersonStatsModel? Statistics { get; init; }

    public ulong? StatisticsId { get; init; }

    [ForeignKey("GoinWallet")] public GoinWallet? Wallet { get; init; }

    public ulong? GoinWalletId { get; init; }
}