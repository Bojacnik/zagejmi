using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record CommandGoinTransactionUpdate(GoinTransaction Old, GoinTransaction Updated);