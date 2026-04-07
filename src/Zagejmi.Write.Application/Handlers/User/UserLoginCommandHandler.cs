using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Konscious.Security.Cryptography;

using LanguageExt;

using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Application.Handlers.User;

/// <summary>
///     Handles the execution of the <see cref="UserLoginCommand" /> command.
///     This handler implements the application logic for user login, including:
///     - Validating user credentials (username/email and password)
///     - Generating JWT tokens for authenticated users
///     - Handling authentication failures gracefully.
/// </summary>
public sealed class UserLoginCommandHandler : IHandlerRequest<UserLoginCommand, Either<Failure, string>>
{
    private static readonly Action<ILogger, string, Exception?> UserNotFoundLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[LoginHandler] User not found for identifier: {Username}");

    private static readonly Action<ILogger, Exception?> UserFoundLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "[LoginHandler] User found! Verifying password");

    private static readonly Action<ILogger, string, Exception?> PasswordVerificationFailedLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[LoginHandler] Password verification failed for user: {Username}");

    private static readonly Action<ILogger, string, Exception?> PasswordVerifiedSuccessfullyLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "[LoginHandler] Password verified successfully for user: {Username}");

    private static readonly Action<ILogger, string, Exception> LoginErrorLogger =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            default,
            "An error occurred during login for user {Username}");

    private static readonly Action<ILogger, Exception?> PasswordHashMismatchLogger =
        LoggerMessage.Define(
            LogLevel.Warning,
            default,
            "[VerifyPassword] Password hash mismatch!");

    private static readonly Action<ILogger, string, Exception> PasswordVerificationErrorLogger =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            default,
            "[VerifyPassword] Error during password verification: {Error}");

    private readonly ILogger<UserLoginCommandHandler> logger;
    private readonly ITokenService tokenService;
    private readonly IUserRepository userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLoginCommandHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The repository for accessing user data.</param>
    /// <param name="tokenService">The service for generating JWT tokens.</param>
    /// <param name="logger">The logger instance.</param>
    public UserLoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<UserLoginCommandHandler> logger)
    {
        this.userRepository = userRepository;
        this.tokenService = tokenService;
        this.logger = logger;
    }

    /// <summary>
    ///     Asynchronously handles the user login command by validating credentials and generating a JWT token.
    /// </summary>
    /// <param name="request">The CommandUserLogin containing username/email and password.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An Either monad containing either:
    ///     - Success: A JWT token string for the authenticated user.
    ///     - Failure: A Failure object describing the authentication error.
    /// </returns>
    public async Task<Either<Failure, string>> Handle(
        UserLoginCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return new VerificationFailureInvalidLogin("Username or email cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new VerificationFailureInvalidLogin("Password cannot be empty.");
            }

            // Determine if the identifier is an email (contains @) or username
            Domain.Auth.User? user;
            if (request.Username.Contains('@', StringComparison.OrdinalIgnoreCase))
            {
                // Try email lookup first, then username as fallback
                user = await this.userRepository.GetByEmailAsync(request.Username, cancellationToken);
                user ??= await this.userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            }
            else
            {
                // Try username lookup first, then email as fallback
                user = await this.userRepository.GetByUsernameAsync(request.Username, cancellationToken);
                user ??= await this.userRepository.GetByEmailAsync(request.Username, cancellationToken);
            }

            if (user == null)
            {
                UserNotFoundLogger(this.logger, request.Username, null);
                return new VerificationFailureInvalidLogin("Invalid username/email or password.");
            }

            UserFoundLogger(this.logger, null);

            // Verify password
            if (!VerifyPassword(request.Password, user.AuthCredentials!.PasswordHash, this.logger))
            {
                PasswordVerificationFailedLogger(this.logger, request.Username, null);
                return new VerificationFailureInvalidLogin("Invalid username/email or password.");
            }

            PasswordVerifiedSuccessfullyLogger(this.logger, request.Username, null);

            // Generate and return JWT token
            string token = this.tokenService.GenerateToken(user);
            return token;
        }
        catch (Exception ex)
        {
            LoginErrorLogger(this.logger, request.Username, ex);
            return new FailureEventStoreConnectionLost($"An error occurred during login: {ex.Message}");
        }
    }

    /// <summary>
    ///     Verifies a plaintext password against a stored Argon2 hash.
    /// </summary>
    /// <param name="plainPassword">The plaintext password to verify.</param>
    /// <param name="hash">The Argon2 hash to verify against.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    private static bool VerifyPassword(string plainPassword, string hash, ILogger<UserLoginCommandHandler> logger)
    {
        try
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            Argon2id argon = new(Encoding.UTF8.GetBytes(plainPassword))
            {
                Salt = hashBytes.AsSpan(0, 16).ToArray(),
                Iterations = 4,
                MemorySize = 1024 * 16,
                DegreeOfParallelism = 2
            };

            byte[] computedHash = argon.GetBytes(16);
            bool isValid = computedHash.AsSpan().SequenceEqual(hashBytes.AsSpan(16));

            if (!isValid)
            {
                PasswordHashMismatchLogger(logger, null);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            PasswordVerificationErrorLogger(logger, ex.Message, ex);
            return false;
        }
    }
}