using System.Collections.Generic;

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

    /// <summary>
    ///     Gets or sets the total score accumulated by the user. This score is a cumulative measure of the user's
    ///     activities and interactions within the platform, reflecting their overall engagement and performance. The total
    ///     score can be influenced by various factors such as the user's level, time spent on the platform, interactions with
    ///     other users, and participation in events or activities. It serves as a key metric for tracking the user's progress
    ///     and achievements within the application, and may be used for ranking, rewards, or other gamification features to
    ///     enhance user engagement and motivation.
    /// </summary>
    public ulong TotalScore { get; set; }

    /// <summary>
    ///     Gets or sets the current level of the user. The level is a representation of the user's progress and experience
    ///     within
    ///     the platform. As users engage with the application, they can earn experience points (XP) through various activities
    ///     such as completing tasks, participating in events, or interacting with other users.
    /// </summary>
    public ulong Level { get; set; }

    /// <summary>
    ///     Gets or sets the total time spent by the user on the platform. This metric reflects the overall duration of the
    ///     user's engagement with the application, including all activities and interactions. The total time can be used to
    ///     gauge the user's level of involvement and commitment to the platform, and may be considered in conjunction with
    ///     other metrics such as total score and level to provide a comprehensive view of the user's profile and performance.
    /// </summary>
    public ulong TimeTotal { get; set; }

    public ulong TimeWatching { get; set; }

    // Chats
    public uint ChatsSent { get; set; }

    // Money
    public ulong CzkSpent { get; set; }

    public ulong GoinSpent { get; set; }

    public ulong TransactionsAmount { get; set; }

    public ulong GiftsSent { get; set; }

    /// <summary>
    ///     Gets the atomic values that define the equality of this value object. This method is used to determine whether two
    ///     instances of the <see cref="PersonalStatistics" /> class are considered equal based on their properties. The method
    ///     yields each property value in a specific order, allowing for a comprehensive comparison of all relevant attributes.
    ///     When two instances of <see cref="PersonalStatistics" /> are compared, the equality check will evaluate each of
    ///     these properties to determine if they are identical, ensuring that the value object behaves correctly in terms of
    ///     value equality rather than reference equality. This is essential for scenarios where instances of
    ///     <see cref="PersonalStatistics" /> are used in collections, comparisons, or any context where value-based equality
    ///     is important for the integrity of the application.
    /// </summary>
    /// <returns>An enumerable of object values representing the properties of the value object, used for equality comparison.</returns>
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
}