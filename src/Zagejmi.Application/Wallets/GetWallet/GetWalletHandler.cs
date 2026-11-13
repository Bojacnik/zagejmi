using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Profiles;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Wallets.GetWallet;

/// <summary>
///     Reads the balance of a user's Zákazník or Hráč wallet. Allowed in ghost mode.
/// </summary>
public sealed class GetWalletHandler : IHandlerRequest<GetWalletQuery, Result<WalletView>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IWalletRepository walletRepository;
    private readonly TimeProvider timeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetWalletHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="walletRepository">The wallet repository.</param>
    /// <param name="timeProvider">The clock.</param>
    public GetWalletHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IWalletRepository walletRepository,
        TimeProvider timeProvider)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.walletRepository = walletRepository;
        this.timeProvider = timeProvider;
    }

    /// <summary>
    ///     Reads the wallet. A profile that never had a wallet simply has an empty one.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The wallet, or a failure when the profile does not exist.</returns>
    public async Task<Result<WalletView>> Handle(GetWalletQuery request, CancellationToken cancellationToken = default)
    {
        Result<Guid> profileId = await WalletOwners.ResolveAsync(
            this.userRepository,
            this.profileRepository,
            request.UserId,
            request.Profile,
            cancellationToken);

        if (profileId.IsFailure)
        {
            return Result<WalletView>.Failure(profileId.Error!);
        }

        IReadOnlyList<Account> accounts = await this.walletRepository.GetAccountsAsync(profileId.Value, cancellationToken);

        if (request.Profile == ProfileType.Player)
        {
            return new WalletView(ProfileType.Player, profileId.Value, BalanceOf(accounts, AccountKind.PlayerEarnings), 0, []);
        }

        IReadOnlyList<BonusGrant> grants = await this.walletRepository.GetActiveGrantsAsync(
            profileId.Value,
            this.timeProvider.GetUtcNow(),
            cancellationToken);

        return new WalletView(
            ProfileType.Customer,
            profileId.Value,
            BalanceOf(accounts, AccountKind.CustomerPaid),
            BalanceOf(accounts, AccountKind.CustomerBonus),
            [.. grants.Select(g => new BonusBatchView(g.Reason, g.Remaining.Amount, g.ExpiresAt))]);
    }

    private static long BalanceOf(IReadOnlyList<Account> accounts, AccountKind kind)
    {
        return accounts.FirstOrDefault(a => a.Kind == kind)?.Balance ?? 0;
    }
}
