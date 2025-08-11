namespace Zagejmi.Server.Write.Infrastructure.Models;

public class ModelPersonalStatistics
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