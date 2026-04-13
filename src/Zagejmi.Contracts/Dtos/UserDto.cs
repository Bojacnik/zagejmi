using System;

namespace Zagejmi.Contracts.Dtos;

/// <summary>
///     Basic user representation returned by API endpoints.
/// </summary>
public class UserDto
{
    /// <summary>
    ///     Gets or sets the user aggregate ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address associated with the account.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}