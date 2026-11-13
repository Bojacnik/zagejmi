using Zagejmi.Shared;

namespace Zagejmi.Domain.Auth;

/// <summary>
///     Business rules for deriving the <see cref="GrammaticalGender" /> of a user from their <see cref="Gender" />.
/// </summary>
public static class GrammaticalGenderRules
{
    /// <summary>
    ///     Resolves the grammatical gender: male users are addressed in masculine forms, female users in feminine forms,
    ///     and users with <see cref="Gender.Other" /> or <see cref="Gender.Unknown" /> must choose one.
    /// </summary>
    /// <param name="gender">The gender the user identifies as.</param>
    /// <param name="chosen">The grammatical gender the user picked, if any.</param>
    /// <returns>The resolved grammatical gender, or a validation failure when a required choice is missing.</returns>
    public static Result<GrammaticalGender> Resolve(Gender gender, GrammaticalGender? chosen)
    {
        return gender switch
        {
            Gender.Male => GrammaticalGender.Masculine,
            Gender.Female => GrammaticalGender.Feminine,
            _ when chosen is { } value => value,
            _ => Result<GrammaticalGender>.Failure(ErrorDetail.Validation(
                "User.GrammaticalGenderRequired",
                "Vyberte, zda vás máme oslovovat v mužském, nebo v ženském rodě.")),
        };
    }
}
