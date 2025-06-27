using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.User;

public sealed class PersonalStatistics : Entity<IPersonEvent>
{
    #region Entity Properties

    public override ulong Id { get; }
    protected override ulong Version { get; set; }

    #endregion

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
        ulong id,
        ulong version,
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
        Id = id;
        Version = version;
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
}