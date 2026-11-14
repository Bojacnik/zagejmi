using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261114025813_AddServiceReviews : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ServiceReviews",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PlayerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                AuthorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServiceReviews", x => x.Id);
                table.CheckConstraint("CK_ServiceReviews_Rating", "\"Rating\" BETWEEN 1 AND 5");
                table.ForeignKey(
                    name: "FK_ServiceReviews_Customers_AuthorProfileId",
                    column: x => x.AuthorProfileId,
                    principalSchema: "profiles",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ServiceReviews_Players_PlayerProfileId",
                    column: x => x.PlayerProfileId,
                    principalSchema: "profiles",
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ServiceReviews_AuthorProfileId",
            schema: "profiles",
            table: "ServiceReviews",
            column: "AuthorProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_ServiceReviews_OrderId",
            schema: "profiles",
            table: "ServiceReviews",
            column: "OrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ServiceReviews_PlayerProfileId_CreatedAt",
            schema: "profiles",
            table: "ServiceReviews",
            columns: new[] { "PlayerProfileId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ServiceReviews_ServiceId",
            schema: "profiles",
            table: "ServiceReviews",
            column: "ServiceId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ServiceReviews",
            schema: "profiles");
    }
}
