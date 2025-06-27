using MediatR;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Events.GoinTransactions;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionUpdateCommand(
    GoinTransaction old,
    GoinTransaction updated
) : IRequest<GoinTransactionUpdatedEvent>;