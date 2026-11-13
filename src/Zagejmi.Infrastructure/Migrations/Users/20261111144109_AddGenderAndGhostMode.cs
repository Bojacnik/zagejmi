using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Users;

/// <inheritdoc />
public partial class _20261111144109_AddGenderAndGhostMode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Gender",
            schema: "users",
            table: "Users",
            type: "character varying(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "Unknown");

        migrationBuilder.AddColumn<string>(
            name: "GrammaticalGender",
            schema: "users",
            table: "Users",
            type: "character varying(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "Masculine");

        migrationBuilder.AddColumn<bool>(
            name: "IsGhostMode",
            schema: "users",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Gender",
            schema: "users",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "GrammaticalGender",
            schema: "users",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IsGhostMode",
            schema: "users",
            table: "Users");
    }
}
