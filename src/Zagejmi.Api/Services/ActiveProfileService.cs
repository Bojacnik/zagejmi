using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Zagejmi.Application.Profiles.GetUserProfiles;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;
using Zagejmi.Shared;

namespace Zagejmi.Api.Services;

/// <summary>
///     Holds the signed-in user's profiles and which one is currently active, for the lifetime of a Blazor circuit.
///     After sign-in the active profile is the user's default (Zákazník unless changed on the profile page). Ghost-mode
///     users always act as customers.
/// </summary>
public sealed class ActiveProfileService
{
    private readonly GetUserProfilesHandler getUserProfilesHandler;

    private Guid? loadedUserId;
    private Task? loading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActiveProfileService" /> class.
    /// </summary>
    /// <param name="getUserProfilesHandler">Handler reading the user's profiles.</param>
    public ActiveProfileService(GetUserProfilesHandler getUserProfilesHandler)
    {
        this.getUserProfilesHandler = getUserProfilesHandler;
    }

    /// <summary>
    ///     Raised whenever the profiles or the active profile change.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    ///     Gets the last loaded profile data, or <see langword="null" /> when nothing is loaded.
    /// </summary>
    public UserProfilesView? View { get; private set; }

    /// <summary>
    ///     Gets the profile types the user owns, customer first.
    /// </summary>
    public IReadOnlyList<ProfileType> Available { get; private set; } = [];

    /// <summary>
    ///     Gets the currently active profile.
    /// </summary>
    public ProfileType Active { get; private set; } = ProfileType.Customer;

    /// <summary>
    ///     Gets the grammatical gender used for role labels.
    /// </summary>
    public GrammaticalGender GrammaticalGender => this.View?.GrammaticalGender ?? GrammaticalGender.Masculine;

    /// <summary>
    ///     Gets a value indicating whether the user can switch profiles (owns more than one and is not a ghost).
    /// </summary>
    public bool CanSwitch => this.Available.Count > 1 && this.View is { IsGhostMode: false };

    /// <summary>
    ///     Loads the user's profiles. Concurrent callers share one load, so the scoped DbContexts are never used in
    ///     parallel. A user switch, or <paramref name="force" />, reloads the data.
    /// </summary>
    /// <param name="userId">The signed-in user.</param>
    /// <param name="force">Reload even if the data for this user is already loaded.</param>
    /// <returns>A task that completes when the data is loaded.</returns>
    public async Task LoadAsync(Guid userId, bool force = false)
    {
        if (this.loading is not null)
        {
            await this.loading;
        }

        if (!force && this.loadedUserId == userId && this.View is not null)
        {
            return;
        }

        this.loading = this.LoadCoreAsync(userId);
        try
        {
            await this.loading;
        }
        finally
        {
            this.loading = null;
        }
    }

    /// <summary>
    ///     Switches the active profile. Ignored when the user does not own that profile or is in ghost mode.
    /// </summary>
    /// <param name="profile">The profile to activate.</param>
    public void SetActive(ProfileType profile)
    {
        if (this.Active == profile || !this.Available.Contains(profile))
        {
            return;
        }

        if (this.View is { IsGhostMode: true } && profile != ProfileType.Customer)
        {
            return;
        }

        this.Active = profile;
        this.Changed?.Invoke();
    }

    /// <summary>
    ///     Forgets all loaded data (on sign-out).
    /// </summary>
    public void Clear()
    {
        this.loadedUserId = null;
        this.View = null;
        this.Available = [];
        this.Active = ProfileType.Customer;
        this.Changed?.Invoke();
    }

    private async Task LoadCoreAsync(Guid userId)
    {
        Result<UserProfilesView> result = await this.getUserProfilesHandler.Handle(userId);
        if (result.IsFailure)
        {
            this.Clear();
            return;
        }

        bool isNewUser = this.loadedUserId != userId;
        UserProfilesView view = result.Value;

        List<ProfileType> available = [];
        if (view.Customer is not null)
        {
            available.Add(ProfileType.Customer);
        }

        if (view.Player is not null)
        {
            available.Add(ProfileType.Player);
        }

        this.View = view;
        this.Available = available;
        this.loadedUserId = userId;

        ProfileType active = isNewUser ? view.DefaultProfile : this.Active;
        if (!available.Contains(active) || view.IsGhostMode)
        {
            active = ProfileType.Customer;
        }

        this.Active = active;
        this.Changed?.Invoke();
    }
}
