using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Events;

namespace Zagejmi.Application.Repository;

public interface IGoinTransactionRepository : IDefaultRepository<GoinTransaction, IGoinTransactionEvent>
{
    public Task<List<GoinTransaction>> GetBySender(Person sender);
    
    public Task<List<GoinTransaction>> GetByReceiver(Person receiver);
}