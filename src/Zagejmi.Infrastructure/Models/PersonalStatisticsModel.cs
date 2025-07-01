using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Infrastructure.Models;

public class PersonalStatisticsModel
{
    public ulong TotalScore { get; set; }
    public ulong Level { get; set; }

    public ulong TimeTotal { get; set; }
    public ulong TimeWatching { get; set; }

    public uint ChatsSent { get; set; }

    public ulong CzkSpent { get; set; }
    public ulong GoingSpent { get; set; }
    public ulong TransactionAmount { get; set; }
    public ulong GiftsSent { get; set; }
}