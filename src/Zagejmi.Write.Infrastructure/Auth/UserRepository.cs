using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Auth.Events;
using Zagejmi.Write.Infrastructure.Ctx;

namespace Zagejmi.Write.Infrastructure.Auth;

/// <summary>
///     Implementation of user repository using event sourcing.
///     Reconstructs user aggregates from stored events.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private static readonly Action<ILogger, Exception?> NullUsernameLogger =
        LoggerMessage.Define(
            LogLevel.Warning,
            default,
            "[UserRepository] GetByUsernameAsync called with null/empty username");

    private static readonly Action<ILogger, string, Exception?> SearchingForUserLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "[UserRepository] Searching for user by username: {Username}");

    private static readonly Action<ILogger, int, Exception?> FoundEventsLogger =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            default,
            "[UserRepository] Found {Count} UserCreatedEvent entries in database");

    private static readonly Action<ILogger, Exception?> NoEventsFoundLogger =
        LoggerMessage.Define(
            LogLevel.Warning,
            default,
            "[UserRepository] No UserCreatedEvent entries found in database");

    private static readonly Action<ILogger, string, Exception?> CouldNotResolveTypeLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[UserRepository] Could not resolve type: {EventType}");

    private static readonly Action<ILogger, string, Exception?> FoundEventForUserLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "[UserRepository] Found event for user: {Username}");

    private static readonly Action<ILogger, Guid, Exception?> MatchFoundLogger =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            default,
            "[UserRepository] Match found! AggregateId: {AggregateId}");

    private static readonly Action<ILogger, Guid, Exception?> NoEventsForAggregateLogger =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            default,
            "[UserRepository] No events found for AggregateId: {AggregateId}");

    private static readonly Action<ILogger, string, Exception?> ErrorDeserializingLogger =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            default,
            "[UserRepository] Error: {Error}");

    private static readonly Action<ILogger, string, Exception?> UserNotFoundLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[UserRepository] No user found with username: {Username}");

    private readonly IEventStore eventStore;
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ILogger<UserRepository> logger;
    private readonly ZagejmiWriteContext writeContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRepository" /> class.
    /// </summary>
    /// <param name="eventStore">The event store for retrieving user events.</param>
    /// <param name="writeContext">The database context for querying stored events.</param>
    /// <param name="logger">The logger instance.</param>
    public UserRepository(IEventStore eventStore, ZagejmiWriteContext writeContext, ILogger<UserRepository> logger)
    {
        this.eventStore = eventStore;
        this.writeContext = writeContext;
        this.logger = logger;
    }

    /// <summary>
    ///     Gets a user by username by searching through all users' events.
    ///     This is a simplified implementation - in production, use a read model instead.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            NullUsernameLogger(this.logger, null);
            return null;
        }

        SearchingForUserLogger(this.logger, username, null);

        // Query all UserCreatedEvent entries to find a user with matching username
        List<StoredEvent> storedEvents = await this.writeContext.StoredEvents
            .Where(e => e.EventType.Contains("UserCreatedEvent"))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        FoundEventsLogger(this.logger, storedEvents.Count, null);

        if (storedEvents.Count == 0)
        {
            NoEventsFoundLogger(this.logger, null);
            return null;
        }

        // Search through all events to find one with matching username
        foreach (StoredEvent storedEvent in storedEvents)
        {
            try
            {
                Type? eventType = Type.GetType(storedEvent.EventType);
                if (eventType == null)
                {
                    CouldNotResolveTypeLogger(this.logger, storedEvent.EventType, null);
                    continue;
                }

                IDomainEvent? domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(
                    storedEvent.Data,
                    eventType,
                    this.jsonSerializerOptions);

                if (domainEvent is UserCreatedEvent userCreatedEvent)
                {
                    FoundEventForUserLogger(this.logger, userCreatedEvent.Username, null);

                    if (userCreatedEvent.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                    {
                        MatchFoundLogger(this.logger, userCreatedEvent.AggregateId, null);

                        // Reconstruct the user aggregate from all its events
                        IReadOnlyList<IDomainEvent> allEvents = await this.eventStore.LoadAsync(
                            userCreatedEvent.AggregateId,
                            cancellationToken);
                        if (allEvents.Count == 0)
                        {
                            NoEventsForAggregateLogger(this.logger, userCreatedEvent.AggregateId, null);
                            continue;
                        }

                        User user = CreateUserInstance(userCreatedEvent.AggregateId);
                        user.LoadFromHistory(allEvents);

                        return user;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDeserializingLogger(this.logger, ex.Message, null);
            }
        }

        UserNotFoundLogger(this.logger, username, null);
        return null;
    }

    /// <summary>
    ///     Gets a user by email by searching through all users' events.
    ///     This is a simplified implementation - in production, use a read model instead.
    /// </summary>
    /// <param name="email">The email to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        // Query all UserCreatedEvent entries to find a user with matching email
        List<StoredEvent> storedEvents = await this.writeContext.StoredEvents
            .Where(e => e.EventType.Contains("UserCreatedEvent"))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (storedEvents.Count == 0)
        {
            return null;
        }

        // Search through all events to find one with matching email
        foreach (StoredEvent storedEvent in storedEvents)
        {
            try
            {
                Type? eventType = Type.GetType(storedEvent.EventType);
                if (eventType == null)
                {
                    continue;
                }

                IDomainEvent? domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(
                    storedEvent.Data,
                    eventType,
                    this.jsonSerializerOptions);

                if (domainEvent is UserCreatedEvent userCreatedEvent &&
                    userCreatedEvent.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    // Reconstruct the user aggregate from all its events
                    IReadOnlyList<IDomainEvent> allEvents = await this.eventStore.LoadAsync(
                        userCreatedEvent.AggregateId,
                        cancellationToken);
                    if (allEvents.Count == 0)
                    {
                        continue;
                    }

                    User user = CreateUserInstance(userCreatedEvent.AggregateId);
                    user.LoadFromHistory(allEvents);

                    return user;
                }
            }
            catch
            {
                // Skip events that cannot be deserialized
            }
        }

        return null;
    }

    /// <summary>
    ///     Creates a User instance from an aggregate ID for event reconstruction.
    /// </summary>
    private static User CreateUserInstance(Guid aggregateId)
    {
        ConstructorInfo? constructor = typeof(User).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(Guid)],
            null);

        if (constructor == null)
        {
            throw new InvalidOperationException("Unable to find User constructor for event reconstruction");
        }

        return (User)constructor.Invoke([aggregateId])!;
    }
}