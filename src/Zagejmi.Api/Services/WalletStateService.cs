using System;
using System.Threading.Tasks;

using Zagejmi.Application.Wallets.GetWallet;
using Zagejmi.Domain.Profiles;
using Zagejmi.Shared;

namespace Zagejmi.Api.Services;

/// <summary>
///     Holds the balance of the active profile's wallet for the lifetime of a Blazor circuit, so the top bar and the
///     wallet page show the same numbers. Reloads whenever the profiles or the active profile change.
/// </summary>
public sealed class WalletStateService : IDisposable
{
    private readonly GetWalletHandler getWalletHandler;
    private readonly ActiveProfileService activeProfiles;

    private int version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WalletStateService" /> class.
    /// </summary>
    /// <param name="getWalletHandler">Reads wallets.</param>
    /// <param name="activeProfiles">The signed-in user's profiles.</param>
    public WalletStateService(GetWalletHandler getWalletHandler, ActiveProfileService activeProfiles)
    {
        this.getWalletHandler = getWalletHandler;
        this.activeProfiles = activeProfiles;
        this.activeProfiles.Changed += this.OnProfilesChanged;
    }

    /// <summary>
    ///     Raised whenever <see cref="Current" /> changes.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    ///     Gets the active profile's wallet, or <see langword="null" /> when nobody is signed in or it is not loaded yet.
    /// </summary>
    public WalletView? Current { get; private set; }

    /// <summary>
    ///     Reloads the active profile's wallet.
    /// </summary>
    /// <returns>A task that completes when the wallet is loaded.</returns>
    public async Task RefreshAsync()
    {
        // Profile switches can overlap; only the newest load may publish its result.
        int myVersion = ++this.version;

        WalletView? wallet = null;
        if (this.activeProfiles.View is { } view)
        {
            ProfileType active = this.activeProfiles.Active;
            Result<WalletView> result = await this.getWalletHandler.Handle(new GetWalletQuery(view.UserId, active));
            wallet = result.IsSuccess ? result.Value : null;
        }

        if (myVersion != this.version)
        {
            return;
        }

        this.Current = wallet;
        this.Changed?.Invoke();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.activeProfiles.Changed -= this.OnProfilesChanged;
    }

    private async void OnProfilesChanged()
    {
        try
        {
            await this.RefreshAsync();
        }
        catch (Exception)
        {
            // The balance is informative only; a failed load (e.g. the circuit closing) must not crash the circuit.
            this.Current = null;
        }
    }
}
