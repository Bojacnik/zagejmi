using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261111141635_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "profiles");

        migrationBuilder.CreateTable(
            name: "Associates",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                DisplayName = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                CardProfilePictureId = table.Column<Guid>(type: "uuid", nullable: true),
                BannerPictureId = table.Column<Guid>(type: "uuid", nullable: true),
                BackgroundServicesPictureId = table.Column<Guid>(type: "uuid", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Associates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                DisplayName = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                ProfilePictureId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                SubscribedId = table.Column<Guid>(type: "uuid", nullable: false),
                StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Subscriptions_Associates_SubscribedId",
                    column: x => x.SubscribedId,
                    principalSchema: "profiles",
                    principalTable: "Associates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Subscriptions_Customers_SubscriberId",
                    column: x => x.SubscriberId,
                    principalSchema: "profiles",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Associates_OwnerId",
            schema: "profiles",
            table: "Associates",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_DisplayName",
            schema: "profiles",
            table: "Customers",
            column: "DisplayName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Email",
            schema: "profiles",
            table: "Customers",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "profiles",
            table: "Customers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_SubscribedId",
            schema: "profiles",
            table: "Subscriptions",
            column: "SubscribedId");

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_SubscriberId",
            schema: "profiles",
            table: "Subscriptions",
            column: "SubscriberId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Subscriptions",
            schema: "profiles");

        migrationBuilder.DropTable(
            name: "Associates",
            schema: "profiles");

        migrationBuilder.DropTable(
            name: "Customers",
            schema: "profiles");
    }
}
