using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "categoria",
                table: "negocios",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "categoria",
                columns: new[] { "id", "categoria" },
                values: new object[,]
                {
                    { 1, "Sin categoría" },
                    { 2, "Gimnasio" },
                    { 3, "Salón de belleza" },
                    { 4, "Barbería" },
                    { 5, "Clínica dental" },
                    { 6, "Spa" },
                    { 7, "Fisioterapia" },
                    { 8, "Psicología" },
                    { 9, "Nutrición" },
                    { 10, "Entrenador personal" },
                    { 11, "Veterinaria" },
                    { 12, "Tatuajes" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_negocios_categoria",
                table: "negocios",
                column: "categoria");

            migrationBuilder.AddForeignKey(
                name: "FK_negocios_categoria_categoria",
                table: "negocios",
                column: "categoria",
                principalTable: "categoria",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_negocios_categoria_categoria",
                table: "negocios");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropIndex(
                name: "IX_negocios_categoria",
                table: "negocios");

            migrationBuilder.AlterColumn<string>(
                name: "categoria",
                table: "negocios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
