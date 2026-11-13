using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261113083418_WholeGoinPrices : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Goins are whole numbers now. PostgreSQL converts numeric to bigint by rounding (199.50 -> 200).
        migrationBuilder.AlterColumn<long>(
            name: "Price",
            schema: "profiles",
            table: "PlayerServices",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(12,2)",
            oldPrecision: 12,
            oldScale: 2);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            schema: "profiles",
            table: "PlayerServices",
            type: "numeric(12,2)",
            precision: 12,
            scale: 2,
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint");
    }
}
