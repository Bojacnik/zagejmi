using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Passwords;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.CreateUser;

/// <summary>
///     Represents a handler for creating a new user in the application.
/// </summary>
public class CreateUserHandler : IHandlerRequest<CreateUserCommand, Result<Guid>>
{
    /// <summary>
    ///     The user repository used for creating and managing user entities.
    /// </summary>
    private readonly IUserRepository userRepository;

    /// <summary>
    ///     The password hasher used to hash the plain text password before it is stored.
    /// </summary>
    private readonly IPasswordHasher passwordHasher;

    /// <summary>
    ///     The profile repository used to create the customer profile of the new user.
    /// </summary>
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Issues and sends the email verification link.
    /// </summary>
    private readonly EmailVerificationMailer mailer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateUserHandler" /> class with the specified user repository.
    /// </summary>
    /// <param name="userRepository">The user repository used for creating and managing user entities.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="mailer">Issues and sends the email verification link.</param>
    public CreateUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IProfileRepository profileRepository,
        EmailVerificationMailer mailer)
    {
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
        this.profileRepository = profileRepository;
        this.mailer = mailer;
    }

    /// <summary>
    ///     Creates the user together with their customer profile (every user is a customer), emails the verification link
    ///     and returns the user ID.
    /// </summary>
    /// <param name="request">The command containing the details of the user to be created.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identifier of the created user, or a validation failure.</returns>
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        Result<GrammaticalGender> grammaticalGender =
            GrammaticalGenderRules.Resolve(request.Gender, request.GrammaticalGender);

        if (grammaticalGender.IsFailure)
        {
            return Result<Guid>.Failure(grammaticalGender.Error!);
        }

        string userName = request.UserName.Trim();
        string email = request.Email.Trim();

        if (userName.Length is < 3 or > 64)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "User.UserNameLength",
                "Uživatelské jméno musí mít 3 až 64 znaků."));
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "User.EmailInvalid",
                "Zadejte platnou e-mailovou adresu."));
        }

        if (request.Password.Length < 8)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "User.PasswordTooShort",
                "Heslo musí mít alespoň 8 znaků."));
        }

        if (await this.userRepository.IsUserNameOrEmailTakenAsync(userName, email, cancellationToken))
        {
            return Result<Guid>.Failure(ErrorDetail.Conflict(
                "User.AlreadyExists",
                "Uživatelské jméno nebo e-mail už někdo používá."));
        }

        User user = new(Guid.CreateVersion7())
        {
            AuthCredentials = new AuthCredentials(
                userName,
                this.passwordHasher.Hash(request.Password),
                email),
            Gender = request.Gender,
            GrammaticalGender = grammaticalGender.Value,
        };

        string verificationToken = this.mailer.IssueToken(user);
        await this.userRepository.AddAsync(user, cancellationToken);

        // TODO: users and profiles live in separate DbContexts, so these two writes are not atomic.
        CustomerProfile customer = new(Guid.CreateVersion7(), user.Id, userName);
        await this.profileRepository.AddCustomerAsync(customer, cancellationToken);

        // A failed delivery does not fail the registration: the login page can send a new link.
        await this.mailer.SendAsync(user, verificationToken, cancellationToken);

        return user.Id;
    }
}
