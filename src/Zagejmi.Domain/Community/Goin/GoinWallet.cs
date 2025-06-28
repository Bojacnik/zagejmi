using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinWallet(List<GoinTransaction> transactions) : Entity<IGoinTransactionEvent>
{
    #region Entity Properties

    public override ulong Id { get; }
    protected override ulong Version { get; set; }

    #endregion

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
    public List<GoinTransaction> Transactions = transactions;
    
    #endregion

    public GoinWallet(ulong id, ulong version, List<GoinTransaction> transactions) : this(transactions)
    {
        Id = id;
        Version = version;
        Transactions = transactions;
    }
}