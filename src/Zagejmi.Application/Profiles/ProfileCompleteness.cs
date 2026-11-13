namespace Zagejmi.Application.Profiles;

/// <summary>
///     How much each part of a player's presentation lifts them in the Hráči and Služby order (RANK-001). The weights are
///     powers of two, so a more important part always outweighs all less important ones together:
///     profile picture &gt; banner &gt; service image.
/// </summary>
/// <remarks>
///     Keep this the single source of the weights: the planned "complete your profile" hints for players
///     (PROFILE-003) should use them to tell which missing part helps most.
/// </remarks>
public static class ProfileCompleteness
{
    /// <summary>
    ///     Points for a profile picture (the most important).
    /// </summary>
    public const int ProfilePicture = 4;

    /// <summary>
    ///     Points for a banner.
    /// </summary>
    public const int Banner = 2;

    /// <summary>
    ///     Points for a service image: on the Hráči page for having at least one available service with an image, on the
    ///     Služby page for the listed service's own image.
    /// </summary>
    public const int ServiceImage = 1;
}
