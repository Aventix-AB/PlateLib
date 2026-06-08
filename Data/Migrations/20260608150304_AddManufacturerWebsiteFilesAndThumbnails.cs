using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerWebsiteFilesAndThumbnails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailStorageKey",
                table: "Plates",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailStorageKey",
                table: "Manufacturers",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Manufacturers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileManufacturer",
                columns: table => new
                {
                    FilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManufacturersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileManufacturer", x => new { x.FilesId, x.ManufacturersId });
                    table.ForeignKey(
                        name: "FK_FileManufacturer_Files_FilesId",
                        column: x => x.FilesId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileManufacturer_Manufacturers_ManufacturersId",
                        column: x => x.ManufacturersId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileManufacturer_ManufacturersId",
                table: "FileManufacturer",
                column: "ManufacturersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileManufacturer");

            migrationBuilder.DropColumn(
                name: "ThumbnailStorageKey",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "ThumbnailStorageKey",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Manufacturers");
        }
    }
}
