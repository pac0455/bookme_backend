using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class negocioConNombre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_negocios_nombre",
                table: "negocios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_negocios_nombre",
                table: "negocios",
                column: "nombre",
                unique: true);
        }
    }
}
