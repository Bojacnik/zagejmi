using Zagejmi.Server.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Community.People;

public sealed record PersonalStatistics : ValueObject
{
    #region Personal Statistics Properties

    public ulong TotalScore { get; set; }
    public ulong Level { get; set; }

    // Time in seconds
    public ulong TimeTotal { get; set; }
    public ulong TimeWatching { get; set; }

    // Chats
    public uint ChatsSent { get; set; }

    // Money
    public ulong CzkSpent { get; set; }
    public ulong GoinSpent { get; set; }
    public ulong TransactionsAmount { get; set; }
    public ulong GiftsSent { get; set; }

    #endregion

    public PersonalStatistics(
        ulong totalScore,
        ulong level,
        ulong timeTotal,
        ulong timeWatching,
        uint chatsSent,
        ulong czkSpent,
        ulong goingSpent,
        ulong transactionsAmount,
        ulong giftsSent)
    {
        TotalScore = totalScore;
        Level = level;
        TimeTotal = timeTotal;
        TimeWatching = timeWatching;
        ChatsSent = chatsSent;
        CzkSpent = czkSpent;
        GoinSpent = goingSpent;
        TransactionsAmount = transactionsAmount;
        GiftsSent = giftsSent;
    }

    public PersonalStatistics() { }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return TotalScore;
        yield return Level;
        yield return TimeTotal;
        yield return TimeWatching;
        yield return ChatsSent;
        yield return CzkSpent;
        yield return GoinSpent;
        yield return TransactionsAmount;
        yield return GiftsSent;
    }
}