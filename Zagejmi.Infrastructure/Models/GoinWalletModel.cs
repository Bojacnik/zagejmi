using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Infrastructure.Models;

[Table("GoinWallet")]
public class GoinWalletModel
{
    [Key] public ulong Id { get; init; }

    [ForeignKey("Person")] public Person? Owner { get; init; }
    public ulong? OwnerId { get; init; }

    public ulong CacheBalance { get; init; }
}