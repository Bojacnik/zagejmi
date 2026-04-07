using System.ComponentModel.DataAnnotations;

using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Commands;

/// <summary>
///     Represents a command to create a new user in the system.
///     This command encapsulates all the necessary information required to create a user account,
///     including authentication credentials such as username, password, and email address.
/// </summary>
public class UserCreateCommand : IMessage
{
    /// <summary>
    ///     Gets or sets the username/nickname for the new user account.
    ///     The username must be unique across all users in the system, at least 3 characters long,
    ///     and is used for authentication and identification.
    /// </summary>
    [Required(ErrorMessage = "Username cannot be empty.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the plaintext password for the new user account.
    ///     This password will be hashed and securely stored.
    ///     Requirements: At least 8 characters, with uppercase, lowercase, digit, and special character.
    /// </summary>
    [Required(ErrorMessage = "Password cannot be empty.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address for the new user account.
    ///     The email address must be unique across all users, valid format,
    ///     and is used for communication and identity verification.
    /// </summary>
    [Required(ErrorMessage = "Email cannot be empty.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; set; } = string.Empty;
}