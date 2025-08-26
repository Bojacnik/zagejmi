using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AggregateId",
                table: "StoredEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserProjections_Email",
                table: "UserProjections",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AggregateId",
                table: "StoredEvents",
                column: "AggregateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProjections_Email",
                table: "UserProjections");

            migrationBuilder.DropIndex(
                name: "IX_AggregateId",
                table: "StoredEvents");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "StoredEvents");
        }
    }
}
