using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class valoracionesCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Valoraciones_AspNetUsers_usuario_id",
                table: "Valoraciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Valoraciones_AspNetUsers_usuario_id",
                table: "Valoraciones",
                column: "usuario_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Valoraciones_AspNetUsers_usuario_id",
                table: "Valoraciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Valoraciones_AspNetUsers_usuario_id",
                table: "Valoraciones",
                column: "usuario_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
