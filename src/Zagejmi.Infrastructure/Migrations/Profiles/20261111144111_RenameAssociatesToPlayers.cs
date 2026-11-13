using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.Profiles;

/// <inheritdoc />
public partial class _20261111144111_RenameAssociatesToPlayers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Hand-written: EF scaffolded a drop + create of the Associates table, which would lose data.
        migrationBuilder.DropForeignKey(
            name: "FK_Subscriptions_Associates_SubscribedId",
            schema: "profiles",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Associates_OwnerId",
            schema: "profiles",
            table: "Associates");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Associates",
            schema: "profiles",
            table: "Associates");

        migrationBuilder.RenameTable(
            name: "Associates",
            schema: "profiles",
            newName: "Players",
            newSchema: "profiles");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Players",
            schema: "profiles",
            table: "Players",
            column: "Id");

        migrationBuilder.RenameColumn(
            name: "OwnerId",
            schema: "profiles",
            table: "Players",
            newName: "UserId");

        migrationBuilder.DropColumn(
            name: "Email",
            schema: "profiles",
            table: "Players");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedAt",
            schema: "profiles",
            table: "Players",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "now()");

        migrationBuilder.DropIndex(
            name: "IX_Customers_Email",
            schema: "profiles",
            table: "Customers");

        migrationBuilder.DropIndex(
            name: "IX_Customers_UserId",
            schema: "profiles",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "Email",
            schema: "profiles",
            table: "Customers");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreatedAt",
            schema: "profiles",
            table: "Customers",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "now()");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "profiles",
            table: "Customers",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Players_DisplayName",
            schema: "profiles",
            table: "Players",
            column: "DisplayName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Players_UserId",
            schema: "profiles",
            table: "Players",
            column: "UserId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Subscriptions_Players_SubscribedId",
            schema: "profiles",
            table: "Subscriptions",
            column: "SubscribedId",
            principalSchema: "profiles",
            principalTable: "Players",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        // Every user is a customer: backfill a customer profile for users registered before this change.
        // Reads users."Users" (owned by ZagejmiUsersContext, whose migrations run first at startup).
        migrationBuilder.Sql("""
            INSERT INTO profiles."Customers" ("Id", "UserId", "DisplayName", "CreatedAt")
            SELECT gen_random_uuid(), u."Id", u."UserName", now()
            FROM users."Users" u
            WHERE NOT EXISTS (SELECT 1 FROM profiles."Customers" c WHERE c."UserId" = u."Id")
            ON CONFLICT DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Subscriptions_Players_SubscribedId",
            schema: "profiles",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Players_UserId",
            schema: "profiles",
            table: "Players");

        migrationBuilder.DropIndex(
            name: "IX_Players_DisplayName",
            schema: "profiles",
            table: "Players");

        migrationBuilder.DropIndex(
            name: "IX_Customers_UserId",
            schema: "profiles",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            schema: "profiles",
            table: "Customers");

        migrationBuilder.AddColumn<string>(
            name: "Email",
            schema: "profiles",
            table: "Customers",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "profiles",
            table: "Customers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Email",
            schema: "profiles",
            table: "Customers",
            column: "Email",
            unique: true);

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            schema: "profiles",
            table: "Players");

        migrationBuilder.AddColumn<string>(
            name: "Email",
            schema: "profiles",
            table: "Players",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.RenameColumn(
            name: "UserId",
            schema: "profiles",
            table: "Players",
            newName: "OwnerId");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Players",
            schema: "profiles",
            table: "Players");

        migrationBuilder.RenameTable(
            name: "Players",
            schema: "profiles",
            newName: "Associates",
            newSchema: "profiles");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Associates",
            schema: "profiles",
            table: "Associates",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Associates_OwnerId",
            schema: "profiles",
            table: "Associates",
            column: "OwnerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Subscriptions_Associates_SubscribedId",
            schema: "profiles",
            table: "Subscriptions",
            column: "SubscribedId",
            principalSchema: "profiles",
            principalTable: "Associates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
