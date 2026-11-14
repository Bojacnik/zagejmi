using System;

namespace Zagejmi.Application.Profiles;

/// <summary>
///     The public face of a customer or player profile: name and picture.
/// </summary>
/// <param name="ProfileId">The profile.</param>
/// <param name="DisplayName">The display name.</param>
/// <param name="ImageId">The profile picture, if any.</param>
public sealed record ProfileCard(Guid ProfileId, string DisplayName, Guid? ImageId)
{
    /// <summary>
    ///     Gets a placeholder for a profile that no longer exists.
    /// </summary>
    /// <param name="profileId">The missing profile.</param>
    /// <returns>The placeholder.</returns>
    public static ProfileCard Deleted(Guid profileId) => new(profileId, "Smazaný profil", null);
}
