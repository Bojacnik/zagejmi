namespace Zagejmi.Write.Domain.Auth;

/// <summary>
/// Represents the gender of a user.
/// </summary>
public enum Gender
{
    /// <summary>
    /// The gender of the user is unknown or not specified.
    /// </summary>
    Unknown,

    /// <summary>
    /// The user identifies as male.
    /// </summary>
    Male,

    /// <summary>
    /// The user identifies as female.
    /// </summary>
    Female,

    /// <summary>
    /// The user identifies as a gender that is not listed above, or prefers not to specify their gender.
    /// </summary>
    Other,
}