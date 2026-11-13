namespace Zagejmi.Domain.Profiles;

/// <summary>
///     Represents the statistics of a user's profile, encapsulating various metrics related to their engagement and
///     performance within the platform. The ProfileStatistics class provides a structured way to store and access
///     information such as total score, level, time spent on the platform, interactions with other users, and financial
///     transactions. Each property of the class corresponds to a specific aspect of the user's profile, allowing for a
///     comprehensive overview of their activity and achievements. This class can be used in applications to track user
///     progress, inform recommendations, and enhance user engagement through gamification and rewards.
/// </summary>
public sealed record ProfileStatistics
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileStatistics" /> class with the specified parameters.
    ///     This constructor allows for the creation of a ProfileStatistics object with detailed information about a user's
    ///     profile, including their total score, level, time spent on the platform, interactions, and financial transactions.
    ///     Each parameter corresponds to a specific aspect of the user's engagement and performance within the application,
    ///     providing a comprehensive overview of their profile statistics.
    /// </summary>
    /// <param name="totalScore">
    ///     Total score accumulated by the user, reflecting their overall engagement and performance
    ///     within the platform.
    /// </param>
    /// <param name="level">Current level of the user, representing their progress and experience within the platform.</param>
    /// <param name="timeTotal">Total time spent by the user on the platform, reflecting their overall engagement.</param>
    /// <param name="timeWatching">Time spent by the user watching content on the platform.</param>
    /// <param name="chatsSent">Number of chats sent by the user.</param>
    /// <param name="czkSpent">Amount of CZK spent by the user on the platform.</param>
    /// <param name="goingSpent">Specific amount of "going" currency spent by the user on the platform.</param>
    /// <param name="transactionsAmount">Total amount of transactions made by the user.</param>
    /// <param name="giftsSent">Number of gifts sent by the user.</param>
    public ProfileStatistics(
        ulong totalScore,
        ulong level,
        ulong timeTotal,
        ulong timeWatching,
        uint chatsSent,
        ulong czkSpent,
        long goingSpent,
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileStatistics" /> class with default values.
    /// </summary>
    public ProfileStatistics()
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

    /// <summary>
    ///     Gets or sets the time spent by the user watching content on the platform. This metric specifically tracks the
    ///     duration of time the user has spent viewing videos, streams, or other media content available within the
    ///     application. It provides insight into the user's content consumption habits and preferences, and may be used to
    ///     inform recommendations, content curation, or engagement strategies. The time spent watching can also contribute to
    ///     the user's overall profile statistics, influencing their total score and level based on their level of interaction
    ///     with the platform's media offerings.
    /// </summary>
    public ulong TimeWatching { get; set; }

    /// <summary>
    ///     Gets or sets the number of chats sent by the user. This metric tracks the user's communication and interaction with
    ///     other users on the platform.
    /// </summary>
    public uint ChatsSent { get; set; }

    /// <summary>
    ///     Gets or sets the amount of CZK (Czech Koruna) spent by the user on the platform. This metric reflects the user's
    ///     financial transactions and expenditures within the application, providing insight into their spending behavior and
    ///     engagement with premium features, virtual goods, or other monetized aspects of the platform. The CZK spent can be
    ///     used to assess the user's value to the platform, inform marketing strategies, and tailor offers or promotions based
    ///     on their spending patterns. It may also contribute to the user's overall profile statistics, influencing their
    ///     total score, level, and other metrics related to their engagement and performance within the application.
    /// </summary>
    public decimal CzkSpent { get; set; }

    /// <summary>
    ///     Gets or sets the amount of "going" currency spent by the user on the platform. This metric tracks the user's
    ///     financial transactions and expenditures within the application, providing insight into their spending behavior and
    ///     engagement with premium features, virtual goods, or other monetized aspects of the platform. The "going" currency
    ///     spent can be
    ///     used to assess the user's value to the platform, inform marketing strategies, and tailor offers or promotions based
    ///     on their spending patterns. It may also contribute to the user's overall profile statistics, influencing their
    ///     total score, level, and other metrics related to their engagement and performance within the application.
    /// </summary>
    public long GoinSpent { get; set; }

    /// <summary>
    ///     Gets or sets the total amount of transactions made by the user. This metric reflects the user's overall financial
    ///     activity and engagement with the platform's monetization features.
    /// </summary>
    public ulong TransactionsAmount { get; set; }

    /// <summary>
    ///     Gets or sets the number of gifts sent by the user. This metric tracks the user's interactions and engagement with
    ///     other users on the platform, specifically in the context of sending virtual gifts or tokens of appreciation. The
    ///     number of gifts sent can provide insight into the user's social behavior, generosity, and level of participation in
    ///     community activities. It may also contribute to the user's overall profile statistics, influencing their total
    ///     score, level, and other metrics related to their engagement and performance within the application.
    /// </summary>
    public ulong GiftsSent { get; set; }
}
