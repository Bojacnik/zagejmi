namespace Zagejmi.Domain.Profiles;

/// <summary>
///     The kinds of profile a user can have. Every user has a customer profile and can add a player profile.
///     Display names are gendered (Zákazník/Zákaznice, Hráč/Hráčka).
/// </summary>
public enum ProfileType
{
    /// <summary>
    ///     Customer profile (Zákazník/Zákaznice).
    /// </summary>
    Customer,

    /// <summary>
    ///     Player profile (Hráč/Hráčka), offers services.
    /// </summary>
    Player,
}
