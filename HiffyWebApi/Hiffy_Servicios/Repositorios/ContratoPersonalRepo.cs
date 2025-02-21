using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Mail; 

namespace Hiffy_Servicios.Repositorios
{
    public class ContratoPersonalRepositorio : IContratoPersonal
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        private readonly IHostEnvironment _hostEnvironment; private readonly string _hiffyWebUrl;
        private readonly FirebaseTranslationService _firebasetranslate;

        public ContratoPersonalRepositorio(AppDbContext context, INotificationService notificationService, IEmailService emailService, IHostEnvironment hostEnvironment, IConfiguration configuration, FirebaseTranslationService firebasetranslate

           )
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
            _hostEnvironment = hostEnvironment;
            _hiffyWebUrl = configuration["HiffyWeb:Url"];
            _firebasetranslate = firebasetranslate;

        }

        public async Task<OperationResult> ObtenerContratoPorId(int idContrato, string lenguaje = "es")
        {
            // Buscar la tarea asignada por ID
            var tareaAsignada = await _context.ContratoPersonal
                .Where(t => t.IdContrato == idContrato)
                .Select(t => new GetContratoFamiliaDto
                {
                    ContratoPersonal = t,
                    CorreoOfertador = _context.Usuario.Where(u => u.IdUsuario == t.Servicio.IdUsuario).Select(u => u.Correo).FirstOrDefault(),
                    FotoOfertador = _context.Usuario.Where(u => u.IdUsuario == t.Servicio.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                    NombreOfertador = _context.Usuario.Where(u => u.IdUsuario == t.Servicio.IdUsuario).Select(u => u.Nombre).FirstOrDefault(),
                    Servicio = t.Servicio
                })
                .FirstOrDefaultAsync(); // Obtener el primer resultado o null si no existe

            if (tareaAsignada == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato de personal no encontrado.", lenguaje);

                return new OperationResult(false, mensaje, null);
            }
            var mensaje2 = _firebasetranslate.Traducir("Contrato de personal encontrado.", lenguaje);

            return new OperationResult(true, mensaje2, tareaAsignada);
        }

        public async Task<object> GetContratosVendedorPorEstado(int id, EstadoContrato estado, bool isVendor, string lenguaje = "es")
        {
            if (isVendor)
            {

                var contratos = await _context.ContratoPersonal
                   .Where(x => x.Servicio.IdUsuario == id && x.Estado == estado)
                   .Select(x => new
                   {
                       ContratoPersonal = x,
                       CorreoOfertador = _context.Usuario
                           .Where(u => u.IdUsuario == x.Servicio.IdUsuario)
                           .Select(u => u.Correo)
                           .FirstOrDefault(),
                       FotoOfertador = _context.Usuario
                           .Where(u => u.IdUsuario == x.Servicio.IdUsuario)
                           .Select(u => u.FotoUrl)
                           .FirstOrDefault(),
                       NombreOfertador = _context.Usuario
                           .Where(u => u.IdUsuario == x.Servicio.IdUsuario)
                           .Select(u => u.Nombre)
                           .FirstOrDefault(),
                       Servicio = x.Servicio,
                        
                       Solicitante = _context.Usuario.Include(u => u.RolFamilia)
                           .Where(u => u.IdFamilia == x.IdFamilia && u.RolFamilia.EsAdmin)
                           .Select(u => new
                           {
                               IdUsuario = u.IdUsuario,
                               Nombre = u.Nombre,
                               Correo = u.Correo,
                               FotoUrl = u.FotoUrl,
                               Longitud = _context.Familia
                                   .Where(f => f.IdFamilia == x.IdFamilia)
                                   .Select(f =>f.Longitud).FirstOrDefault(),
                               Latitud = _context.Familia
                                   .Where(f => f.IdFamilia == x.IdFamilia)
                                   .Select(f => f.Altitud).FirstOrDefault(),
                               // Construcción de la URL de Google Maps
                               Direccion = _context.Familia
                                   .Where(f => f.IdFamilia == x.IdFamilia)
                                   .Select(f => $"https://www.google.com/maps?q={f.Altitud},{f.Longitud}")
                                   .FirstOrDefault()
                           })
                           .FirstOrDefault()

                   }).ToListAsync();

                return contratos;
            }
            else
            {
                var contratos = await _context.ContratoPersonal
            .Where(x => x.IdFamilia == id && x.Estado == estado).Select(x => new GetContratoFamiliaDto
            {
                ContratoPersonal = x,
                CorreoOfertador = _context.Usuario.Where(u => u.IdUsuario == x.Servicio.IdUsuario).Select(u => u.Correo).FirstOrDefault(),
                FotoOfertador = _context.Usuario.Where(u => u.IdUsuario == x.Servicio.IdUsuario).Select(u => u.FotoUrl).FirstOrDefault(),
                NombreOfertador = _context.Usuario.Where(u => u.IdUsuario == x.Servicio.IdUsuario).Select(u => u.Nombre).FirstOrDefault(),
                Servicio = x.Servicio

            }).ToListAsync();

                return contratos;
            }
        }

        // Crear un nuevo contrato personal
        public async Task<OperationResult> CrearContratoPersonal(PostContratoDto postContratoDto, int idFamilia, int idUsuario, string lenguaje = "es"
)
        {
            try
            {
                // Crear una nueva instancia de ContratoPersonal y asignar los datos del PostContratoDto
                var contratoPersonal = new ContratoPersonal
                {
                    IdFamilia = idFamilia,
                    IdServicioContratado = postContratoDto.IdServicioContratado,
                    FechaInicio = postContratoDto.FechaInicio,
                    FechaFin = postContratoDto.FechaFin,
                    Estado = EstadoContrato.Solicitado, // Enum para definir el estado inicial
                    CodigoVerificacion = 0,
                    CodigoFinalizacion = 0,
                    Valoracion = null,
                    FechaRegistro = DateTime.Now,
                };

                // Obtener el ID del vendedor asociado al servicio
                var vendedorId = await _context.Servicio
                    .Where(s => s.IdServicio == postContratoDto.IdServicioContratado)
                    .Select(s => s.IdUsuario)
                    .FirstOrDefaultAsync();

                if (vendedorId == 0)
                {
                    var respuestaError = "El servicio especificado no existe o no tiene un vendedor asociado.";
                    return new OperationResult(false, respuestaError, null);
                }

                // Verificar si ya existe un contrato con solapamiento de fechas para el mismo vendedor o servicio
                bool existeSolapamiento = await _context.ContratoPersonal
                    .Include(cp => cp.Servicio) // Incluir la relación para acceder al vendedor y servicios
                    .AnyAsync(cp =>
                        cp.IdFamilia == idFamilia &&
                        cp.Estado != EstadoContrato.Finalizado && cp.Estado != EstadoContrato.Cancelado &&
                        (cp.Servicio.IdUsuario == vendedorId) && // Validar por el vendedor
                        (
                            (postContratoDto.FechaInicio >= cp.FechaInicio && postContratoDto.FechaInicio <= cp.FechaFin) || // Solapamiento por inicio
                            (postContratoDto.FechaFin >= cp.FechaInicio && postContratoDto.FechaFin <= cp.FechaFin) ||      // Solapamiento por fin
                            (postContratoDto.FechaInicio <= cp.FechaInicio && postContratoDto.FechaFin >= cp.FechaFin)      // Solapamiento total
                        )
                    );

                if (existeSolapamiento)
                {
                    var respuesta =   _firebasetranslate.Traducir("Ya existe un contrato que se solapa con las fechas proporcionadas para este vendedor.", lenguaje);
                    return new OperationResult(false, respuesta, null);
                }

                // Agregar el contrato personal al contexto y guardar los cambios
                await _context.ContratoPersonal.AddAsync(contratoPersonal);
                await _context.SaveChangesAsync();
                 


                // ENVIAR CORREO DE SOLICITADO 
                var familia = await _context.Familia.Where(x => x.IdFamilia == idFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
                var fecha = postContratoDto.FechaInicio;
                var servicio = await _context.Servicio.Where(x => x.IdServicio == postContratoDto.IdServicioContratado).FirstOrDefaultAsync();
                var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
                var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
                var usuario = await _context.Usuario.Where(x => x.IdUsuario == idUsuario).FirstOrDefaultAsync();
                var nombreSolicitante = usuario.Nombre;
                var estado = EstadoContrato.Solicitado;

                // Crear el mensaje del correo
                var mensajeOfertador = new EmailRequestDto
                {
                    Mensaje = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Estado de Solicitud</title>
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Estado de Solicitud</h2>

                                <p style=""color: #666; text-align: center;"">
                                    Se ha solicitado el servicio <strong style=""color: #004080;"">{detalleServicio}</strong> el día 
                                    <strong style=""color: #004080;"">{fecha}</strong> por el usuario 
                                    <strong style=""color: #004080;"">{nombreSolicitante}</strong> de la familia 
                                    <strong style=""color: #004080;"">{familia}</strong>.
                                </p>

                                <p style=""color: #666; text-align: center;"">
                                    El estado actual de la solicitud es: <strong style=""color: #0066cc;"">{estado}</strong>.
                                </p>

                                <!-- Botón -->
                                <div style=""text-align: center; margin: 20px 0;"">
                                    <a href=""{_hiffyWebUrl}""
                                       style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                              padding: 10px 20px; border-radius: 5px;"">
                                        Ver Detalles
                                    </a>
                                </div>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                    <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                                </div>
                            </div>
                        </body>
                        </html>
                        ",
                    Encabezado = "Nuevo Servicio Solicitado",
                    EmailDestino = correoOfertador,
                };
                // Enviar el correo
                _emailService.EnviarCorreo(mensajeOfertador);

                var resumenNotificacion = $@"
                    Se ha solicitado el servicio '{detalleServicio}' el día {fecha:dd/MM/yyyy} por el usuario '{nombreSolicitante}' de la familia '{familia}'.
                    El estado actual de la solicitud es: '{estado}'.
                    ";

                await _notificationService.SendNotificationAsync(
                "Solicitud de Servicio Creada",
                resumenNotificacion,
                servicio.IdUsuario);

                var mensaje = _firebasetranslate.Traducir("Contrato de personal solicitado exitosamente.", lenguaje);

                return new OperationResult(true, mensaje, contratoPersonal.IdContrato);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        // Aceptar contrato personal (asignar código de verificación)
        public async Task<OperationResult> AceptarContratoPersonal(int idContrato, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.Estado != EstadoContrato.Solicitado)
            {
                var mensaje = _firebasetranslate.Traducir("El contrato no está en estado 'Solicitado'.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Verificar si ya existen contratos aceptados o en curso con la misma fecha de inicio
            var contratosEnFechaInicio = await _context.ContratoPersonal
                .Where(x => x.FechaInicio == contrato.FechaInicio &&
                            (x.Estado == EstadoContrato.Aceptado || x.Estado == EstadoContrato.EnCurso))
                .AnyAsync();

            if (contratosEnFechaInicio)
            {
                var mensaje = _firebasetranslate.Traducir("Ya existe un contrato en estado 'Aceptado' o 'EnCurso' con la misma fecha de inicio.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Generar el código de verificación único para el contrato
            contrato.CodigoVerificacion = await GenerarCodigoVerificacionUnico(contrato.IdServicioContratado);

            contrato.Estado = EstadoContrato.Aceptado;

            await _context.SaveChangesAsync();


            // ENVIAR CORREO DE ACEPTADO 
            var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
            var fecha = contrato.FechaInicio;
            var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
            var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
            var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
            var estado = contrato.Estado.ToString(); // Estado dinámico basado en el valor de contrato.Estado

            var usuarioAdminFamilia = await _context.Usuario.Include(u => u.RolFamilia)
             .Where(x => x.IdFamilia == contrato.IdFamilia && x.RolFamilia.EsAdmin == true)
             .ToListAsync();

            // Crear el mensaje del correo
            var mensajeOfertador = new EmailRequestDto
            {
                Mensaje = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Cambio de Estado de Solicitud</title>
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Cambio de Estado de Solicitud</h2>

                                <p style=""color: #666; text-align: center;"">
                                    Se ha actualizado el estado del servicio <strong style=""color: #004080;"">{detalleServicio}</strong> solicitado el 
                                    <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                    <strong style=""color: #004080;"">{familia}</strong>.
                                </p>

                                <p style=""color: #666; text-align: center;"">
                                    El nuevo estado de la solicitud es: <strong style=""color: #0066cc;"">{estado}</strong>.
                                </p>

                                <!-- Botón -->
                                <div style=""text-align: center; margin: 20px 0;"">
                                    <a href=""{_hiffyWebUrl}""
                                       style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                              padding: 10px 20px; border-radius: 5px;"">
                                        Ver Detalles
                                    </a>
                                </div>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                    <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                                </div>
                            </div>
                        </body>
                        </html>
                        ", 
                Encabezado = "Solicitud de Servicio Aceptada",
            };

            // Enviar el correo a cada administrador de familia
            foreach (var admin in usuarioAdminFamilia)
            {
                mensajeOfertador.EmailDestino = admin.Correo; // Asignar el correo del administrador
                _emailService.EnviarCorreo(mensajeOfertador); // Enviar el correo
            }

            var resumenNotificacion = $@"
            Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
            El nuevo estado de la solicitud es: '{estado}'.";

            foreach (var admin in usuarioAdminFamilia)
            {
                await _notificationService.SendNotificationAsync(
                "Solicitud de Servicio Aceptada",
                resumenNotificacion,
                admin.IdUsuario);

            }

            var mensaje2 = _firebasetranslate.Traducir("Contrato aceptado exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2);
        }


        // Comenzar contrato personal (poner estado en 'EnCurso' y solicitar código de verificación)
        public async Task<OperationResult> ComenzarContratoPersonal(int idContrato, int codigoVerificacion, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.Estado != EstadoContrato.Aceptado)
            {
                var mensaje = _firebasetranslate.Traducir("El contrato no está en estado 'Aceptado'.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Verificar que el código de verificación sea válido y coincida con el de la entidad
            if (contrato.CodigoVerificacion != codigoVerificacion)
            {
                var mensaje = _firebasetranslate.Traducir("El código de verificación proporcionado no coincide con el de la entidad.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Generar el código de finalización único cuando el contrato pase a 'EnCurso'
            contrato.CodigoFinalizacion = await GenerarCodigoVerificacionUnico(contrato.IdServicioContratado);

            contrato.Estado = EstadoContrato.EnCurso;

            await _context.SaveChangesAsync();


            // ENVIAR CORREO DE ACEPTADO 
            var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
            var fecha = contrato.FechaInicio;
            var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
            var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
            var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
            var estado = contrato.Estado.ToString(); // Estado dinámico basado en el valor de contrato.Estado

            var usuarioAdminFamilia = await _context.Usuario.Include(u => u.RolFamilia)
             .Where(x => x.IdFamilia == contrato.IdFamilia && x.RolFamilia.EsAdmin == true)
             .ToListAsync();

            // Crear el mensaje del correo
            var mensajeOfertador = new EmailRequestDto
            {
                Mensaje = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Cambio de Estado de Solicitud</title>
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Cambio de Estado de Solicitud</h2>

                                <p style=""color: #666; text-align: center;"">
                                    Se ha actualizado el estado del servicio <strong style=""color: #004080;"">{detalleServicio}</strong> solicitado el 
                                    <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                    <strong style=""color: #004080;"">{familia}</strong>.
                                </p>

                                <p style=""color: #666; text-align: center;"">
                                    El nuevo estado de la solicitud es: <strong style=""color: #0066cc;"">{estado}</strong>.
                                </p>

                                <!-- Botón -->
                                <div style=""text-align: center; margin: 20px 0;"">
                                    <a href=""{_hiffyWebUrl}""
                                       style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                              padding: 10px 20px; border-radius: 5px;"">
                                        Ver Detalles
                                    </a>
                                </div>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                    <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                                </div>
                            </div>
                        </body>
                        </html>
                        ",
                Encabezado = "Servicio Iniciada",
            };

            // Enviar el correo a cada administrador de familia
            foreach (var admin in usuarioAdminFamilia)
            {
                mensajeOfertador.EmailDestino = admin.Correo; // Asignar el correo del administrador
                _emailService.EnviarCorreo(mensajeOfertador); // Enviar el correo
            }

            var resumenNotificacion = $@"
            Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
            El nuevo estado de la solicitud es: '{estado}'.";

            foreach (var admin in usuarioAdminFamilia)
            {
                await _notificationService.SendNotificationAsync(
                "Contrato comenzado exitosamente",
                resumenNotificacion,
                admin.IdUsuario);

            }

            var mensaje2 = _firebasetranslate.Traducir("Contrato comenzado exitosamente.", lenguaje);


            return new OperationResult(true, mensaje2);
        }


        // Cancelar contrato personal (solo si está en estado 'Pendiente' o 'Aceptado')
        public async Task<OperationResult> CancelarContratoPersonalVendedor(int idContrato, string motivo, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.Estado != EstadoContrato.Solicitado  &&  contrato.Estado != EstadoContrato.Aceptado )
            {
                var mensaje = _firebasetranslate.Traducir("El contrato no puede cancelarse en este estado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            contrato.Estado = EstadoContrato.Cancelado ;

            await _context.SaveChangesAsync();


            // ENVIAR CORREO DE ACEPTADO 
            var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
            var fecha = contrato.FechaInicio;
            var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
            var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
            var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
            var estado = contrato.Estado.ToString(); // Estado dinámico basado en el valor de contrato.Estado

            var usuarioAdminFamilia = await _context.Usuario.Include(u => u.RolFamilia)
             .Where(x => x.IdFamilia == contrato.IdFamilia && x.RolFamilia.EsAdmin == true)
             .ToListAsync();

            // Crear el mensaje del correo
            var mensajeOfertador = new EmailRequestDto
            {
                Mensaje = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Cambio de Estado de Solicitud</title>
                    </head>
                    <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                        <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                            <!-- Encabezado con imagen -->
                            <div style=""text-align: center; margin-bottom: 20px;"">
                                <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                            </div>

                            <h2 style=""color: #333; text-align: center;"">Cambio de Estado de Solicitud</h2>

                            <p style=""color: #666; text-align: center;"">
                                Se ha actualizado el estado del servicio <strong style=""color: #004080;"">{detalleServicio}</strong> solicitado el 
                                <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                <strong style=""color: #004080;"">{familia}</strong>.
                            </p>

                            <p style=""color: #666; text-align: center;"">
                                El nuevo estado de la solicitud es: <strong style=""color: #0066cc;"">{estado}</strong>.
                            </p>
                            <p style=""color: #666; text-align: center;"">
                                Motivo de la cancelación: <strong style=""color: #0066cc;"">{motivo}</strong>.
                            </p>

                            <!-- Botón -->
                            <div style=""text-align: center; margin: 20px 0;"">
                                <a href=""{_hiffyWebUrl}""
                                   style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                          padding: 10px 20px; border-radius: 5px;"">
                                    Ver Detalles
                                </a>
                            </div>

                            <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                            </div>
                        </div>
                    </body>
                    </html>
    ",
                Encabezado = "Solicitud de Servicio Cancelada",
            };

            // Enviar el correo a cada administrador de familia
            foreach (var admin in usuarioAdminFamilia)
            {
                mensajeOfertador.EmailDestino = admin.Correo; // Asignar el correo del administrador
                _emailService.EnviarCorreo(mensajeOfertador); // Enviar el correo
            }

            var resumenNotificacion = $@"
            Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
            El nuevo estado de la solicitud es: '{estado}'.";

            foreach (var admin in usuarioAdminFamilia)
            {
                await _notificationService.SendNotificationAsync(
                "Contrato cancelado exitosamente",
                resumenNotificacion,
                admin.IdUsuario);

            }

            var mensaje2 = _firebasetranslate.Traducir("Contrato cancelado exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2);
        }

        // Cancelar contrato personal (solo si está en estado 'Pendiente' o 'Aceptado')
        public async Task<OperationResult> CancelarContratoPersonalFamilia(int idContrato, string motivo, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.Estado != EstadoContrato.Solicitado && contrato.Estado != EstadoContrato.Aceptado)
            {
                var mensaje = _firebasetranslate.Traducir("El contrato no puede cancelarse en este estados.", lenguaje);

                return new OperationResult(false, mensaje);
            }
            var eliminar = contrato.Estado == EstadoContrato.Solicitado;

            // Lógica de cancelación o eliminación
            if (eliminar)
            {
                // Eliminar el contrato
                _context.ContratoPersonal.Remove(contrato);
                await _context.SaveChangesAsync();

                // Obtener la información de la familia, servicio, etc.
                var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
                var fecha = contrato.FechaInicio;
                var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
                var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
                var estado = "Eliminado"; // En este caso, el estado es Eliminado

                // Correo a los familiares
                var usuarioAdminFamilia = await _context.Usuario.Include(u => u.RolFamilia)
                    .Where(x => x.IdFamilia == contrato.IdFamilia && x.RolFamilia.EsAdmin == true)
                    .ToListAsync();

                var mensajeFamilia = new EmailRequestDto
                {
                    Mensaje = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Cambio de Estado de Solicitud</title>
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Solicitud de Servicio Eliminada</h2>

                                <p style=""color: #666; text-align: center;"">
                                    El contrato del servicio <strong style=""color: #004080;"">{detalleServicio}</strong>, solicitado el 
                                    <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                    <strong style=""color: #004080;"">{familia}</strong>, ha sido eliminado.
                                </p>

                                <p style=""color: #666; text-align: center;"">
                                    Motivo de la eliminación: <strong style=""color: #0066cc;"">{motivo}</strong>.
                                </p>

                                <!-- Botón -->
                                <div style=""text-align: center; margin: 20px 0;"">
                                    <a href=""{_hiffyWebUrl}""
                                       style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                              padding: 10px 20px; border-radius: 5px;"">
                                        Ver Detalles
                                    </a>
                                </div>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                    <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                    Encabezado = "Contrato de Servicio Eliminado",
                };

                // Enviar correo a los administradores de familia
                foreach (var admin in usuarioAdminFamilia)
                {
                    mensajeFamilia.EmailDestino = admin.Correo;
                    _emailService.EnviarCorreo(mensajeFamilia); // Enviar el correo
                }

                var resumenNotificacion = $@"
                Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
                El nuevo estado de la solicitud es: '{estado}'.";

                foreach (var admin in usuarioAdminFamilia)
                {
                    await _notificationService.SendNotificationAsync(
                    "Contrato eliminado exitosamente",
                    resumenNotificacion,
                    admin.IdUsuario);

                }
            }
            else
            {
                // Cancelar el contrato
                contrato.Estado = EstadoContrato.Cancelado;
                contrato.MotivoCancelacion = motivo;
                await _context.SaveChangesAsync();

                // Obtener la información de la familia, servicio, etc.
                var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
                var fecha = contrato.FechaInicio;
                var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
                var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
                var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
                var estado = contrato.Estado.ToString(); // Estado dinámico basado en el valor de contrato.Estado

                // Correo a los vendedores
                var mensajeVendedor = new EmailRequestDto
                {
                    Mensaje = $@"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Cambio de Estado de Solicitud</title>
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Solicitud de Servicio Eliminada</h2>

                                <p style=""color: #666; text-align: center;"">
                                    El contrato del servicio <strong style=""color: #004080;"">{detalleServicio}</strong>, solicitado el 
                                    <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                    <strong style=""color: #004080;"">{familia}</strong>, ha sido cancelado.
                                </p>

                                <p style=""color: #666; text-align: center;"">
                                    Motivo de la cancelación: <strong style=""color: #0066cc;"">{motivo}</strong>.
                                </p>

                                <!-- Botón -->
                                <div style=""text-align: center; margin: 20px 0;"">
                                    <a href=""{_hiffyWebUrl}""
                                       style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                              padding: 10px 20px; border-radius: 5px;"">
                                        Ver Detalles
                                    </a>
                                </div>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                    <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ", 
                    Encabezado = "Solicitud de Servicio Cancelada",
                };


                // Enviar correo al vendedor
                mensajeVendedor.EmailDestino = correoOfertador;
                _emailService.EnviarCorreo(mensajeVendedor); // Enviar el correo

                var resumenNotificacion = $@"
                Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
                El nuevo estado de la solicitud es: '{estado}'.";

                
                    await _notificationService.SendNotificationAsync(
                    "Contrato eliminado exitosamente",
                    resumenNotificacion,
                    servicio.IdUsuario);

               
            }

            var mensaje2 = _firebasetranslate.Traducir("Contrato eliminado exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2);
        }
         
        // Finalizar contrato personal (solo si está en estado 'EnCurso' y solicitando código de finalización)
        public async Task<OperationResult> FinalizarContratoPersonal(int idContrato, int codigoFinalizacion, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.Estado != EstadoContrato.EnCurso)
            {
                var mensaje = _firebasetranslate.Traducir("El contrato no está en estado 'EnCurso'.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Validar que el código de finalización tenga 4 dígitos
            if (codigoFinalizacion.ToString().Length != 4)
            {
                var mensaje = _firebasetranslate.Traducir("El código de finalización debe tener 4 dígitos.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Verificar que el código de finalización sea igual al de la entidad
            if (contrato.CodigoFinalizacion != codigoFinalizacion)
            {
                var mensaje = _firebasetranslate.Traducir("El código de finalización no coincide con el de la entidad.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            contrato.Estado = EstadoContrato.Finalizado;

            await _context.SaveChangesAsync();

            // ENVIAR CORREO DE ACEPTADO 
            var familia = await _context.Familia.Where(x => x.IdFamilia == contrato.IdFamilia).Select(x => x.Nombre).FirstOrDefaultAsync();
            var fecha = contrato.FechaInicio;
            var servicio = await _context.Servicio.Where(x => x.IdServicio == contrato.IdServicioContratado).FirstOrDefaultAsync();
            var detalleServicio = servicio.Nombre + " - " + servicio.Descripcion;
            var correoOfertador = await _context.Usuario.Where(x => x.IdUsuario == servicio.IdUsuario).Select(x => x.Correo).FirstOrDefaultAsync();
            var estado = contrato.Estado.ToString(); // Estado dinámico basado en el valor de contrato.Estado

            var usuarioAdminFamilia = await _context.Usuario.Include(u => u.RolFamilia)
             .Where(x => x.IdFamilia == contrato.IdFamilia && x.RolFamilia.EsAdmin == true)
             .ToListAsync();

            // Crear el mensaje del correo
            var mensajeOfertador = new EmailRequestDto
            {
                Mensaje = $@"
                    <!DOCTYPE html>
                    <html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Cambio de Estado de Solicitud</title>
                    </head>
                    <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f9f9f9;"">
                        <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                            <!-- Encabezado con imagen -->
                            <div style=""text-align: center; margin-bottom: 20px;"">
                                <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                            </div>

                            <h2 style=""color: #333; text-align: center;"">Estado de Solicitud Actualizado</h2>

                            <p style=""color: #666; text-align: center;"">
                                El contrato del servicio <strong style=""color: #004080;"">{detalleServicio}</strong>, solicitado el 
                                <strong style=""color: #004080;"">{fecha}</strong> por la familia 
                                <strong style=""color: #004080;"">{familia}</strong>, ha sido actualizado.
                            </p>

                            <p style=""color: #666; text-align: center;"">
                                El nuevo estado de la solicitud es: <strong style=""color: #0066cc;"">{estado}</strong>.
                            </p>

                            <!-- Botón -->
                            <div style=""text-align: center; margin: 20px 0;"">
                                <a href=""{_hiffyWebUrl}""
                                   style=""text-decoration: none; font-size: 16px; color: #ffffff; background-color: #0066cc; 
                                          padding: 10px 20px; border-radius: 5px;"">
                                    Ver Detalles
                                </a>
                            </div>

                            <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px; text-align: center;"">
                                <p style=""color: #666;"">Si tienes alguna pregunta, por favor contáctanos.</p>
                                <p style=""color: #666;"">Gracias,<br>El equipo de Hiffy</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ",
                Encabezado = "Servicio Finalizado",
            };


            // Enviar el correo a cada administrador de familia
            foreach (var admin in usuarioAdminFamilia)
            {
                mensajeOfertador.EmailDestino = admin.Correo; // Asignar el correo del administrador
                _emailService.EnviarCorreo(mensajeOfertador); // Enviar el correo
            }

            var resumenNotificacion = $@"
                Se ha actualizado el estado del servicio '{detalleServicio}' solicitado el {fecha:dd/MM/yyyy} por la familia '{familia}'.
                El nuevo estado de la solicitud es: '{estado}'.";

            foreach (var admin in usuarioAdminFamilia)
            {
                await _notificationService.SendNotificationAsync(
                "Contrato finalizado exitosamente",
                resumenNotificacion,
                admin.IdUsuario);

            }

            var mensaje2 = _firebasetranslate.Traducir("Contrato finalizado exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2);
        }
         
        public async Task<OperationResult> ObtenerCodigoVerificacion(int idContrato, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.CodigoVerificacion == 0)  // Validación para un int
            {
                var mensaje = _firebasetranslate.Traducir("El código de verificación no ha sido generado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var mensaje2 = _firebasetranslate.Traducir("Código de verificación obtenido exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2, contrato.CodigoVerificacion);
        }

        public async Task<OperationResult> ObtenerCodigoFinalizacion(int idContrato, string lenguaje = "es")
        {
            var contrato = await _context.ContratoPersonal
                .FirstOrDefaultAsync(x => x.IdContrato == idContrato);

            if (contrato == null)
            {
                var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            if (contrato.CodigoFinalizacion == 0)  // Validación para un int
            {
                var mensaje = _firebasetranslate.Traducir("El código de finalización no ha sido generado.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var mensaje2 = _firebasetranslate.Traducir("Código de finalización obtenido exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2, contrato.CodigoFinalizacion);
        }


        public async Task<int> GenerarCodigoVerificacionUnico(int idServicioContratado, string lenguaje = "es")
        {
            Random random = new Random();
            int codigoGenerado = 0;
            bool codigoValido = false;

            // Intentar generar un código único
            while (!codigoValido)
            {
                // Generar un código aleatorio de 4 dígitos
                codigoGenerado = random.Next(1000, 10000); // Genera un número entre 1000 y 9999

                // Verificar si el código ya está en uso por otro contrato con el mismo idServicioContratado
                var codigoExistente = await _context.ContratoPersonal
                    .Where(x => x.IdServicioContratado == idServicioContratado &&
                                (x.CodigoVerificacion == codigoGenerado || x.CodigoFinalizacion == codigoGenerado))
                    .AnyAsync();

                // Si no se encuentra un código existente, es válido
                if (!codigoExistente)
                {
                    codigoValido = true;
                }
            }

            return codigoGenerado;
        }

        public async Task<OperationResult> ValorarContratoAsync(int idContrato, int valoracion, string lenguaje = "es")
        {
            try
            {
                // Buscar el contrato
                var contrato = await _context.ContratoPersonal
                    .Include(c => c.Servicio)
                    .FirstOrDefaultAsync(c => c.IdContrato == idContrato);

                if (contrato == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Contrato no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                if(contrato.Estado != EstadoContrato.Finalizado)
                {
                    var mensaje = _firebasetranslate.Traducir("El contrato debe tener estado finalizado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Actualizar la valoración del contrato
                contrato.Valoracion = valoracion;
                await _context.SaveChangesAsync();

                // Obtener el IdUsuario del servicio relacionado
                var idUsuario = contrato.Servicio.IdUsuario;

                // Calcular el promedio de valoraciones para el usuario
                var promedioValoracion = await _context.ContratoPersonal
                    .Where(c => c.Servicio.IdUsuario == idUsuario && c.Valoracion != null)
                    .AverageAsync(c => c.Valoracion);

                // Redondear el promedio a un decimal
                promedioValoracion = Math.Round((double)promedioValoracion, 1);

                // Actualizar el promedio en la tabla Usuario
                var usuario = await _context.Usuario.FindAsync(idUsuario);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                usuario.Valoracion = (decimal?)promedioValoracion;
                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Valoración actualizada correctamente", lenguaje);

                return new OperationResult(true, mensaje2, promedioValoracion);
            }
            catch (Exception ex)
            {
                // Capturar el error y devolver un mensaje descriptivo
                return new OperationResult(false, $"Ocurrió un error al valorar el contrato: {ex.Message}");
            }
        }



    }

}
