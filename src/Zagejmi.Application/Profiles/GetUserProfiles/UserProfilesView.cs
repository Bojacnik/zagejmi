using System;

using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Profiles.GetUserProfiles;

/// <summary>
///     The profiles of a user and the settings the UI needs to present them.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="GrammaticalGender">How to address the user (Hráč vs. Hráčka, Zákazník vs. Zákaznice).</param>
/// <param name="IsGhostMode">Whether the user is in ghost mode.</param>
/// <param name="DefaultProfile">The profile that is active right after sign-in.</param>
/// <param name="Customer">The customer profile; every user has one.</param>
/// <param name="Player">The player profile, if the user created one.</param>
public sealed record UserProfilesView(
    Guid UserId,
    GrammaticalGender GrammaticalGender,
    bool IsGhostMode,
    ProfileType DefaultProfile,
    ProfileSummary? Customer,
    ProfileSummary? Player);

/// <summary>
///     Minimal information about a single profile.
/// </summary>
/// <param name="ProfileId">The profile identifier.</param>
/// <param name="DisplayName">The public display name.</param>
/// <param name="CreatedAt">When the profile was created.</param>
/// <param name="ImageId">The profile picture, if any.</param>
public sealed record ProfileSummary(Guid ProfileId, string DisplayName, DateTimeOffset CreatedAt, Guid? ImageId);
