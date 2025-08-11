using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Server.Write.Infrastructure.Models;

[Table("GoinTransaction")]
public class ModelGoinTransaction
{
    [Key] public ulong Id { get; init; }
    
    public Guid DomainId { get; init; }

    [ForeignKey("GoinWallet")] public required ModelGoinWallet Sender { get; init; }
    public ulong SenderId { get; init; }

    [ForeignKey("GoinWallet")] public required ModelGoinWallet Receiver { get; set; }
    public ulong ReceiverId { get; init; }

    public ulong Amount { get; init; }
}