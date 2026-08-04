using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Profile.Events;

namespace Zagejmi.Application.Handlers.User;

public sealed class UserRemoveProfileImageCommandHandler : IHandlerRequest<UserRemoveProfileImageCommand, (Unit? Result, Failure? Error)>
{
    private readonly IEventStore eventStore;

    public UserRemoveProfileImageCommandHandler(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task<(Unit? Result, Failure? Error)> Handle(
        UserRemoveProfileImageCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty || request.ProfileId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("UserId and ProfileId are required.");
        }

        IReadOnlyList<IDomainEvent> history = await this.eventStore.LoadAsync(request.ProfileId, cancellationToken);
        UserProfileCreatedEvent? profileCreatedEvent = history.OfType<UserProfileCreatedEvent>().FirstOrDefault();
        if (profileCreatedEvent == null)
        {
            return new FailureArgumentInvalidValue("Profile was not found.");
        }

        if (profileCreatedEvent.UserId != request.UserId)
        {
            return new FailureArgumentInvalidValue("Profile does not belong to the current user.");
        }

        var removedEvent = new UserProfileImageRemovedEvent(request.ProfileId, request.UserId);

        await this.eventStore.AppendAsync(
            request.ProfileId,
            history.Count,
            new IDomainEvent[] { removedEvent },
            cancellationToken);

        return Unit.Default;
    }
}
