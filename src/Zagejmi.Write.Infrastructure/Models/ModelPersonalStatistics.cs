using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zagejmi.Write.Infrastructure.Models;

[Table("PersonalStatistics")]
public class ModelPersonalStatistics
{
    [Key]
    public ulong Id { get; set; }
    public ulong TotalScore { get; set; }
    public ulong Level { get; set; }

    public ulong TimeTotal { get; set; }
    public ulong TimeWatching { get; set; }

    public uint ChatsSent { get; set; }

    public ulong CzkSpent { get; set; }
    public ulong GoinSpent { get; set; }
    public ulong TransactionsAmount { get; set; }
    public ulong GiftsSent { get; set; }

    public ModelPersonalStatistics() {}
}
