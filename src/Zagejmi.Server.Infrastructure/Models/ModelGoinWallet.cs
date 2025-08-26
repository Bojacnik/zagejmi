using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Infrastructure.Models;

[Table("GoinWallet")]
public class ModelGoinWallet
{
    [Key] public Guid Id { get; set; }

    [ForeignKey("Person")] public Guid PersonId { get; set; }
    public ModelPerson? Owner { get; set; }

    public ulong CacheBalance { get; set; }

    public ModelGoinWallet() {}
}
