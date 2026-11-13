using System;

namespace Zagejmi.Domain.Games;

/// <summary>
///     How a player plays a game within a service. A service can offer one or both.
/// </summary>
[Flags]
public enum PlayStyles
{
    /// <summary>
    ///     Not set (only for services that are not tied to a game).
    /// </summary>
    None = 0,

    /// <summary>
    ///     Competitive play (ranked, tryhard).
    /// </summary>
    Competitive = 1,

    /// <summary>
    ///     Casual play (just for fun).
    /// </summary>
    Casual = 2,

    /// <summary>
    ///     Both competitive and casual play.
    /// </summary>
    Both = Competitive | Casual,
}
