using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record GoinTransactionUpdateCommand(GoinTransaction Old, GoinTransaction Updated);