using System;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Wallets.GetWalletHistory;

/// <summary>
///     Reads a page of a wallet's transaction history, newest first.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="Profile">Which profile's wallet (Zákazník or Hráč).</param>
/// <param name="Skip">How many transactions to skip.</param>
/// <param name="Take">The page size (1 to 100).</param>
public sealed record GetWalletHistoryQuery(Guid UserId, ProfileType Profile, int Skip = 0, int Take = 20);
