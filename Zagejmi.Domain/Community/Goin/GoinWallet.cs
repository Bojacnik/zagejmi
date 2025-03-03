namespace Zagejmi.Domain.Community.Goin;

public class GoinWallet(List<GoinTransaction> transactions)
{
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

    public GoinWallet() : this([])
    {
    }
}