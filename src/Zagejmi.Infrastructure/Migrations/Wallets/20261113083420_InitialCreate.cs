using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zagejmi.Infrastructure.Migrations.Wallets;

/// <inheritdoc />
public partial class _20261113083420_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "wallets");

        migrationBuilder.CreateTable(
            name: "Accounts",
            schema: "wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                OwnerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                Balance = table.Column<long>(type: "bigint", nullable: false),
                AllowNegative = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", x => x.Id);
                table.CheckConstraint("CK_Accounts_NotNegative", "\"AllowNegative\" OR \"Balance\" >= 0");
            });

        migrationBuilder.CreateTable(
            name: "BonusGrants",
            schema: "wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<long>(type: "bigint", nullable: false),
                Remaining = table.Column<long>(type: "bigint", nullable: false),
                Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BonusGrants", x => x.Id);
                table.CheckConstraint("CK_BonusGrants_Remaining", "\"Remaining\" >= 0 AND \"Remaining\" <= \"Amount\"");
            });

        migrationBuilder.CreateTable(
            name: "LedgerTransactions",
            schema: "wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LedgerTransactions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "LedgerEntries",
            schema: "wallets",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                AccountKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                Amount = table.Column<long>(type: "bigint", nullable: false),
                Funding = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                BonusGrantId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                table.CheckConstraint("CK_LedgerEntries_BonusHasGrant", "(\"Funding\" = 'Bonus') = (\"BonusGrantId\" IS NOT NULL)");
                table.CheckConstraint("CK_LedgerEntries_NotZero", "\"Amount\" <> 0");
                table.ForeignKey(
                    name: "FK_LedgerEntries_Accounts_AccountId",
                    column: x => x.AccountId,
                    principalSchema: "wallets",
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_LedgerEntries_BonusGrants_BonusGrantId",
                    column: x => x.BonusGrantId,
                    principalSchema: "wallets",
                    principalTable: "BonusGrants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_LedgerEntries_LedgerTransactions_TransactionId",
                    column: x => x.TransactionId,
                    principalSchema: "wallets",
                    principalTable: "LedgerTransactions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            schema: "wallets",
            table: "Accounts",
            columns: new[] { "Id", "AllowNegative", "Balance", "CreatedAt", "Kind", "OwnerProfileId" },
            values: new object[,]
            {
                { new Guid("00000000-0000-7000-8000-000000000001"), true, 0L, new DateTimeOffset(new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PromoBudget", null },
                { new Guid("00000000-0000-7000-8000-000000000002"), false, 0L, new DateTimeOffset(new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PlatformRevenue", null },
                { new Guid("00000000-0000-7000-8000-000000000003"), false, 0L, new DateTimeOffset(new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Escrow", null },
                { new Guid("00000000-0000-7000-8000-000000000004"), true, 0L, new DateTimeOffset(new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "TopUpClearing", null },
                { new Guid("00000000-0000-7000-8000-000000000005"), false, 0L, new DateTimeOffset(new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PayoutClearing", null }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Accounts_OwnerProfileId_Kind",
            schema: "wallets",
            table: "Accounts",
            columns: new[] { "OwnerProfileId", "Kind" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BonusGrants_CustomerProfileId",
            schema: "wallets",
            table: "BonusGrants",
            column: "CustomerProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_BonusGrants_ExpiresAt",
            schema: "wallets",
            table: "BonusGrants",
            column: "ExpiresAt",
            filter: "\"Remaining\" > 0");

        migrationBuilder.CreateIndex(
            name: "IX_LedgerEntries_AccountId_TransactionId",
            schema: "wallets",
            table: "LedgerEntries",
            columns: new[] { "AccountId", "TransactionId" });

        migrationBuilder.CreateIndex(
            name: "IX_LedgerEntries_BonusGrantId",
            schema: "wallets",
            table: "LedgerEntries",
            column: "BonusGrantId");

        migrationBuilder.CreateIndex(
            name: "IX_LedgerEntries_TransactionId",
            schema: "wallets",
            table: "LedgerEntries",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_LedgerTransactions_CreatedAt",
            schema: "wallets",
            table: "LedgerTransactions",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_LedgerTransactions_Type_IdempotencyKey",
            schema: "wallets",
            table: "LedgerTransactions",
            columns: new[] { "Type", "IdempotencyKey" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LedgerEntries",
            schema: "wallets");

        migrationBuilder.DropTable(
            name: "Accounts",
            schema: "wallets");

        migrationBuilder.DropTable(
            name: "BonusGrants",
            schema: "wallets");

        migrationBuilder.DropTable(
            name: "LedgerTransactions",
            schema: "wallets");
    }
}
