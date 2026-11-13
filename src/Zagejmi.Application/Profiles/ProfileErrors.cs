using Zagejmi.Domain.Auth;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles;

/// <summary>
///     Errors shared by the profile use cases.
/// </summary>
internal static class ProfileErrors
{
    /// <summary>
    ///     Ghosts can only browse; every change is denied until ghost mode is turned off.
    /// </summary>
    public static readonly ErrorDetail GhostMode = new(
        "User.GhostMode",
        "V režimu ducha můžete web pouze prohlížet. Pro tuto akci režim ducha vypněte.",
        ErrorType.Forbidden);

    /// <summary>
    ///     The user has no player profile yet.
    /// </summary>
    public static ErrorDetail PlayerMissing(GrammaticalGender gender)
    {
        string message = gender == GrammaticalGender.Feminine
            ? "Nejdříve si vytvořte profil hráčky."
            : "Nejdříve si vytvořte profil hráče.";

        return ErrorDetail.Validation("Player.Missing", message);
    }
}
