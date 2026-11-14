using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Wallets;

/// <inheritdoc />
public partial class _20261114023134_AddOrders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            schema: "wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                PlayerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PriceUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                UnitPrice = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                Total = table.Column<long>(type: "bigint", nullable: false),
                Fee = table.Column<long>(type: "bigint", nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Cancellation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                AcceptDeadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ConfirmDeadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                DisputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                DisputeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.CheckConstraint("CK_Orders_Amounts", "\"Total\" >= 0 AND \"Fee\" >= 0 AND \"Fee\" <= \"Total\"");
                table.CheckConstraint("CK_Orders_Quantity", "\"Quantity\" BETWEEN 1 AND 10");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Orders_AcceptDeadline",
            schema: "wallets",
            table: "Orders",
            column: "AcceptDeadline",
            filter: "\"Status\" = 'Pending'");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_ConfirmDeadline",
            schema: "wallets",
            table: "Orders",
            column: "ConfirmDeadline",
            filter: "\"Status\" = 'Finished'");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerProfileId_CreatedAt",
            schema: "wallets",
            table: "Orders",
            columns: new[] { "CustomerProfileId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Orders_DisputedAt",
            schema: "wallets",
            table: "Orders",
            column: "DisputedAt",
            filter: "\"Status\" = 'Disputed'");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_PlayerProfileId_CreatedAt",
            schema: "wallets",
            table: "Orders",
            columns: new[] { "PlayerProfileId", "CreatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Orders",
            schema: "wallets");
    }
}
