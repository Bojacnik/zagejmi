using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Read.Api.Ctx;
using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Services;

/// <summary>
///     Provides read-model access for user lookups.
/// </summary>
public class UserReadRepository
{
    private readonly ZagejmiReadContext context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserReadRepository" /> class.
    /// </summary>
    /// <param name="context">Read-side database context.</param>
    public UserReadRepository(ZagejmiReadContext context)
    {
        this.context = context;
    }

    /// <summary>
    ///     Finds a user by username or email using a case-insensitive match.
    /// </summary>
    /// <param name="identifier">User-supplied username or email.</param>
    /// <returns>The matching user when found; otherwise <see langword="null" />.</returns>
    public async Task<UserReadModel?> FindByUserNameOrEmailAsync(string identifier)
    {
        string normalizedIdentifier = identifier.Trim();
        if (string.IsNullOrWhiteSpace(normalizedIdentifier))
        {
            return null;
        }

        return await this.context.Users
            .FirstOrDefaultAsync(u =>
                EF.Functions.ILike(u.UserName, normalizedIdentifier) ||
                EF.Functions.ILike(u.Email, normalizedIdentifier));
    }
}