using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class imageLastUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "imagen_updated_at",
                table: "servicios",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "logo_updated_at",
                table: "negocios",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagen_updated_at",
                table: "servicios");

            migrationBuilder.DropColumn(
                name: "logo_updated_at",
                table: "negocios");
        }
    }
}
