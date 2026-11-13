using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zagejmi.Infrastructure.Migrations.BlobImages;

/// <inheritdoc />
public partial class _20261111141645_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "image_blobs");

        migrationBuilder.CreateTable(
            name: "ImageBlobs",
            schema: "image_blobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BlobKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Sha256Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImageBlobs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ImageBlobs_BlobKey",
            schema: "image_blobs",
            table: "ImageBlobs",
            column: "BlobKey",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ImageBlobs",
            schema: "image_blobs");
    }
}
