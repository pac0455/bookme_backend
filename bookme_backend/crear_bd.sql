IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [firebase_uid] nvarchar(255) NULL,
    [Bloqueado] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [categoria] (
    [id] int NOT NULL IDENTITY,
    [categoria] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_categoria] PRIMARY KEY ([id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RolesGlobales] (
    [Id] int NOT NULL IDENTITY,
    [UsuarioId] nvarchar(450) NOT NULL,
    [Rol] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RolesGlobales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RolesGlobales_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [negocios] (
    [id] int NOT NULL IDENTITY,
    [nombre] nvarchar(255) NOT NULL,
    [descripcion] nvarchar(255) NOT NULL,
    [direccion] nvarchar(255) NOT NULL,
    [logo] nvarchar(255) NULL,
    [latitud] float NOT NULL,
    [longitud] float NOT NULL,
    [categoria] int NOT NULL,
    [activo] bit NOT NULL,
    [eliminado] bit NOT NULL,
    [Bloqueado] bit NOT NULL,
    CONSTRAINT [PK_negocios] PRIMARY KEY ([id]),
    CONSTRAINT [FK_negocios_categoria_categoria] FOREIGN KEY ([categoria]) REFERENCES [categoria] ([id]) ON DELETE CASCADE
);

CREATE TABLE [horarios] (
    [Id] int NOT NULL IDENTITY,
    [id_negocio] int NOT NULL,
    [dia_semana] nvarchar(20) NOT NULL,
    [hora_inicio] time NOT NULL,
    [hora_fin] time NOT NULL,
    CONSTRAINT [PK_horarios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_horarios_negocios_id_negocio] FOREIGN KEY ([id_negocio]) REFERENCES [negocios] ([id]) ON DELETE CASCADE
);

CREATE TABLE [servicios] (
    [id] int NOT NULL IDENTITY,
    [negocio_id] int NOT NULL,
    [nombre] nvarchar(255) NULL,
    [descripcion] nvarchar(max) NULL,
    [duracion_minutos] int NOT NULL,
    [precio] decimal(10,2) NOT NULL,
    [imagen_url] nvarchar(500) NULL,
    CONSTRAINT [PK_servicios] PRIMARY KEY ([id]),
    CONSTRAINT [FK_servicios_negocios_negocio_id] FOREIGN KEY ([negocio_id]) REFERENCES [negocios] ([id]) ON DELETE CASCADE
);

CREATE TABLE [suscripciones] (
    [id] int NOT NULL IDENTITY,
    [id_negocio] int NOT NULL,
    [id_usuario] nvarchar(450) NOT NULL,
    [fecha_suscripcion] datetime NOT NULL,
    [rol_negocio] nvarchar(255) NULL,
    CONSTRAINT [PK_suscripciones] PRIMARY KEY ([id]),
    CONSTRAINT [FK_suscripciones_AspNetUsers_id_usuario] FOREIGN KEY ([id_usuario]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_suscripciones_negocios_id_negocio] FOREIGN KEY ([id_negocio]) REFERENCES [negocios] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Valoraciones] (
    [id] int NOT NULL IDENTITY,
    [negocio_id] int NOT NULL,
    [usuario_id] nvarchar(450) NOT NULL,
    [puntuacion] float NOT NULL,
    [comentario] nvarchar(max) NULL,
    [fecha_valoracion] datetime2 NOT NULL,
    CONSTRAINT [PK_Valoraciones] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Valoraciones_AspNetUsers_usuario_id] FOREIGN KEY ([usuario_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Valoraciones_negocios_negocio_id] FOREIGN KEY ([negocio_id]) REFERENCES [negocios] ([id]) ON DELETE CASCADE
);

CREATE TABLE [reservas] (
    [id] int NOT NULL IDENTITY,
    [negocio_id] int NOT NULL,
    [usuario_id] nvarchar(450) NOT NULL,
    [fecha] date NOT NULL,
    [hora_inicio] time NOT NULL,
    [hora_fin] time NOT NULL,
    [estado] nvarchar(255) NOT NULL,
    [fecha_creacion] datetime NULL,
    [cancelacion_motivo] nvarchar(500) NULL,
    [servicio_id] int NOT NULL,
    CONSTRAINT [PK_reservas] PRIMARY KEY ([id]),
    CONSTRAINT [FK_reservas_AspNetUsers_usuario_id] FOREIGN KEY ([usuario_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_reservas_negocios_negocio_id] FOREIGN KEY ([negocio_id]) REFERENCES [negocios] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_reservas_servicios_servicio_id] FOREIGN KEY ([servicio_id]) REFERENCES [servicios] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [pagos] (
    [id] int NOT NULL IDENTITY,
    [reserva_id] int NOT NULL,
    [monto] decimal(10,2) NOT NULL,
    [fecha_pago] datetime NULL,
    [estado_pago] nvarchar(max) NOT NULL,
    [metodo_pago] int NOT NULL,
    [respuesta_pasarela] nvarchar(max) NULL,
    [moneda] nvarchar(10) NULL,
    CONSTRAINT [PK_pagos] PRIMARY KEY ([id]),
    CONSTRAINT [FK_pagos_reservas_reserva_id] FOREIGN KEY ([reserva_id]) REFERENCES [reservas] ([id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'categoria') AND [object_id] = OBJECT_ID(N'[categoria]'))
    SET IDENTITY_INSERT [categoria] ON;
INSERT INTO [categoria] ([id], [categoria])
VALUES (1, N'Clínica'),
(2, N'Tienda'),
(3, N'Gimnasio'),
(4, N'Salón de Belleza'),
(5, N'Veterinaria'),
(6, N'Restaurante'),
(7, N'Cafetería'),
(8, N'Barbería'),
(9, N'Psicología'),
(10, N'Nutrición'),
(11, N'Fisioterapia'),
(12, N'Podología'),
(13, N'Asesoría'),
(14, N'Consultoría'),
(15, N'Servicios Jurídicos'),
(16, N'Clases Particulares'),
(17, N'Academia de Idiomas'),
(18, N'Tatuajes y Piercings'),
(19, N'Centro Estético'),
(20, N'Terapias Alternativas'),
(21, N'Cuidado de Mascotas'),
(22, N'Mecánica'),
(23, N'Electricista'),
(24, N'Fontanero'),
(25, N'Fotografía');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'categoria') AND [object_id] = OBJECT_ID(N'[categoria]'))
    SET IDENTITY_INSERT [categoria] OFF;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UQ__usuarios__AB6E6164E28DE90C] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL;

CREATE UNIQUE INDEX [UQ__usuarios__AB6E6164E28DE90S] ON [AspNetUsers] ([firebase_uid]) WHERE [firebase_uid] IS NOT NULL;

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_horarios_id_negocio] ON [horarios] ([id_negocio]);

CREATE INDEX [IX_negocios_categoria] ON [negocios] ([categoria]);

CREATE UNIQUE INDEX [IX_negocios_nombre] ON [negocios] ([nombre]);

CREATE UNIQUE INDEX [IX_pagos_reserva_id] ON [pagos] ([reserva_id]);

CREATE INDEX [IX_reservas_negocio_id] ON [reservas] ([negocio_id]);

CREATE INDEX [IX_reservas_servicio_id] ON [reservas] ([servicio_id]);

CREATE INDEX [IX_reservas_usuario_id] ON [reservas] ([usuario_id]);

CREATE UNIQUE INDEX [IX_RolesGlobales_UsuarioId_Rol] ON [RolesGlobales] ([UsuarioId], [Rol]);

CREATE INDEX [IX_servicios_negocio_id] ON [servicios] ([negocio_id]);

CREATE INDEX [IX_suscripciones_id_negocio] ON [suscripciones] ([id_negocio]);

CREATE INDEX [IX_suscripciones_id_usuario] ON [suscripciones] ([id_usuario]);

CREATE INDEX [IX_Valoraciones_negocio_id] ON [Valoraciones] ([negocio_id]);

CREATE INDEX [IX_Valoraciones_usuario_id] ON [Valoraciones] ([usuario_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250613230809_init', N'9.0.4');

COMMIT;
GO

