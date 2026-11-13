namespace Zagejmi.Application.Profiles;

/// <summary>
///     The groups of the default gender split on the Hráči and Služby pages (MARKET-002), in display order.
/// </summary>
/// <remarks>
///     Based only on the gender the user chose at registration, never guessed. Where "Jiné" and "Nechci uvádět" go is
///     a proposal (PROPOSED in the product blueprint): for now they share the last group.
/// </remarks>
public enum GenderGroup
{
    /// <summary>
    ///     Women (shown first).
    /// </summary>
    Women,

    /// <summary>
    ///     Men.
    /// </summary>
    Men,

    /// <summary>
    ///     Everyone else: "Jiné" and "Nechci uvádět".
    /// </summary>
    Others,
}
