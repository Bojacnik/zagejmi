using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Infrastructure.Auth.Users.Ctx;
using Zagejmi.Infrastructure.Auth.Users.Records;

namespace Zagejmi.Infrastructure.Auth.Users.Repositories;

/// <summary>
///     Represents the repository for managing user entities in the application.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<ZagejmiUsersContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRepository" /> class with the specified database context.
    /// </summary>
    /// <param name="contextFactory">Creates a short-lived database context per operation, so concurrent calls on one Blazor circuit never share a DbContext.</param>
    public UserRepository(IDbContextFactory<ZagejmiUsersContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        UserRecord? record = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    /// <inheritdoc />
    public async Task<Guid> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        UserRecord userRecord = new()
        {
            Id = entity.Id,
            Email = entity.AuthCredentials.Email,
            PasswordHash = entity.AuthCredentials.PasswordHash,
            UserName = entity.AuthCredentials.UserName,
            EmailVerifiedAt = entity.AuthCredentials.EmailVerifiedAt,
            PendingEmailVerificationTokenHash = entity.AuthCredentials.PendingEmailVerificationTokenHash,
            PendingEmailVerificationTokenExpiresAt = entity.AuthCredentials.PendingEmailVerificationExpiresAt,
            Gender = entity.Gender,
            GrammaticalGender = entity.GrammaticalGender,
            IsGhostMode = entity.IsGhostMode,
            DefaultProfile = entity.DefaultProfile,
        };

        context.Users.Add(userRecord);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        UserRecord userRecord = await context.Users
                                    .FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
                                ?? throw new UserNotFoundException(entity.Id);

        userRecord.Email = entity.AuthCredentials.Email;
        userRecord.UserName = entity.AuthCredentials.UserName;
        userRecord.PasswordHash = entity.AuthCredentials.PasswordHash;
        userRecord.EmailVerifiedAt = entity.AuthCredentials.EmailVerifiedAt;
        userRecord.PendingEmailVerificationTokenHash = entity.AuthCredentials.PendingEmailVerificationTokenHash;
        userRecord.PendingEmailVerificationTokenExpiresAt = entity.AuthCredentials.PendingEmailVerificationExpiresAt;
        userRecord.Gender = entity.Gender;
        userRecord.GrammaticalGender = entity.GrammaticalGender;
        userRecord.IsGhostMode = entity.IsGhostMode;
        userRecord.DefaultProfile = entity.DefaultProfile;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        int deleted = await context.Users
            .Where(x => x.Id == entityId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            throw new UserNotFoundException(entityId);
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsUserNameOrEmailTakenAsync(
        string userName,
        string email,
        CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        string normalizedUserName = userName.ToLowerInvariant();
        string normalizedEmail = email.ToLowerInvariant();

        // Runs in PostgreSQL as lower(column); Npgsql cannot translate ToLowerInvariant or StringComparison overloads.
#pragma warning disable CA1304, CA1311, CA1862
        return await context.Users.AnyAsync(
            x => x.UserName.ToLower() == normalizedUserName || x.Email.ToLower() == normalizedEmail,
            cancellationToken);
#pragma warning restore CA1304, CA1311, CA1862
    }

    /// <inheritdoc />
    public async Task<User?> GetByUserNameOrEmailAsync(
        string userNameOrEmail,
        CancellationToken cancellationToken = default)
    {
        await using ZagejmiUsersContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        string normalized = userNameOrEmail.Trim().ToLowerInvariant();

        // Runs in PostgreSQL as lower(column); Npgsql cannot translate ToLowerInvariant or StringComparison overloads.
#pragma warning disable CA1304, CA1311, CA1862
        UserRecord? record = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserName.ToLower() == normalized || x.Email.ToLower() == normalized,
                cancellationToken);
#pragma warning restore CA1304, CA1311, CA1862

        return record is null ? null : ToDomain(record);
    }

    private static User ToDomain(UserRecord record)
    {
        return new User(record.Id)
        {
            AuthCredentials = new AuthCredentials(
                record.UserName,
                record.PasswordHash,
                record.Email,
                record.EmailVerifiedAt,
                record.PendingEmailVerificationTokenHash,
                record.PendingEmailVerificationTokenExpiresAt),
            Gender = record.Gender,
            GrammaticalGender = record.GrammaticalGender,
            IsGhostMode = record.IsGhostMode,
            DefaultProfile = record.DefaultProfile,
        };
    }
}

/// <summary>
///     Represents an exception that is thrown when a user with a specified ID is not found in
///     the repository. This exception is typically used to indicate that an operation on a user entity cannot be completed
///     because the user does not exist in the data store. It provides a clear and specific error message that includes the
///     user ID that was not found, allowing for easier debugging and error handling in the application.
/// </summary>
public class UserNotFoundException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserNotFoundException" /> class with the specified user ID that was
    ///     not found.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user that was not found in the repository. This parameter is used to
    ///     construct a detailed error message that indicates which user ID was not found, providing context for the exception
    ///     and aiding in debugging and error handling within the application.
    /// </param>
    public UserNotFoundException(Guid userId)
        : base($"User with ID '{userId}' was not found.")
    {
    }
}