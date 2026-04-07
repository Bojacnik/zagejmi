using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Konscious.Security.Cryptography;

using LanguageExt;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Application.Handlers.User;

/// <summary>
///     Handles the execution of the <see cref="UserCreateCommand" /> command.
///     This handler implements the application logic for creating a new user, including:
///     - Validating input (email format, password strength, username length)
///     - Creating a new User aggregate with the provided credentials
///     - Persisting the domain events to the event store
///     - Publishing domain events for event-driven subscribers
///     This handler is part of the CQRS pattern and is responsible for processing write operations.
/// </summary>
public sealed class UserCreateCommandHandler : IHandlerRequest<UserCreateCommand, Either<Failure, Guid>>
{
    private readonly IEventStore eventStore;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserCreateCommandHandler" /> class with the required dependencies.
    /// </summary>
    /// <param name="eventStore">
    ///     The event store for persisting domain events. This is essential for event sourcing and maintaining
    ///     the audit trail of all user-related operations.
    /// </param>
    public UserCreateCommandHandler(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    /// <summary>
    ///     Asynchronously handles the create user command by validating inputs and creating a new User aggregate.
    /// </summary>
    /// <param name="request">The CreateUserCommand containing the user creation information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An Either monad containing either:
    ///     - Success: The newly created user's ID (Guid).
    ///     - Failure: A Failure object describing what went wrong.
    /// </returns>
    public async Task<Either<Failure, Guid>> Handle(
        UserCreateCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate username/nickname
            Either<Failure, string> usernameValidation = ValidateUsername(request.Username);
            if (usernameValidation.IsLeft)
            {
                return usernameValidation.LeftAsEnumerable().First();
            }

            // Validate email format
            Either<Failure, string> emailValidation = ValidateEmail(request.Email);
            if (emailValidation.IsLeft)
            {
                return emailValidation.LeftAsEnumerable().First();
            }

            // Validate password strength
            Either<Failure, string> passwordValidation = ValidatePassword(request.Password);
            if (passwordValidation.IsLeft)
            {
                return passwordValidation.LeftAsEnumerable().First();
            }

            // Hash the password using Argon2
            byte[] salt = Guid.CreateVersion7().ToByteArray();
            Argon2id argon = new(Encoding.UTF8.GetBytes(request.Password))
            {
                Salt = salt,
                Iterations = 4,
                MemorySize = 1024 * 16, // 16 MB
                DegreeOfParallelism = 2
            };
            byte[] hashBytes = argon.GetBytes(16);

            // Store salt + hash together (salt first 16 bytes, hash next 16 bytes)
            byte[] saltAndHash = new byte[32];
            Buffer.BlockCopy(salt, 0, saltAndHash, 0, 16);
            Buffer.BlockCopy(hashBytes, 0, saltAndHash, 16, 16);
            string passwordHash = Convert.ToBase64String(saltAndHash);

            // Create the user aggregate
            Domain.Auth.User user = Domain.Auth.User.Create(request.Username, passwordHash, request.Email);

            // Persist the domain events to the event store
            await this.eventStore.AppendAsync(
                user.Id,
                0,
                user.DomainEvents,
                cancellationToken);

            // Return the newly created user ID
            return user.Id;
        }
        catch (Exception ex)
        {
            // Log the exception (in a real application, use a proper logging framework)
            return new FailureEventStoreUnableToSave($"An error occurred while creating the user: {ex.Message}");
        }
    }

    /// <summary>
    ///     Validates the username/nickname to ensure it meets requirements.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>
    ///     An Either containing the validated username or a validation failure.
    /// </returns>
    private static Either<Failure, string> ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new FailureArgumentInvalidValue("Username cannot be empty.");
        }

        if (username.Length < 3)
        {
            return new FailureArgumentInvalidValue("Username must be at least 3 characters long.");
        }

        if (username.Length > 50)
        {
            return new FailureArgumentInvalidValue("Username must not exceed 50 characters.");
        }

        return username;
    }

    /// <summary>
    ///     Validates the email format.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>
    ///     An Either containing the validated email or a validation failure.
    /// </returns>
    private static Either<Failure, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new FailureArgumentInvalidValue("Email cannot be empty.");
        }

        // Basic email validation regex
        const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            return new FailureArgumentInvalidValue("Email must be in a valid format (e.g., user@example.com).");
        }

        if (email.Length > 254)
        {
            return new FailureArgumentInvalidValue("Email must not exceed 254 characters.");
        }

        return email;
    }

    /// <summary>
    ///     Validates the password strength.
    ///     Requirements:
    ///     - At least 8 characters long
    ///     - Contains at least one uppercase letter
    ///     - Contains at least one lowercase letter
    ///     - Contains at least one digit
    ///     - Contains at least one special character (!@#$%^&*).
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>
    ///     An Either containing the validated password or a validation failure.
    /// </returns>
    private static Either<Failure, string> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return new FailureArgumentInvalidValue("Password cannot be empty.");
        }

        if (password.Length < 8)
        {
            return new FailureArgumentInvalidValue("Password must be at least 8 characters long.");
        }

        if (password.Length > 128)
        {
            return new FailureArgumentInvalidValue("Password must not exceed 128 characters.");
        }

        // Check for uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return new FailureArgumentInvalidValue("Password must contain at least one uppercase letter.");
        }

        // Check for lowercase letter
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return new FailureArgumentInvalidValue("Password must contain at least one lowercase letter.");
        }

        // Check for digit
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return new FailureArgumentInvalidValue("Password must contain at least one digit.");
        }

        // Check for special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            return new FailureArgumentInvalidValue(
                "Password must contain at least one special character (!@#$%^&*()_+-=[]{};:'\"|,.<>/?\\).");
        }

        return password;
    }
}