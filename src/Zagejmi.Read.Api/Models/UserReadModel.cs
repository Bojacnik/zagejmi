using System;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Read-side representation of an authenticated user.
/// </summary>
public class UserReadModel
{
    /// <summary>
    ///     Gets or sets the user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether email was verified.
    /// </summary>
    public bool IsEmailVerified { get; set; }
}