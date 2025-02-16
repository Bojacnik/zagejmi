using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Community.Goin;

public class GoinWallet
{
    public Person Owner;

    /// <summary>
    /// CACHED AMOUNT ONLY!!!
    /// IF YOU NEED TO MAKE A TRANSACTION
    /// YOU HAVE TO EVALUATE EVERYTHING FROM THE TRANSACTION LIST
    /// </summary>
    public ulong Balance;

    /// <summary>
    /// TRANSACTION LIST
    /// </summary>
    public List<GoinTransaction> Transactions;

    public GoinWallet(Person owner)
    {
        Owner = owner;
        Transactions = [];
    }

    public GoinWallet(Person owner, List<GoinTransaction> transactions)
    {
        Owner = owner;
        Transactions = transactions;
    }
}