using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models;

[Table("GoinTransaction")]
public class GoinTransactionModel
{
    [Key] public ulong Id { get; init; }

    [ForeignKey("GoinWallet")] public required GoinWalletModel Sender { get; init; }
    public ulong SenderId { get; init; }

    [ForeignKey("GoinWallet")] public required GoinWalletModel Receiver { get; set; }
    public ulong ReceiverId { get; init; }

    public ulong Amount { get; init; }
}