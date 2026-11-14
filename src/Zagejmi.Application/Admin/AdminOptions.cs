using System;
using System.Linq;

using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Admin;

/// <summary>
///     Who the admins are (configuration section <c>Admin</c>). There is no admin role in the database yet; admins are
///     the users whose e-mail is listed here, e.g. <c>Admin__Emails__0=owner@zagejmi.cz</c>.
/// </summary>
public sealed class AdminOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SectionName = "Admin";

    /// <summary>
    ///     Gets or sets the admins' e-mail addresses.
    /// </summary>
    public string[] Emails { get; set; } = [];

    /// <summary>
    ///     Gets a value indicating whether the user is an admin. Requires a verified e-mail, so nobody becomes an admin by
    ///     registering with an admin's address.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true" /> for an admin.</returns>
    public bool IsAdmin(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.AuthCredentials.EmailVerifiedAt is not null
               && this.Emails.Any(e => string.Equals(e.Trim(), user.AuthCredentials.Email, StringComparison.OrdinalIgnoreCase));
    }
}
