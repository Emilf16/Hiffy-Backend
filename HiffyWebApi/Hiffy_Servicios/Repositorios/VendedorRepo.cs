using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Repositorios
{
    public class VendedorRepo  
    {

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        private readonly FirebaseTranslationService _firebasetranslate;


        public VendedorRepo(AppDbContext context, INotificationService notificationService, IEmailService emailService, IConfiguration configuration, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _notificationService = notificationService;

            _emailService = emailService;
            _configuration = configuration;
            _firebasetranslate = firebasetranslate;

        }


        public async Task<OperationResult> SubirCertificacionesAsync(int usuarioId, List<PostCertificacionDto> certificacionDtos, string lenguaje = "es")
        {
            if (certificacionDtos == null || certificacionDtos.Count == 0)
            {
                var mensaje = _firebasetranslate.Traducir("No se han cargado archivos.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var urlsArchivos = new List<string>();

            try
            {
                // Crear ruta de almacenamiento en el servidor
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/certificaciones");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var certificacionDto in certificacionDtos)
                {
                    if (certificacionDto.Archivo == null || certificacionDto.Archivo.Length == 0)
                    {
                        var mensaje = _firebasetranslate.Traducir("Uno o más archivos no se han cargado.", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    // Crear el nombre del archivo
                    var fileName = $"{Guid.NewGuid()}_{certificacionDto.Archivo.FileName}";
                    var fullPath = Path.Combine(folderPath, fileName);

                    // Guardar archivo en el sistema de archivos
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await certificacionDto.Archivo.CopyToAsync(stream);
                    }

                    // Crear objeto CertificacionVendedor y guardar en la base de datos
                    var certificacion = new CertificacionVendedor  
                    {
                        IdUsuario = usuarioId,
                        Nombre = certificacionDto.Archivo.FileName,
                        Descripcion = certificacionDto.Descripcion,
                        UrlArchivo = Path.Combine("certificaciones", fileName),
                        FechaCertificacion = certificacionDto.FechaCertificacion,
                        
                    };

                    _context.CertificacionVendedor.Add(certificacion);
                    urlsArchivos.Add(certificacion.UrlArchivo);
                }

                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Certificaciones subidas correctamente", lenguaje);

                return new OperationResult(true, mensaje2, urlsArchivos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al subir las certificaciones: {ex.Message}");
            }
        }
        public async Task<OperationResult> EliminarCertificacionPendiente(int idCertificacion, string lenguaje = "es")
        {
            try
            {
                // Buscar la certificación en la base de datos
                var certificacion = await _context.CertificacionVendedor
                    .FirstOrDefaultAsync(c => c.IdCertificacion == idCertificacion);

                if (certificacion == null)
                {
                    var mensaje = _firebasetranslate.Traducir("La certificación no existe o ya fue eliminada.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Buscar las asociaciones existentes con esta certificación
                var certificacionesTipoServicioExistentes = await _context.CertificacionTipoServicio
                    .Where(c => c.IdCertificacion == idCertificacion)
                    .ToListAsync();

                // Eliminar las asociaciones si existen
                if (certificacionesTipoServicioExistentes.Any())
                {
                    _context.CertificacionTipoServicio.RemoveRange(certificacionesTipoServicioExistentes);
                }

                // Eliminar la certificación de la base de datos
                _context.CertificacionVendedor.Remove(certificacion);

                // Eliminar el archivo relacionado con la certificación, si existe
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificacion.UrlArchivo);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                // Verificar si el vendedor tiene alguna certificación restante
                var vendedorCertificaciones = await _context.CertificacionVendedor
                    .Where(c => c.IdUsuario == certificacion.IdUsuario)
                    .ToListAsync();

                if (vendedorCertificaciones.Count == 0)
                {
                    // Si no tiene certificaciones, cambiar el estado del vendedor a Pendiente
                    var estadoVendedorPendiente = await _context.EstadoVendedor
                        .FirstOrDefaultAsync(x => x.PendienteValidacion == true);

                    if (estadoVendedorPendiente != null)
                    {
                        var vendedor = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == certificacion.IdUsuario);
                        if (vendedor != null)
                        {
                            vendedor.IdEstadoVendedor = estadoVendedorPendiente.IdEstadoVendedor;
                            _context.Usuario.Update(vendedor);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var mensaje2 = _firebasetranslate.Traducir("La certificación pendiente se eliminó correctamente.", lenguaje);

                return new OperationResult(true, mensaje2);
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que ocurra
                return new OperationResult(false, $"Error al eliminar la certificación pendiente: {ex.Message}");
            }
        }
        public async Task<OperationResult> AsociarCertificacionTipoServicio(List<CertificacionTipoServicioDto> dto, string lenguaje = "es" )
        {
            // Validar si la lista de DTO es nula, vacía o si algún elemento no tiene tipos de servicio asociados
            if (dto == null || !dto.Any() || dto.Any(d => d.TipoServicioIds == null))
            {
                var mensaje = _firebasetranslate.Traducir("No se ha proporcionado una lista de tipos de servicio.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            try
            {
                // Crear un objeto para registrar el estado de las certificaciones procesadas
                var certificacionesStatus = new CertificacionesStatusDto
                {
                    CertificacionesAprobadas = new List<CertificacionVendedorDto>(),
                    CertificacionesRechazadas = new List<CertificacionVendedorDto>(),
                    CertificacionesPendientes = new List<CertificacionVendedorDto>() // Pendientes opcional
                };

                int vendedorId = 0; // Variable para almacenar el ID del vendedor

                // Iterar sobre cada DTO proporcionado
                foreach (var item in dto)
                {
                    // Obtener la certificación del vendedor desde la base de datos
                    var certificacion = await _context.CertificacionVendedor
                        .FirstOrDefaultAsync(c => c.IdCertificacion == item.IdCertificacion);

                    // Si no existe la certificación, continuar con el siguiente elemento
                    if (certificacion == null)
                    {
                        continue;
                    }

                    // Guardar el ID del vendedor asociado con la certificación
                    vendedorId = certificacion.IdUsuario;

                    if (item.Aprobar)
                    {
                        // Si se aprueba, asociar los tipos de servicio proporcionados con la certificación

                        // Buscar las asociaciones existentes con esta certificación
                        var certificacionesTipoServicioExistentes = await _context.CertificacionTipoServicio
                            .Where(c => c.IdCertificacion == item.IdCertificacion)
                            .ToListAsync();

                        // Eliminar las asociaciones existentes
                        _context.CertificacionTipoServicio.RemoveRange(certificacionesTipoServicioExistentes);

                        // Crear nuevas asociaciones con los tipos de servicio del DTO
                        var nuevasAsociaciones = item.TipoServicioIds.Select(tipoServicioId => new CertificacionTipoServicio
                        {
                            IdCertificacion = item.IdCertificacion,
                            IdTipoServicio = tipoServicioId
                        }).ToList();

                        // Agregar las nuevas asociaciones a la base de datos
                        _context.CertificacionTipoServicio.AddRange(nuevasAsociaciones);
                        await _context.SaveChangesAsync();

                        // Registrar la certificación como aprobada
                        certificacionesStatus.CertificacionesAprobadas.Add(new CertificacionVendedorDto
                        {
                            IdCertificacion = certificacion.IdCertificacion,
                            IdUsuario = certificacion.IdUsuario,
                            Nombre = certificacion.Nombre,
                            Descripcion = certificacion.Descripcion,
                            UrlArchivo = certificacion.UrlArchivo,
                            FechaCertificacion = certificacion.FechaCertificacion,
                            ComentarioCancelado = item.ComentarioCancelado ?? "" // Agregado
                        });
                    }
                    else
                    {
                        // Si no se aprueba, eliminar la certificación y sus asociaciones

                        // Buscar las asociaciones existentes con esta certificación
                        var certificacionesTipoServicioExistentes = await _context.CertificacionTipoServicio
                            .Where(c => c.IdCertificacion == item.IdCertificacion)
                            .ToListAsync();

                        // Eliminar las asociaciones si existen
                        if (certificacionesTipoServicioExistentes.Any())
                        {
                            _context.CertificacionTipoServicio.RemoveRange(certificacionesTipoServicioExistentes);
                        }

                        // Eliminar la certificación de la base de datos
                        _context.CertificacionVendedor.Remove(certificacion);

                        // Eliminar el archivo relacionado con la certificación, si existe
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificacion.UrlArchivo);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        await _context.SaveChangesAsync();

                        // Registrar la certificación como rechazada
                        certificacionesStatus.CertificacionesRechazadas.Add(new CertificacionVendedorDto
                        {
                            IdCertificacion = certificacion.IdCertificacion,
                            IdUsuario = certificacion.IdUsuario,
                            Nombre = certificacion.Nombre,
                            Descripcion = certificacion.Descripcion,
                            UrlArchivo = certificacion.UrlArchivo,
                            FechaCertificacion = certificacion.FechaCertificacion,
                            ComentarioCancelado = item.ComentarioCancelado??"" // Agregado
                        });
                    }
                }


                // Si hay certificaciones aprobadas, activar el estado del vendedor
                if (certificacionesStatus.CertificacionesAprobadas.Any())
                {
                    // Buscar el estado activo en la tabla EstadoVendedor
                    var estadoVendedorActivo = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.Activo == true);

                    if (estadoVendedorActivo != null)
                    {
                        // Buscar el vendedor asociado por ID
                        var vendedor = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == vendedorId);

                        if (vendedor != null)
                        {
                            // Actualizar el estado del vendedor a activo
                            vendedor.IdEstadoVendedor = estadoVendedorActivo.IdEstadoVendedor;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                var certificacionesAprobadasIds = await _context.CertificacionTipoServicio
                    .Select(c => c.IdCertificacion)
                    .Distinct()
                    .ToListAsync();

                // Obtener todas las certificaciones del usuario
                                var todasLasCertificaciones = await _context.CertificacionVendedor
                .Where(c => c.IdUsuario == vendedorId)
                .ToListAsync();

                foreach (var certificacion in todasLasCertificaciones)
                {
                    if (!certificacionesAprobadasIds.Contains(certificacion.IdCertificacion))
                    {
                        certificacionesStatus.CertificacionesPendientes.Add(new CertificacionVendedorDto
                        {
                            IdCertificacion = certificacion.IdCertificacion,
                            IdUsuario = certificacion.IdUsuario,
                            Nombre = certificacion.Nombre,
                            Descripcion = certificacion.Descripcion,
                            UrlArchivo = certificacion.UrlArchivo,
                            FechaCertificacion = certificacion.FechaCertificacion,
                            ComentarioCancelado = "" // Agregado
                        });
                    }
                }

                var vendedorCorreo = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == vendedorId);
                if (vendedorCorreo != null)
                {
                    var mensaje = new EmailRequestDto()
                    {
                        Mensaje = @"
                            <!DOCTYPE html>
                            <html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Estado de Certificaciones</title>
                            </head>
                            <body>
                                <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                    <!-- Encabezado con imagen -->
                                    <div style=""text-align: center; margin-bottom: 20px;"">
                                        <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                    </div>

                                    <h2 style=""color: #333; text-align: center;"">Estado de tus Certificaciones</h2>
                                    <p style=""color: #666;"">Hola " + vendedorCorreo.Nombre + @",</p>
                                    <p style=""color: #666;"">A continuación te presentamos el estado de tus certificaciones:</p>

                                    <!-- Certificaciones Aprobadas -->
                                    " + (certificacionesStatus.CertificacionesAprobadas.Any() ? @"
                                    <div style=""margin-top: 20px;"">
                                        <h3 style=""color: #0066cc;"">Certificaciones Aprobadas</h3>
                                        <ul style=""list-style-type: none; padding-left: 0;"">" +
                                            string.Join("", certificacionesStatus.CertificacionesAprobadas.Select(c => @"
                                            <li style=""padding: 10px; background-color: #f8f9fa; margin-bottom: 5px; border-left: 4px solid #0066cc;"">
                                                <strong>" + c.Nombre + @"</strong><br>
                                                <span style=""color: #666; font-size: 14px;"">Fecha: " + c.FechaCertificacion.ToString("dd/MM/yyyy") + @"</span>
                                            </li>")) + @"
                                        </ul>
                                    </div>" : "") + @"

                                    <!-- Certificaciones Pendientes -->
                                    " + (certificacionesStatus.CertificacionesPendientes.Any() ? @"
                                    <div style=""margin-top: 20px;"">
                                        <h3 style=""color: #ffc107;"">Certificaciones Pendientes</h3>
                                        <ul style=""list-style-type: none; padding-left: 0;"">" +
                                            string.Join("", certificacionesStatus.CertificacionesPendientes.Select(c => @"
                                            <li style=""padding: 10px; background-color: #f8f9fa; margin-bottom: 5px; border-left: 4px solid #ffc107;"">
                                                <strong>" + c.Nombre + @"</strong><br>
                                                <span style=""color: #666; font-size: 14px;"">Fecha: " + c.FechaCertificacion.ToString("dd/MM/yyyy") + @"</span>
                                            </li>")) + @"
                                        </ul>
                                    </div>" : "") + @"

                                    <!-- Certificaciones Rechazadas -->
                                    " + (certificacionesStatus.CertificacionesRechazadas.Any() ? @"
                                    <div style=""margin-top: 20px;"">
                                        <h3 style=""color: #dc3545;"">Certificaciones Rechazadas</h3>
                                        <ul style=""list-style-type: none; padding-left: 0;"">" +
                                            string.Join("", certificacionesStatus.CertificacionesRechazadas.Select(c => @"
                                            <li style=""padding: 10px; background-color: #f8f9fa; margin-bottom: 5px; border-left: 4px solid #dc3545;"">
                                                <strong>" + c.Nombre + @"</strong><br>
                                                <span style=""color: #666; font-size: 14px;"">Fecha: " + c.FechaCertificacion.ToString("dd/MM/yyyy") + @"</span><br>
                                                <span style=""color: #dc3545; font-size: 14px;"">Motivo: " + c.ComentarioCancelado + @"</span>
                                            </li>")) + @"
                                        </ul>
                                    </div>" : "") + @"

                                    <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;"">
                                        <p style=""color: #666;"">Si tienes alguna pregunta sobre el estado de tus certificaciones, no dudes en contactarnos.</p>
                                        <p style=""color: #666;"">Gracias,<br>HIFFY Team</p>
                                    </div>
                                </div>
                            </body>
                            </html>
                        ",
                        Encabezado = "Estado de tus Certificaciones",
                        EmailDestino = vendedorCorreo.Correo

                    };
                    _emailService.EnviarCorreo(mensaje);


                    var resumenNotificacion = $@"El estado de sus certificaciones se ha actualizado, porfavor verificar su correo para conocer el estatus de sus solicitudes.
                    .
                    ";

                    await _notificationService.SendNotificationAsync(
                    "Solicitud de vendedor actualizada",
                    resumenNotificacion,
                    vendedorId);

                }

                var mensaje2 = _firebasetranslate.Traducir("Operación completada con éxito", lenguaje);


                // Retornar el resultado de la operación con los estados de las certificaciones
                return new OperationResult(true, mensaje2, certificacionesStatus);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir($"Error al asociar o eliminar certificaciones con tipos de servicio: {ex.Message}", lenguaje);

                // Manejo de errores y retorno de mensaje detallado
                return new OperationResult(false, mensaje);
            }
        }
        public async Task<OperationResult> ObtenerCertificacionesAprobadasOListado(int usuarioId, string lenguaje = "es")
        {
            try
            {
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("El usuario no existe.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                var certificacionesStatus = new CertificacionesStatusDto
                {
                    CertificacionesAprobadas = new List<CertificacionVendedorDto>(),
                    CertificacionesRechazadas = new List<CertificacionVendedorDto>(),
                    CertificacionesPendientes = new List<CertificacionVendedorDto>()
                };

                // Obtener las certificaciones relacionadas con tipos de servicio (aprobadas)
                var certificacionesAprobadasIds = await _context.CertificacionTipoServicio
                    .Select(c => c.IdCertificacion)
                    .Distinct()
                    .ToListAsync();

                // Obtener todas las certificaciones del usuario
                var todasLasCertificaciones = await _context.CertificacionVendedor
                    .Where(c => c.IdUsuario == usuarioId)
                    .ToListAsync();

                foreach (var certificacion in todasLasCertificaciones)
                {
                    if (certificacionesAprobadasIds.Contains(certificacion.IdCertificacion))
                    {
                        certificacionesStatus.CertificacionesAprobadas.Add(new CertificacionVendedorDto
                        {
                            IdCertificacion = certificacion.IdCertificacion,
                            IdUsuario = certificacion.IdUsuario,
                            Nombre = certificacion.Nombre,
                            Descripcion = certificacion.Descripcion,
                            UrlArchivo = certificacion.UrlArchivo,
                            FechaCertificacion = certificacion.FechaCertificacion,
                            ComentarioCancelado = "" // Agregado
                        });
                    }
                    else
                    {
                        certificacionesStatus.CertificacionesPendientes.Add(new CertificacionVendedorDto
                        {
                            IdCertificacion = certificacion.IdCertificacion,
                            IdUsuario = certificacion.IdUsuario,
                            Nombre = certificacion.Nombre,
                            Descripcion = certificacion.Descripcion,
                            UrlArchivo = certificacion.UrlArchivo,
                            FechaCertificacion = certificacion.FechaCertificacion,
                            ComentarioCancelado = "" // Agregado
                        });
                    }
                }

                var mensaje2 = _firebasetranslate.Traducir("Certificaciones categorizadas correctamente.", lenguaje);

                // Retornar el objeto de estado
                return new OperationResult(true, mensaje2, certificacionesStatus);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir($"Error al consultar las certificaciones: {ex.Message}", lenguaje);

                return new OperationResult(false, mensaje);
            }
        }
        public async Task<OperationResult> ObtenerTiposServiciosPermitidos(int usuarioId, string lenguaje = "es")
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);
            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("El usuario no existe.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            try
            {
                // Obtener las certificaciones del vendedor
                var certificaciones = await _context.CertificacionVendedor
                    .Where(c => c.IdUsuario == usuarioId)
                    .ToListAsync();

                if (certificaciones == null || !certificaciones.Any())
                {
                    var mensaje = _firebasetranslate.Traducir("El vendedor no tiene certificaciones.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Obtener los tipos de servicio asociados a las certificaciones del vendedor
                var tiposServiciosPermitidos = await _context.CertificacionTipoServicio
                    .Where(c => certificaciones.Select(cert => cert.IdCertificacion).Contains(c.IdCertificacion))
                    .Select(c => c.TipoServicio)
                    .ToListAsync();

                if (tiposServiciosPermitidos == null || !tiposServiciosPermitidos.Any())
                {
                    var mensaje = _firebasetranslate.Traducir("No hay tipos de servicios permitidos para este vendedor.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                foreach (var tipoServicio in tiposServiciosPermitidos)
                {
                    tipoServicio.Nombre =  _firebasetranslate.Traducir(tipoServicio.Nombre, lenguaje);
                    tipoServicio.Descripcion =  _firebasetranslate.Traducir(tipoServicio.Descripcion, lenguaje);
                }

                var mensaje2 = _firebasetranslate.Traducir("Tipos de servicios permitidos obtenidos exitosamente.", lenguaje);


                // Retornar los tipos de servicios permitidos con éxito
                return new OperationResult(true, mensaje2, tiposServiciosPermitidos);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir($"Error al consultar los tipos de servicios disponibles: {ex.Message}", lenguaje);

                return new OperationResult(false, mensaje);
            }
        }

        public async Task<OperationResult> ObtenerVendedores(string lenguaje = "es")
        {
            try
            {
                var usuarios = await _context.Usuario
                    .Include(u => u.EstadoVendedor)
                    .Where(u => u.IdRol == 2 || u.IdRol == 4) // Filtrar por IdRol 2 o 4
                    .Select(u => new UsuarioDto
                    {
                        IdUsuario = u.IdUsuario,
                        Correo = u.Correo,
                        Nombre = u.Nombre,
                        IdRol = u.IdRol,
                        IdFamilia = u.IdFamilia,
                        FechaNacimiento = u.FechaNacimiento,
                        FechaRegistro = u.FechaRegistro,
                        Rol = u.Rol,
                        EstadoFamilia = u.EstadoFamilia,
                        EstadoVendedor = u.EstadoVendedor,
                        RolFamilia = u.RolFamilia
                    })
                    .ToListAsync();

                var mensaje = _firebasetranslate.Traducir("Usuarios obtenidos con éxito", lenguaje);


                return new OperationResult(true, mensaje, usuarios);
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir($"Error al obtener usuarios: {ex.Message}", lenguaje);

                return new OperationResult(false, mensaje);
            }
        }
    }
}
