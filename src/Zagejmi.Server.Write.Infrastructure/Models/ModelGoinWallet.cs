using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Write.Infrastructure.Models;

[Table("GoinWallet")]
public class ModelGoinWallet
{
    [Key] public ulong Id { get; init; }
    
    public Guid DomainId { get; init; }

    [ForeignKey("Person")] public ModelPerson? Owner { get; init; }
    public ulong? OwnerId { get; init; }

    public ulong CacheBalance { get; init; }
}