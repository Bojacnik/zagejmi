using System;

using Zagejmi.Api.Models;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;

namespace Zagejmi.Api.Localization;

/// <summary>
///     Czech display texts that depend on the user's grammatical gender. Keep all gendered wording here so pages never
///     build "Hráč/Hráčka" or "Zákazník/Zákaznice" themselves (and so a future translation has a single place to start).
/// </summary>
public static class CzechLabels
{
    private static readonly TimeZoneInfo? PragueTimeZone =
        TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Prague", out TimeZoneInfo? zone) ? zone : null;

    /// <summary>
    ///     Gets the role name, e.g. "Hráč" / "Hráčka" or "Zákazník" / "Zákaznice".
    /// </summary>
    /// <param name="type">The profile type.</param>
    /// <param name="gender">The grammatical gender of the user.</param>
    /// <returns>The Czech role name.</returns>
    public static string Role(ProfileType type, GrammaticalGender gender)
    {
        bool feminine = gender == GrammaticalGender.Feminine;
        return type switch
        {
            ProfileType.Player => feminine ? "Hráčka" : "Hráč",
            _ => feminine ? "Zákaznice" : "Zákazník",
        };
    }

    /// <summary>
    ///     Gets the role name for a page or person whose gender is not known (e.g. listing headings): "Hráči" / "Zákazníci".
    /// </summary>
    /// <param name="type">The profile type.</param>
    /// <returns>The plural Czech role name.</returns>
    public static string RolePlural(ProfileType type)
    {
        return type == ProfileType.Player ? "Hráči" : "Zákazníci";
    }

    /// <summary>
    ///     Gets the Czech name of a gender option.
    /// </summary>
    /// <param name="gender">The gender.</param>
    /// <returns>The Czech label.</returns>
    public static string GenderLabel(Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Muž",
            Gender.Female => "Žena",
            Gender.Other => "Jiné",
            _ => "Nechci uvádět",
        };
    }

    /// <summary>
    ///     Gets the Czech name of a grammatical gender option.
    /// </summary>
    /// <param name="gender">The grammatical gender.</param>
    /// <returns>The Czech label.</returns>
    public static string GrammaticalGenderLabel(GrammaticalGender gender)
    {
        return gender == GrammaticalGender.Feminine
            ? "Ženský rod (např. „Hráčka“, „byla jste“)"
            : "Mužský rod (např. „Hráč“, „byl jste“)";
    }

    /// <summary>
    ///     Formats an amount of Goins with the correct Czech plural: 1 Goin, 2 Goiny, 5 Goinů, 1 250 Goinů.
    /// </summary>
    /// <param name="amount">The amount (may be negative, e.g. in a history).</param>
    /// <returns>For example „199 Goinů“.</returns>
    public static string Goins(long amount)
    {
        return $"{amount:N0} {GoinUnit(amount)}";
    }

    /// <summary>
    ///     Gets the Czech form of the word "Goin" that goes with an amount: Goin, Goiny or Goinů.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The word.</returns>
    public static string GoinUnit(long amount)
    {
        long abs = Math.Abs(amount);
        return abs == 1 ? "Goin" : abs is >= 2 and <= 4 ? "Goiny" : "Goinů";
    }

    /// <summary>
    ///     Converts a time to Czech local time (Europe/Prague), independent of the server's time zone.
    /// </summary>
    /// <param name="value">The time.</param>
    /// <returns>The same instant in Prague time (or UTC if the time zone database is missing).</returns>
    public static DateTimeOffset InPragueTime(DateTimeOffset value)
    {
        return PragueTimeZone is null ? value.ToUniversalTime() : TimeZoneInfo.ConvertTime(value, PragueTimeZone);
    }
}
