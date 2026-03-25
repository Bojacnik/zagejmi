using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Zagejmi.Write.Domain.Goin;

namespace Zagejmi.Write.Infrastructure.Models;

[Table("GoinTransaction")]
public class ModelGoinTransaction
{
    [Key] public Guid Id { get; set; }

    [ForeignKey("SenderWallet")] public Guid SenderId { get; set; }

    public ModelGoinWallet Sender { get; set; }

    [ForeignKey("ReceiverWallet")] public Guid ReceiverId { get; set; }

    public ModelGoinWallet Receiver { get; set; }

    public Goin Goin { get; set; }
}