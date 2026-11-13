using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.AccessTokens;

/// <inheritdoc />
public partial class _20261111141658_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "access_tokens");

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            schema: "access_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenFamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
                RevocationReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RevokedAccessTokens",
            schema: "access_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                JwtId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RevokedAccessTokens", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_ExpiresAt",
            schema: "access_tokens",
            table: "RefreshTokens",
            column: "ExpiresAt");

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_TokenHash",
            schema: "access_tokens",
            table: "RefreshTokens",
            column: "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_UserId_TokenFamilyId",
            schema: "access_tokens",
            table: "RefreshTokens",
            columns: new[] { "UserId", "TokenFamilyId" });

        migrationBuilder.CreateIndex(
            name: "IX_RevokedAccessTokens_ExpiresAt",
            schema: "access_tokens",
            table: "RevokedAccessTokens",
            column: "ExpiresAt");

        migrationBuilder.CreateIndex(
            name: "IX_RevokedAccessTokens_JwtId",
            schema: "access_tokens",
            table: "RevokedAccessTokens",
            column: "JwtId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RevokedAccessTokens_UserId",
            schema: "access_tokens",
            table: "RevokedAccessTokens",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RefreshTokens",
            schema: "access_tokens");

        migrationBuilder.DropTable(
            name: "RevokedAccessTokens",
            schema: "access_tokens");
    }
}
