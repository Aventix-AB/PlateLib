using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class BlobStorageMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "StoredFiles");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "StoredFiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "StoredFiles",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "StoredFiles");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "StoredFiles",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
