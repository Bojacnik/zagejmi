using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Repository;

public interface IGoinTransactionRepository : IDefaultRepository<GoinTransaction, IGoinTransactionEvent>
{
    public Task<List<GoinTransaction>> GetBySender(Person sender);
    
    public Task<List<GoinTransaction>> GetByReceiver(Person receiver);
}