using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261114020406_AddPlayerReviews : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlayerReviews",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PlayerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                AuthorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayerReviews", x => x.Id);
                table.CheckConstraint("CK_PlayerReviews_Rating", "\"Rating\" BETWEEN 1 AND 5");
                table.ForeignKey(
                    name: "FK_PlayerReviews_Customers_AuthorProfileId",
                    column: x => x.AuthorProfileId,
                    principalSchema: "profiles",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayerReviews_Players_PlayerProfileId",
                    column: x => x.PlayerProfileId,
                    principalSchema: "profiles",
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlayerReviews_AuthorProfileId",
            schema: "profiles",
            table: "PlayerReviews",
            column: "AuthorProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayerReviews_PlayerProfileId_AuthorProfileId",
            schema: "profiles",
            table: "PlayerReviews",
            columns: new[] { "PlayerProfileId", "AuthorProfileId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlayerReviews_PlayerProfileId_CreatedAt",
            schema: "profiles",
            table: "PlayerReviews",
            columns: new[] { "PlayerProfileId", "CreatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlayerReviews",
            schema: "profiles");
    }
}
