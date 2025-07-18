﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace bookme_backend.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    firebase_uid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Bloqueado = table.Column<bool>(type: "bit", maxLength: 255, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolesGlobales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesGlobales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesGlobales_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "negocios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    latitud = table.Column<double>(type: "float", nullable: false),
                    longitud = table.Column<double>(type: "float", nullable: false),
                    categoria = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    eliminado = table.Column<bool>(type: "bit", nullable: false),
                    Bloqueado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_negocios", x => x.id);
                    table.ForeignKey(
                        name: "FK_negocios_categoria_categoria",
                        column: x => x.categoria,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_negocio = table.Column<int>(type: "int", nullable: false),
                    dia_semana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_horarios_negocios_id_negocio",
                        column: x => x.id_negocio,
                        principalTable: "negocios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    negocio_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duracion_minutos = table.Column<int>(type: "int", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    imagen_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.id);
                    table.ForeignKey(
                        name: "FK_servicios_negocios_negocio_id",
                        column: x => x.negocio_id,
                        principalTable: "negocios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "suscripciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_negocio = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    fecha_suscripcion = table.Column<DateTime>(type: "datetime", nullable: false),
                    rol_negocio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_suscripciones_AspNetUsers_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_suscripciones_negocios_id_negocio",
                        column: x => x.id_negocio,
                        principalTable: "negocios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Valoraciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    negocio_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    puntuacion = table.Column<double>(type: "float", nullable: false),
                    comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_valoracion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valoraciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_Valoraciones_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Valoraciones_negocios_negocio_id",
                        column: x => x.negocio_id,
                        principalTable: "negocios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    negocio_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    cancelacion_motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    servicio_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservas_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservas_negocios_negocio_id",
                        column: x => x.negocio_id,
                        principalTable: "negocios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservas_servicios_servicio_id",
                        column: x => x.servicio_id,
                        principalTable: "servicios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reserva_id = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado_pago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    metodo_pago = table.Column<int>(type: "int", nullable: false),
                    respuesta_pasarela = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagos_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    { 13, "Asesoría" },
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
                    { 25, "Fotografía" },
                    { 26, "Otros" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UQ__usuarios__AB6E6164E28DE90C",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__usuarios__AB6E6164E28DE90S",
                table: "AspNetUsers",
                column: "firebase_uid",
                unique: true,
                filter: "[firebase_uid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_horarios_id_negocio",
                table: "horarios",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_negocios_categoria",
                table: "negocios",
                column: "categoria");

            migrationBuilder.CreateIndex(
                name: "IX_negocios_nombre",
                table: "negocios",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reserva_id",
                table: "pagos",
                column: "reserva_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_negocio_id",
                table: "reservas",
                column: "negocio_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_servicio_id",
                table: "reservas",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_usuario_id",
                table: "reservas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolesGlobales_UsuarioId_Rol",
                table: "RolesGlobales",
                columns: new[] { "UsuarioId", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_servicios_negocio_id",
                table: "servicios",
                column: "negocio_id");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_id_negocio",
                table: "suscripciones",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_id_usuario",
                table: "suscripciones",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_negocio_id",
                table: "Valoraciones",
                column: "negocio_id");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_usuario_id",
                table: "Valoraciones",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "horarios");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "RolesGlobales");

            migrationBuilder.DropTable(
                name: "suscripciones");

            migrationBuilder.DropTable(
                name: "Valoraciones");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "negocios");

            migrationBuilder.DropTable(
                name: "categoria");
        }
    }
}
