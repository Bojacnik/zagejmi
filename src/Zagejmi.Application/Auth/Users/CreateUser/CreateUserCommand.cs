using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Auth.Users.CreateUser;

/// <summary>
///     Represents a command to create a new user in the application.
/// </summary>
public class CreateUserCommand
{
    /// <summary>
    ///     Gets the username for the new user.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    ///     Gets the password for the new user.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    ///     Gets the email for the new user.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    ///     Gets the gender the user identifies as.
    /// </summary>
    public Gender Gender { get; init; }

    /// <summary>
    ///     Gets the grammatical gender the user wants to be addressed in. Required when <see cref="Gender" /> is
    ///     <see cref="Domain.Auth.Gender.Other" /> or <see cref="Domain.Auth.Gender.Unknown" />; ignored otherwise.
    /// </summary>
    public GrammaticalGender? GrammaticalGender { get; init; }
}
