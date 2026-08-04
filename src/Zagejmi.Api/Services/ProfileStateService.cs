using System;
using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Api.Services;

/// <summary>
///     Holds in-memory profile selection and creation state for the UI.
/// </summary>
public sealed class ProfileStateService
{
    private static readonly ProfileType[] AllProfileTypes =
    [
        ProfileType.Customer,
        ProfileType.Associate,
        ProfileType.Anon
    ];

    private readonly List<UserProfileCard> profiles = new();

    /// <summary>
    ///     Gets the currently selected profile, if any.
    /// </summary>
    public UserProfileCard? SelectedProfile { get; private set; }

    /// <summary>
    ///     Gets current profiles.
    /// </summary>
    /// <returns>Read-only list of profiles.</returns>
    public IReadOnlyList<UserProfileCard> GetProfiles()
    {
        return this.profiles;
    }

    /// <summary>
    ///     Gets profile types that are not yet created.
    /// </summary>
    /// <returns>Missing profile types.</returns>
    public IReadOnlyList<ProfileType> GetMissingProfileTypes()
    {
        HashSet<ProfileType> existing = this.profiles.Select(p => p.Type).ToHashSet();
        return AllProfileTypes.Where(type => !existing.Contains(type)).ToList();
    }

    /// <summary>
    ///     Determines whether another profile can be created.
    /// </summary>
    /// <returns><see langword="true" /> if at least one profile type is still available.</returns>
    public bool CanCreateAnotherProfile()
    {
        return this.GetMissingProfileTypes().Count > 0;
    }

    /// <summary>
    ///     Selects an existing profile by type.
    /// </summary>
    /// <param name="type">Profile type to select.</param>
    /// <returns><see langword="true" /> when a matching profile exists.</returns>
    public bool SelectProfile(ProfileType type)
    {
        UserProfileCard? selected = this.profiles.FirstOrDefault(p => p.Type == type);
        if (selected is null)
        {
            return false;
        }

        this.SelectedProfile = selected;
        return true;
    }

    /// <summary>
    ///     Creates a new profile if type is not already present.
    /// </summary>
    /// <param name="type">Profile type.</param>
    /// <param name="displayName">Display name.</param>
    /// <param name="profileImageUrl">Optional profile image URL.</param>
    /// <returns><see langword="true" /> when profile is created; otherwise <see langword="false" />.</returns>
    public bool TryCreateProfile(ProfileType type, string displayName, string? profileImageUrl = null)
    {
        if (this.profiles.Any(p => p.Type == type))
        {
            return false;
        }

        UserProfileCard profile = new()
        {
            ProfileId = Guid.CreateVersion7(),
            Type = type,
            DisplayName = displayName,
            ProfileImageUrl = profileImageUrl,
            Stats =
            {
                Level = 1,
                Reputation = 0,
                CompletedSessions = 0
            }
        };

        this.profiles.Add(profile);
        this.SelectedProfile = profile;
        return true;
    }
}