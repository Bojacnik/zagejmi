using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Users;

/// <inheritdoc />
public partial class _20261112083007_AddDefaultProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DefaultProfile",
            schema: "users",
            table: "Users",
            type: "character varying(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "Customer");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DefaultProfile",
            schema: "users",
            table: "Users");
    }
}
