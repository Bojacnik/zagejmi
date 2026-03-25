namespace Zagejmi.Write.Domain.Goin;

public sealed class GoinWallet : Entity<Guid>
{
    public GoinWallet(Guid id, List<GoinTransaction> transactions) : base(id)
    {
        Id = id;
        this.Transactions = transactions;
    }

    #region Wallet Properties

    /// <summary>
    ///     CACHED AMOUNT ONLY!!!
    ///     IF YOU NEED TO MAKE A TRANSACTION
    ///     YOU HAVE TO EVALUATE EVERYTHING FROM THE TRANSACTION LIST
    /// </summary>
    public ulong CacheBalance;

    /// <summary>
    ///     TRANSACTION LIST containing only transactions of user owning this wallet
    /// </summary>
    public List<GoinTransaction> Transactions;

    #endregion
}