using MediatR;
using Zagejmi.Application.Events.GoinTransactions;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionUpdateCommand(
    GoinTransaction old,
    GoinTransaction updated
) : IRequest<GoinTransactionUpdatedEvent>;