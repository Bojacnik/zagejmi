namespace Zagejmi.Domain.Community.User;

public record PersonalStatistics
{
    public ulong TotalScore { get; init; }
    public ulong Level { get; init; }

    // Time in seconds
    public ulong TimeTotal { get; init; }
    public ulong TimeWatching { get; init; }

    // Chats
    public uint ChatsSent { get; init; }

    // Money
    public ulong CzkSpent { get; init; }
    public ulong GoinSpent { get; init; }
    public ulong TransactionsAmount { get; init; }
    public ulong GiftsSent { get; init; }
}