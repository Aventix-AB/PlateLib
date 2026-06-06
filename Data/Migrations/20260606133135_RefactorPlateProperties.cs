using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPlateProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "Lid",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "Skirt",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "Sterile",
                table: "Plates");

            migrationBuilder.RenameColumn(
                name: "Wellnumber",
                table: "Plates",
                newName: "WellCount");

            migrationBuilder.AlterColumn<string>(
                name: "CatalogNumber",
                table: "Plates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "MaterialId",
                table: "Plates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlateProperties",
                columns: table => new
                {
                    PlateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateProperties", x => new { x.PlateId, x.PropertyDefinitionId });
                    table.ForeignKey(
                        name: "FK_PlateProperties_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlateProperties_PropertyDefinitions_PropertyDefinitionId",
                        column: x => x.PropertyDefinitionId,
                        principalTable: "PropertyDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plates_MaterialId",
                table: "Plates",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Code",
                table: "Materials",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlateProperties_PropertyDefinitionId",
                table: "PlateProperties",
                column: "PropertyDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDefinitions_Name",
                table: "PropertyDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Plates_Materials_MaterialId",
                table: "Plates",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plates_Materials_MaterialId",
                table: "Plates");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "PlateProperties");

            migrationBuilder.DropTable(
                name: "PropertyDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Plates_MaterialId",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "Plates");

            migrationBuilder.RenameColumn(
                name: "WellCount",
                table: "Plates",
                newName: "Wellnumber");

            migrationBuilder.AlterColumn<string>(
                name: "CatalogNumber",
                table: "Plates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "Color",
                table: "Plates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Lid",
                table: "Plates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Material",
                table: "Plates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Skirt",
                table: "Plates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Sterile",
                table: "Plates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
