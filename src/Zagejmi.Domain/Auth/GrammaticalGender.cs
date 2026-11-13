namespace Zagejmi.Domain.Auth;

/// <summary>
///     Grammatical gender used when addressing the user in Czech (e.g. "Hráč" vs. "Hráčka", "Zákazník" vs.
///     "Zákaznice"). Derived from <see cref="Gender" /> for <see cref="Gender.Male" /> and <see cref="Gender.Female" />;
///     chosen by the user for <see cref="Gender.Other" /> and <see cref="Gender.Unknown" />.
/// </summary>
public enum GrammaticalGender
{
    /// <summary>
    ///     Masculine forms (mužský rod).
    /// </summary>
    Masculine,

    /// <summary>
    ///     Feminine forms (ženský rod).
    /// </summary>
    Feminine,
}
