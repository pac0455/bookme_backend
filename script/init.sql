IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Bookme')
BEGIN
  CREATE DATABASE Bookme;
END
GO

USE Bookme;
GO

CREATE TABLE usuarios (
  id INT PRIMARY KEY IDENTITY(1,1),
  nombre NVARCHAR(255),
  email NVARCHAR(255) UNIQUE,
  telefono NVARCHAR(255),
  contrasena_hash NVARCHAR(255),
  firebase_uid NVARCHAR(255),
  rol NVARCHAR(255),
  fecha_registro DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE negocios (
  id INT PRIMARY KEY IDENTITY(1,1),
  nombre NVARCHAR(255),
  descripcion NVARCHAR(255),
  direccion NVARCHAR(255),
  latitud FLOAT,
  longitud FLOAT,
  categoria NVARCHAR(255),
  horario_atencion NVARCHAR(MAX),
  activo BIT
);
GO

CREATE TABLE suscripciones (
  id INT PRIMARY KEY IDENTITY(1,1),
  id_negocio INT NOT NULL,
  id_usuario INT NOT NULL,
  fecha_suscripcion DATETIME NOT NULL,
  rol_negocio NVARCHAR(255)
);
GO

CREATE TABLE reservas (
  id INT PRIMARY KEY IDENTITY(1,1),
  negocio_id INT NOT NULL,
  usuario_id INT NOT NULL,
  fecha DATE,
  hora_inicio TIME,
  hora_fin TIME,
  estado NVARCHAR(255),
  comentario_cliente NVARCHAR(MAX),
  fecha_creacion DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE valoraciones (
  id INT PRIMARY KEY IDENTITY(1,1),
  reserva_id INT NOT NULL,
  usuario_id INT NOT NULL,
  puntuacion INT,
  comentario NVARCHAR(MAX),
  fecha_valoracion DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE servicios (
  id INT PRIMARY KEY IDENTITY(1,1),
  negocio_id INT NOT NULL,
  nombre NVARCHAR(255),
  descripcion NVARCHAR(MAX),
  duracion_minutos INT,
  precio DECIMAL(10, 2)
);
GO

CREATE TABLE reservas_servicios (
  id INT PRIMARY KEY IDENTITY(1,1),
  reserva_id INT NOT NULL,
  servicio_id INT NOT NULL
);
GO

CREATE TABLE pagos (
  id INT PRIMARY KEY IDENTITY(1,1),
  reserva_id INT NOT NULL,
  monto DECIMAL(10, 2),
  fecha_pago DATETIME DEFAULT GETDATE(),
  estado_pago NVARCHAR(255) NOT NULL,
  metodo_pago NVARCHAR(255) NOT NULL,
  id_transaccion_externa NVARCHAR(255),
  respuesta_pasarela NVARCHAR(MAX),
  moneda NVARCHAR(10) DEFAULT 'EUR',
  reembolsado BIT DEFAULT 0,
  creado DATETIME DEFAULT GETDATE(),
  actualizado DATETIME
);
GO

ALTER TABLE suscripciones ADD FOREIGN KEY (id_negocio) REFERENCES negocios (id);
ALTER TABLE suscripciones ADD FOREIGN KEY (id_usuario) REFERENCES usuarios (id);
ALTER TABLE reservas ADD FOREIGN KEY (negocio_id) REFERENCES negocios (id);
ALTER TABLE reservas ADD FOREIGN KEY (usuario_id) REFERENCES usuarios (id);
ALTER TABLE valoraciones ADD FOREIGN KEY (reserva_id) REFERENCES reservas (id);
ALTER TABLE valoraciones ADD FOREIGN KEY (usuario_id) REFERENCES usuarios (id);
ALTER TABLE servicios ADD FOREIGN KEY (negocio_id) REFERENCES negocios (id);
ALTER TABLE reservas_servicios ADD FOREIGN KEY (reserva_id) REFERENCES reservas (id);
ALTER TABLE reservas_servicios ADD FOREIGN KEY (servicio_id) REFERENCES servicios (id);
ALTER TABLE pagos ADD FOREIGN KEY (reserva_id) REFERENCES reservas (id);
GO