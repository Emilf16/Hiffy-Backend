-- Crear la base de datos HiffyDB
CREATE DATABASE HiffyDB;
GO

-- Seleccionar la base de datos HiffyDB
USE HiffyDB;
GO
-- Crear la tabla Estado
CREATE TABLE EstadoFamilia(
    IdEstadoFamilia INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion NVARCHAR(50) NOT NULL,
    Activo BIT NOT NULL DEFAULT 0, 
    Inactivo BIT NOT NULL DEFAULT 0,
    Suspendida BIT NOT NULL DEFAULT 0, 
    PendienteFamilia BIT NOT NULL DEFAULT 0, 
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);
-- Crear la tabla Estado
CREATE TABLE EstadoVendedor(
    IdEstadoVendedor INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion NVARCHAR(50) NOT NULL,
    Activo BIT NOT NULL DEFAULT 0, 
    Inactivo BIT NOT NULL DEFAULT 0,
    Suspendida BIT NOT NULL DEFAULT 0, 
    PendienteValidacion BIT NOT NULL DEFAULT 0, 
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

-- Crear la tabla Familia
CREATE TABLE Familia (
    IdFamilia INT IDENTITY(1,1) PRIMARY KEY,
    CodigoFamilia CHAR(8) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(255) NULL,
    Altitud NVARCHAR(30) NULL,
    Longitud NVARCHAR(30) NULL,
    FechaCreacion DATE NOT NULL DEFAULT GETDATE()
);

-- Crear la tabla RolFamilia
CREATE TABLE RolFamilia (
    IdRolFamilia INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(30) NOT NULL,
    Descripcion VARCHAR(250) NULL,
    EsAdmin BIT NOT NULL DEFAULT 0
);

-- Crear la tabla Rol
CREATE TABLE Rol (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(30) NOT NULL,
    Descripcion VARCHAR(250) NULL,
    EsVendedor BIT NOT NULL DEFAULT 0,
    EsAdmin BIT NOT NULL DEFAULT 0,
    EsUsuarioFamilia BIT NOT NULL DEFAULT 0,
    EsAmbos BIT NOT NULL DEFAULT 0
);
CREATE TABLE TipoDocumento (
    IdTipoDocumento INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL -- Ejemplo: "Cédula", "Pasaporte", etc.
);
-- Crear la tabla Usuario
CREATE TABLE Usuario (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(150) NOT NULL UNIQUE,
    Contraseña VARCHAR(255) NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    FechaNacimiento DATE NOT NULL,
    Sexo CHAR(1) NOT NULL,
    IdEstadoFamilia INT NULL,
    IdEstadoVendedor INT NULL,
    IdRol INT NOT NULL,
    IdRolFamilia INT NULL,
    IdFamilia INT NULL, -- Permitir NULL para usuarios sin familia
    Descripcion VARCHAR(150) NULL,
    Valoracion DECIMAL(1,1) NULL,
    CodigoVerificacion VARCHAR(4) NULL,
    FechaLimiteCodigo DATETIME NULL,
    FotoUrl NVARCHAR(255) NULL,
    IdTipoDocumento INT NULL, -- FK para el tipo de documento (Cédula, Pasaporte, etc.)
    Documento VARCHAR(20) NOT NULL, -- Número de documento (Cédula o Pasaporte)
    Altitud NVARCHAR(30) NULL,-- Altitud de vendedores
    Longitud NVARCHAR(30) NULL,-- Longitud de vendedores
    CONSTRAINT FK_Usuario_EstadoFamilia FOREIGN KEY (IdEstadoFamilia) REFERENCES EstadoFamilia(IdEstadoFamilia),
    CONSTRAINT FK_Usuario_EstadoVendedor FOREIGN KEY (IdEstadoVendedor) REFERENCES EstadoVendedor(IdEstadoVendedor),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES Rol(IdRol),
    CONSTRAINT FK_Usuario_Familia FOREIGN KEY (IdFamilia) REFERENCES Familia(IdFamilia),
    CONSTRAINT FK_Usuario_RolFamilia FOREIGN KEY (IdRolFamilia) REFERENCES RolFamilia(IdRolFamilia),
    CONSTRAINT FK_Usuario_TipoDocumento FOREIGN KEY (IdTipoDocumento) REFERENCES TipoDocumento(IdTipoDocumento)
);

ALTER TABLE Usuario
ALTER COLUMN Valoracion DECIMAL(3,1) NULL;


-- Crear la tabla TipoTarea
CREATE TABLE TipoTarea (
    IdTipoTarea INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(30) NOT NULL,
    Descripcion VARCHAR(200)
);


-- Crear la tabla TareaDomestica
CREATE TABLE EstadoTareas (
    IdEstadoTarea INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstado VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200)
);

CREATE TABLE TareaDomestica (
    IdTareaDomestica INT IDENTITY(1,1) PRIMARY KEY,
    IdFamilia INT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255) NULL,
    IdTipoTarea INT NOT NULL,
    Predeterminado BIT NOT NULL,
    IdEstadoTarea INT NOT NULL DEFAULT 1, -- Estado por defecto "Pendiente"
    FOREIGN KEY (IdFamilia) REFERENCES Familia(IdFamilia),
    FOREIGN KEY (IdTipoTarea) REFERENCES TipoTarea(IdTipoTarea),
    FOREIGN KEY (IdEstadoTarea) REFERENCES EstadoTareas(IdEstadoTarea)
);

-- Crear la tabla EstadoAreas
CREATE TABLE EstadoAreasDelHogar (
    IdEstadoAreasDelHogar INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstado VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200)
);

-- Crear la tabla AreaDelHogar_Familia
CREATE TABLE AreaDelHogar_Familia (
    IdAreaFamilia INT IDENTITY(1,1) PRIMARY KEY,
    IdFamilia INT  NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion VARCHAR(200),
    Predeterminado BIT NOT NULL,
	IdEstadoAreasDelHogar INT  NULL,
    FOREIGN KEY (IdFamilia) REFERENCES Familia(IdFamilia),
	FOREIGN KEY (IdEstadoAreasDelHogar) REFERENCES EstadoAreasDelHogar(IdEstadoAreasDelHogar)
);

-- Crear la tabla TareaAsignada
CREATE TABLE TareaAsignada (
    IdTareaAsignada INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
	IdTareaDomestica int not null,
	IdAreaFamilia int not null,
    Descripcion NVARCHAR(255) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL, 
    Prioridad NVARCHAR(50) NOT NULL,
	EsRecurrente BIT NOT NULL,
    HoraInicio DATETIME NOT NULL,
    HoraFin DATETIME NOT NULL,
    DiaSemana INT NULL,
    Estado INT NOT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
	FOREIGN KEY (IdTareaDomestica) REFERENCES TareaDomestica(IdTareaDomestica)
);

-- Crear la tabla RecurrenciaTareas
CREATE TABLE RecurrenciaTareas (
    IdRecurrencia INT IDENTITY(1,1) PRIMARY KEY,
    IdTareaAsignada INT NOT NULL,
    FechaDia DATE NOT NULL,
    Estado INT NOT NULL,
    FOREIGN KEY (IdTareaAsignada) REFERENCES TareaAsignada(IdTareaAsignada)
);
 
-- Crear la tabla Menu
CREATE TABLE Menu (
    IdMenu INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Icono VARCHAR(50) NOT NULL,
    Url VARCHAR(100) NOT NULL
);

-- Crear la tabla Menu_Rol
CREATE TABLE Menu_Rol (
    IdMenuRol INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    IdMenu INT NOT NULL,
    FOREIGN KEY (IdRol) REFERENCES Rol(IdRol),
    FOREIGN KEY (IdMenu) REFERENCES Menu(IdMenu)
);
 

-- Crear la tabla DispositivoFamilia
CREATE TABLE DispositivoFamilia (
    IdDispositivoFamilia INT IDENTITY(1,1) PRIMARY KEY,
    IdDispositivo NVARCHAR(255) NOT NULL,
    NombreDispositivo NVARCHAR(50) NOT NULL,
    IdAreaFamilia INT NOT NULL,
    Estado INT NOT NULL, 
    FOREIGN KEY (IdAreaFamilia) REFERENCES AreaDelHogar_Familia(IdAreaFamilia)
);

-- Crear la tabla Notificacion
CREATE TABLE Notificacion (
    IdNotificacion INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(100) NOT NULL,
    Mensaje NVARCHAR(1000) NOT NULL,
    IdUsuarioDestino INT NOT NULL,
    FechaEnvio DATETIME NOT NULL,
    Estado NVARCHAR(10) NOT NULL,
    FOREIGN KEY (IdUsuarioDestino) REFERENCES Usuario(IdUsuario)
);
 



CREATE TABLE TareasDesactivadas (
    IdTareaDesactivada INT IDENTITY(1,1) PRIMARY KEY,
    IdFamilia INT NOT NULL,
    IdTareaDomestica INT NOT NULL,
	IdEstadoTarea INT not null default 1,
	FOREIGN KEY (IdEstadoTarea) REFERENCES EstadoTareas(IdEstadoTarea),
    FOREIGN KEY (IdFamilia) REFERENCES Familia(IdFamilia),
    FOREIGN KEY (IdTareaDomestica) REFERENCES TareaDomestica(IdTareaDomestica)
);

--TABLAS PARA VENDEDORES 
-- Crear la tabla CertificacionVendedor
CREATE TABLE CertificacionVendedor (
    IdCertificacion INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL, -- Relacionado con el vendedor (usuario)
    Nombre NVARCHAR(100) NOT NULL, -- Descripción de la certificación
    Descripcion NVARCHAR(255) NOT NULL, -- Descripción de la certificación
    UrlArchivo NVARCHAR(500) NOT NULL, -- URL del archivo alojado en el API
    FechaCertificacion DATE NOT NULL DEFAULT GETDATE(), -- Fecha de la certificación
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario) -- Llave foránea
);



CREATE TABLE TipoServicio (
    IdTipoServicio INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255) NULL
);

CREATE TABLE CertificacionTipoServicio (
    IdCertificacionTipoServicio INT IDENTITY(1,1) PRIMARY KEY,
    IdCertificacion INT NOT NULL, -- Relaciona con la certificación del vendedor
    IdTipoServicio INT NOT NULL, -- Relaciona con el tipo de servicio habilitado
    FOREIGN KEY (IdCertificacion) REFERENCES CertificacionVendedor(IdCertificacion),
    FOREIGN KEY (IdTipoServicio) REFERENCES TipoServicio(IdTipoServicio)
);

CREATE TABLE Servicio (
    IdServicio INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL, -- Vendedor que oferta el servicio
    IdTipoServicio INT NOT NULL, -- Tipo de servicio ofertado
    Nombre NVARCHAR(100) NOT NULL, -- Nombre del servicio ofertado
    Descripcion NVARCHAR(255) NULL, -- Descripción del servicio
    Precio DECIMAL(10,2) NOT NULL, -- Precio del servicio
    Disponibilidad NVARCHAR(50) NOT NULL, -- Ejemplo: "Lunes a viernes, 8 AM - 6 PM"
    FechaPublicacion DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    FOREIGN KEY (IdTipoServicio) REFERENCES TipoServicio(IdTipoServicio)
);
-- Crear la tabla ContratoPersonal
CREATE TABLE ContratoPersonal (
    IdContrato INT IDENTITY(1,1) PRIMARY KEY,
    IdFamilia INT NOT NULL, -- Familia que contrata el servicio 
    IdServicioContratado INT NOT NULL, -- Relación con el servicio contratado
    CodigoVerificacion INT, -- Código para verificar el contrato
    CodigoFinalizacion INT, -- Código para finalizar el contrato
    FechaInicio DATETIME NOT NULL, -- Fecha de inicio del contrato
    FechaFin DATETIME NOT NULL, -- Fecha de fin del contrato
    Estado INT NOT NULL, -- Estado del contrato (Ejemplo: "Activo", "Finalizado")
    Valoracion INT NOT NULL, -- Valoración dada al servicio
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(), -- Fecha de registro con valor predeterminado
    MotivoCancelacion VARCHAR(100) NULL, -- MotivoCancelacion del servicio
    FOREIGN KEY (IdFamilia) REFERENCES Familia(IdFamilia),
    FOREIGN KEY (IdServicioContratado) REFERENCES Servicio(IdServicio)
);

ALTER TABLE ContratoPersonal
ALTER COLUMN Valoracion INT NULL;

-- Insetar tipo de servicios la tabla TipoServicio
INSERT INTO TipoServicio (Nombre, Descripcion) 
VALUES 
    ('Limpieza', 'Servicios de limpieza para hogares y oficinas.'),
    ('Jardinería', 'Mantenimiento de jardines y áreas verdes.'),
    ('Plomería', 'Reparaciones e instalaciones de sistemas de agua.'),
    ('Electricidad', 'Instalaciones eléctricas y reparaciones.'),
    ('Pintura', 'Servicios de pintura para interiores y exteriores.'),
    ('Carpintería', 'Diseño, reparación y mantenimiento de muebles.'),
    ('Mecánica básica', 'Reparación básica de vehículos.'),
    ('Albañilería', 'Construcción y reparaciones en albañilería.'),
    ('Cuidado de niños', 'Servicios de niñera y cuidado infantil.'),
    ('Cuidado de adultos mayores', 'Asistencia y cuidado para personas mayores.'),
    ('Mudanza', 'Servicios de transporte y mudanza de muebles y objetos.'),
    ('Reparación de electrodomésticos', 'Mantenimiento y reparación de electrodomésticos del hogar.');

-- Insertar roles en la tabla RolFamilia
INSERT INTO RolFamilia(Nombre, Descripcion, EsAdmin)
VALUES
('Padre','Padre de la familia',1),
('Madre','Madre de la familia',1),
('Tutor','Tutor de la familia',1),
('Hijo','Hijo de la familia',0),
('Hija','Hija de la familia',0);

-- Insertar los estados en la tabla Estado
INSERT INTO EstadoFamilia (Descripcion, Activo, Inactivo, Suspendida,PendienteFamilia)
VALUES
('Activo', 1, 0, 0,0), 
('Inactivo', 0,  1, 0,0),
('Pendiente Familia', 0,  0, 0, 1),
('Suspendida', 0,  0, 1,0)

INSERT INTO EstadoVendedor (Descripcion, Activo, Inactivo, Suspendida,PendienteValidacion)
VALUES
('Activo', 1, 0, 0, 0), 
('Inactivo', 0,  1, 0, 0),
('Pendiente Validación', 0,  0, 0, 1),
('Suspendida', 0,  0, 1, 0)

-- Insertar roles en la tabla Rol
INSERT INTO Rol (Nombre, Descripcion, EsVendedor, EsAdmin, EsUsuarioFamilia, EsAmbos)
VALUES
('Admin', 'Rol para administrar el sistema', 0, 1, 0, 0),
('Vendedor', 'Rol para gestionar ventas', 1, 0, 0, 0),
('Familiar', 'Rol para los usuarios que pertenecen a una familia', 0, 0, 1, 0),
('Vendedor y Familiar', 'Rol que permite ser vendedor y familiar', 0, 0, 0, 1);

INSERT INTO Familia (CodigoFamilia, Nombre, Direccion, Altitud, Longitud)
VALUES ('ADM12345', 'Familia Administradora', '1234 Calle Principal, Ciudad, País', 40.712776, -74.005974);

INSERT INTO EstadoAreasDelHogar (NombreEstado, Descripcion)
VALUES 
('Activo', 'Área del hogar actualmente activa'),
('Inactivo', 'Área del hogar actualmente inactiva');

INSERT INTO AreaDelHogar_Familia (IdFamilia, Nombre, Descripcion, Predeterminado,IdEstadoAreasDelHogar) 
VALUES 
(NULL, 'Cocina', 'Área destinada a la preparación de alimentos', 1,1), 
(NULL, 'Sala de Estar', 'Espacio destinado para recibir visitas y relajarse', 1,1),
(NULL, 'Comedor', 'Área destinada a comer', 1,1),
(NULL, 'Dormitorio Principal', 'Habitación principal del hogar', 1,1),
(NULL, 'Baño Principal', 'Baño principal de la casa', 1,1),
(NULL, 'Oficina', 'Espacio destinado al trabajo o estudio en casa', 1,1),
(NULL, 'Garaje', 'Área destinada al estacionamiento de vehículos', 1,1),
(NULL, 'Patio', 'Espacio exterior para actividades recreativas', 1,1),
(NULL, 'Cuarto de Lavado', 'Área destinada para lavar la ropa', 1,1),
(NULL, 'Almacén', 'Espacio destinado al almacenamiento de objetos', 1,1);

INSERT INTO TipoTarea (Nombre, Descripcion)
VALUES 
('Limpieza General', 'Limpiar todas las áreas de la casa'),
('Lavandería', 'Lavar y secar la ropa'),
('Planchar', 'Planchar ropa después del lavado'),
('Cuidado de Mascotas', 'Alimentar y cuidar de las mascotas'),
('Jardinería', 'Cuidado de plantas y jardín');

INSERT INTO EstadoTareas (NombreEstado, Descripcion) VALUES 
('Activo', 'La tarea está activa para realizar'),
('Desactivada', 'La tarea está actualmente desactivada');

INSERT INTO EstadoAreasDelHogar (NombreEstado, Descripcion) VALUES 
('Activo', 'El area está activa para realizar'),
('Desactivada', 'El area está actualmente desactivada');

-- Insertar tareas domésticas para el tipo 'Limpieza General'
INSERT INTO TareaDomestica (IdFamilia, Nombre, Descripcion, IdTipoTarea, Predeterminado)
VALUES 
(NULL, 'Limpieza de Sala', 'Limpiar toda la sala de estar', 1, 1),
(NULL, 'Limpieza de Cocina', 'Limpiar la cocina completamente', 1, 1),
(NULL, 'Limpieza de Baños', 'Limpiar y desinfectar los baños', 1, 1);

-- Insertar tareas domésticas para el tipo 'Lavandería'
INSERT INTO TareaDomestica (IdFamilia, Nombre, Descripcion, IdTipoTarea, Predeterminado)
VALUES 
(NULL, 'Lavar Ropa Blanca', 'Lavar toda la ropa blanca', 2, 1),
(NULL, 'Lavar Ropa de Color', 'Lavar la ropa de color', 2, 1),
(NULL, 'Secar Ropa', 'Secar toda la ropa lavada', 2, 1);

-- Insertar tareas domésticas para el tipo 'Planchar'
INSERT INTO TareaDomestica (IdFamilia, Nombre, Descripcion, IdTipoTarea, Predeterminado)
VALUES 
(NULL, 'Planchar Camisas', 'Planchar todas las camisas de la familia', 3, 1),
(NULL, 'Planchar Pantalones', 'Planchar los pantalones después del lavado', 3, 1),
(NULL, 'Planchar Ropa de Cama', 'Planchar las sábanas y cobijas', 3, 1);

-- Insertar tareas domésticas para el tipo 'Cuidado de Mascotas'
INSERT INTO TareaDomestica (IdFamilia, Nombre, Descripcion, IdTipoTarea, Predeterminado)
VALUES 
(NULL, 'Alimentar Perros', 'Dar comida a los perros', 4, 1),
(NULL, 'Sacar a pasear Perros', 'Sacar a pasear los perros', 4, 1),
(NULL, 'Limpieza de Jaula de Pájaros', 'Limpiar la jaula de los pájaros', 4, 1);

-- Insertar tareas domésticas para el tipo 'Jardinería'
INSERT INTO TareaDomestica (IdFamilia, Nombre, Descripcion, IdTipoTarea, Predeterminado)
VALUES 
(NULL, 'Podar el Césped', 'Cortar y podar el césped del jardín', 5, 1),
(NULL, 'Regar las Plantas', 'Regar todas las plantas del jardín', 5, 1),
(NULL, 'Limpiar las Hojas Secas', 'Recolectar y limpiar las hojas secas del jardín', 5, 1);

-- Insertar tipos de documentos 
INSERT INTO TipoDocumento (Nombre) 
VALUES  
('Cédula'),
('Pasaporte'),
('Licencia de Conducir'),
('Tarjeta de Residencia'); 

-- Generar usuarios base del sistema
SET IDENTITY_INSERT [dbo].[Usuario] ON 
GO
INSERT [dbo].[Usuario] ([IdUsuario], [Nombre], [Correo], [Contraseña], [FechaRegistro], [FechaNacimiento], [Sexo], [IdEstadoFamilia], [IdEstadoVendedor], [IdRol], [IdRolFamilia], [IdFamilia], [Descripcion], [Valoracion], [CodigoVerificacion], [FechaLimiteCodigo], [FotoUrl], [IdTipoDocumento], [Documento]) 
VALUES (1, N'Emil  Solano', N'pedroflorian884@gmail.com', N'$2a$11$m6LrRSBUAw3onZlWF3eKEe669OrXESvIABjTaMlxKyELLc64BWyvC', CAST(N'2025-01-04T18:54:41.010' AS DateTime), CAST(N'2003-02-12' AS Date), N'M', 2, 1, 1, NULL, NULL, N'', NULL, NULL, NULL, N'/uploads/3_0f075ab7-2573-4106-a895-c4df98663971.png', 1, N'212121212121')
GO
INSERT [dbo].[Usuario] ([IdUsuario], [Nombre], [Correo], [Contraseña], [FechaRegistro], [FechaNacimiento], [Sexo], [IdEstadoFamilia], [IdEstadoVendedor], [IdRol], [IdRolFamilia], [IdFamilia], [Descripcion], [Valoracion], [CodigoVerificacion], [FechaLimiteCodigo], [FotoUrl], [IdTipoDocumento], [Documento])
VALUES (2, N'Sebastian Peralta', N'wilbert45leon@gmail.com', N'$2a$11$m6LrRSBUAw3onZlWF3eKEe669OrXESvIABjTaMlxKyELLc64BWyvC', CAST(N'2025-01-05T15:25:08.207' AS DateTime), CAST(N'2017-01-05' AS Date), N'M', 2, 3, 2, null, null, N'', NULL, NULL, NULL, N'/uploads/4_80ad86de-9f52-4933-8cbf-c8312b1061ec.png', NULL, N'')
GO
INSERT [dbo].[Usuario] ([IdUsuario], [Nombre], [Correo], [Contraseña], [FechaRegistro], [FechaNacimiento], [Sexo], [IdEstadoFamilia], [IdEstadoVendedor], [IdRol], [IdRolFamilia], [IdFamilia], [Descripcion], [Valoracion], [CodigoVerificacion], [FechaLimiteCodigo], [FotoUrl], [IdTipoDocumento], [Documento]) 
VALUES (3, N'Pedro Florian', N'rdzseba@gmail.com', N'$2a$11$m6LrRSBUAw3onZlWF3eKEe669OrXESvIABjTaMlxKyELLc64BWyvC', CAST(N'2025-01-06T12:35:21.880' AS DateTime), CAST(N'2003-07-20' AS Date), N'M', 3, 2, 3, 1, null, N'', NULL, NULL, NULL, NULL, 1, N'40212333724')
GO
SET IDENTITY_INSERT [dbo].[Usuario] OFF
GO