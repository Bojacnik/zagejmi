using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Domain.Auth;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.ResendVerificationEmail;

/// <summary>
///     Sends a new verification link to an unverified user.
/// </summary>
public sealed class ResendVerificationEmailHandler : IHandlerRequest<ResendVerificationEmailCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly EmailVerificationMailer mailer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResendVerificationEmailHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="mailer">Issues and sends verification links.</param>
    public ResendVerificationEmailHandler(IUserRepository userRepository, EmailVerificationMailer mailer)
    {
        this.userRepository = userRepository;
        this.mailer = mailer;
    }

    /// <summary>
    ///     Sends the link. The result is the same whether the account exists, is already verified or was emailed less than
    ///     a minute ago, so the response does not reveal which accounts exist.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success, or a validation failure for an empty identifier.</returns>
    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken = default)
    {
        string identifier = request.UserIdentifier.Trim();
        if (identifier.Length == 0)
        {
            return Result.Failure(ErrorDetail.Validation(
                "User.IdentifierRequired",
                "Zadejte uživatelské jméno nebo e-mail."));
        }

        User? user = await this.userRepository.GetByUserNameOrEmailAsync(identifier, cancellationToken);
        if (user is null || user.AuthCredentials.EmailVerifiedAt is not null || !this.mailer.CanResend(user))
        {
            return Result.Success();
        }

        string token = this.mailer.IssueToken(user);
        await this.userRepository.UpdateAsync(user, cancellationToken);
        await this.mailer.SendAsync(user, token, cancellationToken);

        return Result.Success();
    }
}
