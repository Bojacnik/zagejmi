using Zagejmi.Domain.Events.GoinTransactions;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record GoinTransactionDeleteCommand(ulong id);