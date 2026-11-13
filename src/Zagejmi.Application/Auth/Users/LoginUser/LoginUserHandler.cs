using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Passwords;
using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.LoginUser;

/// <summary>
///     Signs a user in: verifies the credentials and issues an access token together with a refresh token.
/// </summary>
public class LoginUserHandler : IHandlerRequest<LoginUserCommand, Result<LoginUserResult>>
{
    /// <summary>
    ///     Returned (only after a correct password) when the email address is not verified yet.
    /// </summary>
    public static readonly ErrorDetail EmailNotVerified = ErrorDetail.Validation(
        "User.EmailNotVerified",
        "Nejdříve ověřte svůj e-mail. Odkaz jsme vám poslali při registraci.");

    private static readonly ErrorDetail InvalidCredentials = ErrorDetail.Validation(
        "User.InvalidCredentials",
        "Nesprávné uživatelské jméno, e-mail nebo heslo.");

    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly IRefreshTokenRepository refreshTokenRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginUserHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="refreshTokenRepository">The refresh token store.</param>
    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    ///     Verifies the credentials and issues tokens.
    /// </summary>
    /// <param name="request">The sign-in data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The issued tokens, or a validation failure.</returns>
    public async Task<Result<LoginUserResult>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken = default)
    {
        string identifier = request.UserIdentifier.Trim();

        if (identifier.Length == 0 || string.IsNullOrEmpty(request.Password))
        {
            return Result<LoginUserResult>.Failure(ErrorDetail.Validation(
                "User.CredentialsRequired",
                "Zadejte uživatelské jméno nebo e-mail a heslo."));
        }

        User? user = await this.userRepository.GetByUserNameOrEmailAsync(identifier, cancellationToken);

        // Same error for an unknown user and a wrong password, so the response does not reveal which accounts exist.
        if (user is null || !this.passwordHasher.Verify(user.AuthCredentials.PasswordHash, request.Password))
        {
            return Result<LoginUserResult>.Failure(InvalidCredentials);
        }

        if (user.AuthCredentials.EmailVerifiedAt is null)
        {
            return Result<LoginUserResult>.Failure(EmailNotVerified);
        }

        string accessToken = this.tokenService.GenerateToken(user);
        string refreshToken = this.tokenService.GenerateRefreshToken();

        await this.refreshTokenRepository.AddAsync(
            user.Id,
            this.tokenService.HashRefreshToken(refreshToken),
            DateTimeOffset.UtcNow.Add(this.tokenService.RefreshTokenLifetime),
            cancellationToken);

        return new LoginUserResult(user.Id, accessToken, refreshToken);
    }
}
