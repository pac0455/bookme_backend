using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class seedCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "categoria",
                columns: new[] { "id", "categoria" },
                values: new object[,]
                {
                    { 1, "Clínica" },
                    { 2, "Tienda" },
                    { 3, "Gimnasio" },
                    { 4, "Salón de Belleza" },
                    { 5, "Veterinaria" },
                    { 6, "Restaurante" },
                    { 7, "Cafetería" },
                    { 8, "Barbería" },
                    { 9, "Psicología" },
                    { 10, "Nutrición" },
                    { 11, "Fisioterapia" },
                    { 12, "Podología" },
                    { 14, "Consultoría" },
                    { 15, "Servicios Jurídicos" },
                    { 16, "Clases Particulares" },
                    { 17, "Academia de Idiomas" },
                    { 18, "Tatuajes y Piercings" },
                    { 19, "Centro Estético" },
                    { 20, "Terapias Alternativas" },
                    { 21, "Cuidado de Mascotas" },
                    { 22, "Mecánica" },
                    { 23, "Electricista" },
                    { 24, "Fontanero" },
                    { 25, "Fotografía" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "categoria",
                keyColumn: "id",
                keyValue: 25);
        }
    }
}