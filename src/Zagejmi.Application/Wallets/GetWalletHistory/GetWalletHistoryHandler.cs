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

namespace Zagejmi.Application.Wallets.GetWalletHistory;

/// <summary>
///     Reads a page of a wallet's history. Allowed in ghost mode.
/// </summary>
public sealed class GetWalletHistoryHandler : IHandlerRequest<GetWalletHistoryQuery, Result<WalletHistoryPage>>
{
    /// <summary>
    ///     The largest allowed page.
    /// </summary>
    public const int MaxPageSize = 100;

    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IWalletRepository walletRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetWalletHistoryHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="walletRepository">The wallet repository.</param>
    public GetWalletHistoryHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IWalletRepository walletRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.walletRepository = walletRepository;
    }

    /// <summary>
    ///     Reads the page.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page, or a failure.</returns>
    public async Task<Result<WalletHistoryPage>> Handle(
        GetWalletHistoryQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Skip < 0 || request.Take is < 1 or > MaxPageSize)
        {
            return Result<WalletHistoryPage>.Failure(ErrorDetail.Validation(
                "Wallet.Paging",
                $"Stránka může mít 1 až {MaxPageSize} položek."));
        }

        Result<Guid> profileId = await WalletOwners.ResolveAsync(
            this.userRepository,
            this.profileRepository,
            request.UserId,
            request.Profile,
            cancellationToken);

        if (profileId.IsFailure)
        {
            return Result<WalletHistoryPage>.Failure(profileId.Error!);
        }

        // A profile owns only the accounts of its own wallet, so this never mixes the Zákazník and Hráč histories.
        IReadOnlyList<Account> accounts = await this.walletRepository.GetAccountsAsync(profileId.Value, cancellationToken);
        Guid[] accountIds = [.. accounts.Where(a => IsPartOfWallet(a.Kind, request.Profile)).Select(a => a.Id)];

        if (accountIds.Length == 0)
        {
            return new WalletHistoryPage([], false);
        }

        // One extra item tells whether another page exists.
        IReadOnlyList<WalletHistoryItem> items = await this.walletRepository.GetHistoryAsync(
            accountIds,
            request.Skip,
            request.Take + 1,
            cancellationToken);

        bool hasMore = items.Count > request.Take;
        return new WalletHistoryPage([.. items.Take(request.Take)], hasMore);
    }

    private static bool IsPartOfWallet(AccountKind kind, ProfileType profile)
    {
        return profile == ProfileType.Player
            ? kind == AccountKind.PlayerEarnings
            : kind is AccountKind.CustomerPaid or AccountKind.CustomerBonus;
    }
}

/// <summary>
///     A page of wallet history.
/// </summary>
/// <param name="Items">The transactions, newest first.</param>
/// <param name="HasMore">Whether older transactions exist.</param>
public sealed record WalletHistoryPage(IReadOnlyList<WalletHistoryItem> Items, bool HasMore);
