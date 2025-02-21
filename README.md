# Hiffy API - .NET Core + Entity Framework Core

## Descripción

Esta API está construida en **ASP.NET Core** utilizando **Entity Framework Core** como ORM para interactuar con bases de datos. La API gestiona la lógica de negocio para la plataforma Hiffy, incluyendo la autenticación, la gestión de usuarios, dispositivos y otras funcionalidades relacionadas.

La arquitectura sigue el patrón de capas, separando la lógica de negocio, acceso a datos y controladores de API.

## Tecnologías Utilizadas

- **ASP.NET Core 8.0**: Framework para la construcción de aplicaciones web y APIs.
- **Entity Framework Core**: ORM para interactuar con bases de datos SQL.
- **SQL Server**: Base de datos relacional (configurable para usar otras bases de datos).
- **JWT (JSON Web Tokens)**: Autenticación y autorización mediante tokens.

## Estructura del Proyecto

- **Controllers**: Manejan las solicitudes HTTP y responden a los clientes.
- **Services**: Contienen la lógica de negocio principal.
- **Repositories**: Interactúan con la base de datos usando EF Core.
- **Models**: Definiciones de los objetos de negocio y DTOs.
- **Data**: Configuración de EF Core, incluyendo el contexto de base de datos.

## Funcionalidades

1. **Autenticación y Autorización**: Utiliza JWT para la autenticación segura.
2. **Gestión de Usuarios**: Operaciones CRUD sobre usuarios registrados en la plataforma.
3. **Gestión de Dispositivos**: API para registrar y gestionar dispositivos inteligentes conectados.
4. **Manejo de Tareas**: Permite crear, actualizar y eliminar tareas asociadas a usuarios y dispositivos.

## Requisitos

- **.NET SDK 6.0 o superior**
- **SQL Server** (o cualquier base de datos soportada por EF Core)
- **Azure DevOps** para la integración continua y despliegue (opcional).

 
## Instalación

### 1. Clona el repositorio desde Azure DevOps:
 
git clone https://dev.azure.com/1106127/_git/HIFFY
cd HIFFY
 
### 2. Restaurar las dependencias

dotnet restore

### 3. Configuración de la Base de Datos
Asegúrate de que SQL Server esté instalado y corriendo en tu máquina o en un servidor remoto.
Configura la cadena de conexión en el archivo appsettings.json o como variable de entorno:
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HiffyDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}

### 4. Ejecutar la aplicación
dotnet run
