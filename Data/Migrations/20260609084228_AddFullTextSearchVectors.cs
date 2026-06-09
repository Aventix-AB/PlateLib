using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchVectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Plates",
                type: "tsvector",
                nullable: false)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "CatalogNumber" });

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Manufacturers",
                type: "tsvector",
                nullable: false)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Plates_SearchVector",
                table: "Plates",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_SearchVector",
                table: "Manufacturers",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Plates_SearchVector",
                table: "Plates");

            migrationBuilder.DropIndex(
                name: "IX_Manufacturers_SearchVector",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Manufacturers");
        }
    }
}
