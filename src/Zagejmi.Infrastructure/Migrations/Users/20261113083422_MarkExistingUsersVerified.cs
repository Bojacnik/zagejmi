using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Users;

/// <inheritdoc />
public partial class _20261113083422_MarkExistingUsersVerified : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Login now requires a verified email. Accounts created before verification existed could never verify, so
        // they count as verified (they do not get the sign-up bonus, which is granted only by verifying).
        migrationBuilder.Sql(
            """
            UPDATE users."Users"
            SET "EmailVerifiedAt" = now(),
                "PendingEmailVerificationTokenHash" = NULL,
                "PendingEmailVerificationTokenExpiresAt" = NULL
            WHERE "EmailVerifiedAt" IS NULL;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Nothing to undo: which users were verified by this migration is not recorded.
    }
}
