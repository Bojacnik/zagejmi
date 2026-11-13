using System;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Wallets.GetWallet;

/// <summary>
///     Reads the wallet of one of the user's profiles.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="Profile">Which profile's wallet (Zákazník or Hráč).</param>
public sealed record GetWalletQuery(Guid UserId, ProfileType Profile);
