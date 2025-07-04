﻿using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinWallet : Entity<ulong>
{
    #region Wallet Properties

    /// <summary>
    /// CACHED AMOUNT ONLY!!!
    /// IF YOU NEED TO MAKE A TRANSACTION
    /// YOU HAVE TO EVALUATE EVERYTHING FROM THE TRANSACTION LIST
    /// </summary>
    public ulong CacheBalance;

    /// <summary>
    /// TRANSACTION LIST containing only transactions of user owning this wallet
    /// </summary>
    public List<GoinTransaction> Transactions;
    
    #endregion

    public GoinWallet(ulong id, List<GoinTransaction> transactions) : base(id)
    {
        Id = id;
        Transactions = transactions;
    }
}