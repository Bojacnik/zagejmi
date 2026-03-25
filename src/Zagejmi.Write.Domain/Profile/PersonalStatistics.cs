using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Profile;

public sealed record PersonalStatistics : ValueObject
{
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
        this.TotalScore = totalScore;
        this.Level = level;
        this.TimeTotal = timeTotal;
        this.TimeWatching = timeWatching;
        this.ChatsSent = chatsSent;
        this.CzkSpent = czkSpent;
        this.GoinSpent = goingSpent;
        this.TransactionsAmount = transactionsAmount;
        this.GiftsSent = giftsSent;
    }

    public PersonalStatistics()
    {
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.TotalScore;
        yield return this.Level;
        yield return this.TimeTotal;
        yield return this.TimeWatching;
        yield return this.ChatsSent;
        yield return this.CzkSpent;
        yield return this.GoinSpent;
        yield return this.TransactionsAmount;
        yield return this.GiftsSent;
    }

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
}