using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261112090204_AddPlayerServices : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlayerServices",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PlayerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                PriceUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                PromoTag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ImageId = table.Column<Guid>(type: "uuid", nullable: true),
                IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayerServices", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlayerServices_Players_PlayerProfileId",
                    column: x => x.PlayerProfileId,
                    principalSchema: "profiles",
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlayerServices_PlayerProfileId_CreatedAt",
            schema: "profiles",
            table: "PlayerServices",
            columns: new[] { "PlayerProfileId", "CreatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlayerServices",
            schema: "profiles");
    }
}
