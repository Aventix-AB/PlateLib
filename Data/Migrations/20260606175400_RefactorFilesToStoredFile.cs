using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFilesToStoredFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old PlateFiles table (direct FK to Plate)
            migrationBuilder.DropTable(
                name: "PlateFiles");

            // Create StoredFiles table
            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileContent = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            // Create the many-to-many join table
            migrationBuilder.CreateTable(
                name: "PlateFiles",
                columns: table => new
                {
                    PlatesId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateFiles", x => new { x.PlatesId, x.FilesId });
                    table.ForeignKey(
                        name: "FK_PlateFiles_Plates_PlatesId",
                        column: x => x.PlatesId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlateFiles_StoredFiles_FilesId",
                        column: x => x.FilesId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlateFiles_FilesId",
                table: "PlateFiles",
                column: "FilesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlateFiles");
            migrationBuilder.DropTable(name: "StoredFiles");

            // Recreate original PlateFiles table
            migrationBuilder.CreateTable(
                name: "PlateFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    PlateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateFiles_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlateFiles_PlateId",
                table: "PlateFiles",
                column: "PlateId");
        }
    }
}
