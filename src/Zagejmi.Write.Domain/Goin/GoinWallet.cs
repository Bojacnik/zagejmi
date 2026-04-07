using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Goin;

public sealed class GoinWallet : Entity
{
    public GoinWallet(Guid id, List<GoinTransaction> transactions)
        : base(id)
    {
        this.Transactions = transactions;
    }

    /// <summary>
    ///     Gets TRANSACTION LIST containing only transactions of user owning this wallet.
    /// </summary>
    private List<GoinTransaction> Transactions { get; init; }

    /// <summary>
    ///     Gets CACHED AMOUNT ONLY!!!
    ///     IF YOU NEED TO MAKE A TRANSACTION
    ///     YOU HAVE TO EVALUATE EVERYTHING FROM THE TRANSACTION LIST.
    /// </summary>
    public ulong CacheBalance { get; private set; }
}