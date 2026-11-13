using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261113121607_AddServiceGameRankPlayStyles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GameKey",
            schema: "profiles",
            table: "PlayerServices",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PlayStyles",
            schema: "profiles",
            table: "PlayerServices",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "RankKey",
            schema: "profiles",
            table: "PlayerServices",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlayerServices_GameKey",
            schema: "profiles",
            table: "PlayerServices",
            column: "GameKey");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PlayerServices_GameKey",
            schema: "profiles",
            table: "PlayerServices");

        migrationBuilder.DropColumn(
            name: "GameKey",
            schema: "profiles",
            table: "PlayerServices");

        migrationBuilder.DropColumn(
            name: "PlayStyles",
            schema: "profiles",
            table: "PlayerServices");

        migrationBuilder.DropColumn(
            name: "RankKey",
            schema: "profiles",
            table: "PlayerServices");
    }
}
