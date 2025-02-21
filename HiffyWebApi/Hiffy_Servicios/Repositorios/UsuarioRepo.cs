using Google.Cloud.Translation.V2;
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
    public class UsuarioRepo : IFamilia
    {

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly FirebaseTranslationService _firebasetranslate;


        public UsuarioRepo(AppDbContext context, INotificationService notificationService, IEmailService emailService, IConfiguration configuration, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
            _configuration = configuration;
            _firebasetranslate = firebasetranslate;

        }
        public async Task<string> GenerarCodigoFamiliaUnicoAsync()
        {
            var random = new Random();
            string codigoFamilia;

            do
            {
                // Genera un código de 8 dígitos
                codigoFamilia = random.Next(10000000, 99999999).ToString();
            }
            while (await _context.Familia.AnyAsync(f => f.CodigoFamilia == codigoFamilia));

            return codigoFamilia;
        }
        public async Task<OperationResult> CrearUsuario(CrearUsuarioDto usuarioRegistro, string lenguaje = "es")
        {
            if (usuarioRegistro.Correo == null || usuarioRegistro.Contraseña == null)
            {
                var mensaje = _firebasetranslate.Traducir("Favor suministrar las credenciales.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var usuarioExiste = await _context.Usuario.Where(x => x.Correo == usuarioRegistro.Correo).FirstOrDefaultAsync();

            if (usuarioExiste != null)
            {
                var mensaje = _firebasetranslate.Traducir("El correo ya está en uso.", lenguaje);

                return new OperationResult(false, mensaje);
            }
             
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                { 
                    var EstadoFamiliaInactivo = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.Inactivo == true);
                    var EstadoVendedorInactivo = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.Inactivo == true);

                    var edad = DateTime.Now.Year - usuarioRegistro.FechaNacimiento.Year;
                    if (usuarioRegistro.FechaNacimiento > DateTime.Now.AddYears(-edad))
                    {
                        edad--;
                    }

                    // Validar si es mayor de edad y tiene documento
                    if (edad >= 18)
                    {
                        if (usuarioRegistro.IdTipoDocumento == null || string.IsNullOrWhiteSpace(usuarioRegistro.Documento))
                        {
                            var mensaje = _firebasetranslate.Traducir("Los usuarios mayores de edad deben tener un tipo de documento y un documento válido.", lenguaje);

                            return new OperationResult(false, mensaje, 0);
                         
                        }
                    }
                    var usuario = new Usuario
                    {
                        Nombre = usuarioRegistro.Nombre,
                        IdRol = usuarioRegistro.IdRol,
                        Sexo = usuarioRegistro.Sexo,
                        Correo = usuarioRegistro.Correo.ToLower(),
                        FechaRegistro = DateTime.Now,
                        Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioRegistro.Contraseña),
                        Descripcion = "",
                        IdEstadoFamilia = EstadoFamiliaInactivo.IdEstadoFamilia,
                        IdEstadoVendedor = EstadoVendedorInactivo.IdEstadoVendedor,
                        Valoracion = null,
                        FechaNacimiento = usuarioRegistro.FechaNacimiento,
                        IdFamilia = null,
                        IdRolFamilia = null,
                        FechaLimiteCodigo = null,
                        CodigoVerificacion = null,
                        IdTipoDocumento = edad >= 18 ? usuarioRegistro.IdTipoDocumento : null,
                        Documento = edad >= 18 ? usuarioRegistro.Documento : ""
                    }; 
                    await _context.Usuario.AddAsync(usuario);
                    await _context.SaveChangesAsync();


                    transaction.Commit();

                    var mensaje2 = _firebasetranslate.Traducir("Resgistro Exitoso.", lenguaje);

                    return new OperationResult(true, mensaje2, usuario.IdUsuario);
                }
                catch (Exception ex)
                {
                    // Si ocurre algún error, deshacer la transacción
                    transaction.Rollback();
                    return new OperationResult(false, ex.Message, 0);
                }
            }
        }
        public async Task<OperationResult> ActualizarUsuario(ActualizarUsuarioDto actualizarUsuario, string lenguaje = "es")
        {
            try
            {
                // Buscar si el usuario existe
                var usuario = await _context.Usuario
                    .Where(x => x.IdUsuario == actualizarUsuario.IdUsuario)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Validar si el correo es diferente y ya está registrado en la base de datos
                if (!string.Equals(usuario.Correo, actualizarUsuario.Correo, StringComparison.OrdinalIgnoreCase))
                {
                    var existeCorreo = await _context.Usuario
                        .AnyAsync(x => x.Correo == actualizarUsuario.Correo && x.IdUsuario != actualizarUsuario.IdUsuario);
                    if (existeCorreo)
                    {
                        var mensaje = _firebasetranslate.Traducir("El correo proporcionado ya está en uso por otro usuario.", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                }

                // Verificar si el rol está cambiando

                var rolActual = await _context.Rol.FirstOrDefaultAsync(r => r.IdRol == usuario.IdRol);
                var nuevoRol = await _context.Rol.FirstOrDefaultAsync(r => r.IdRol == actualizarUsuario.IdRol);

                var estadoFamilia = await _context.EstadoFamilia.ToListAsync();
                var estadoVendedores = await _context.EstadoVendedor.ToListAsync();

                if (rolActual == null || nuevoRol == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Rol actual o nuevo no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                if (rolActual.IdRol != nuevoRol.IdRol)
                {
                    if (rolActual.EsVendedor && nuevoRol.EsAmbos)
                    {
                        // De vendedor a ambos, se asigna estado pendiente familia
                        actualizarUsuario.IdEstadoFamilia = await _context.EstadoFamilia
                            .Where(e => e.PendienteFamilia)
                            .Select(e => e.IdEstadoFamilia)
                            .FirstOrDefaultAsync();

                        if (actualizarUsuario.IdEstadoFamilia == 0)
                        {
                            var mensaje = _firebasetranslate.Traducir("No se encontró el estado pendiente familia.", lenguaje);

                            return new OperationResult(false, mensaje);
                        }
                    }
                    else if (rolActual.EsUsuarioFamilia && nuevoRol.EsAmbos)
                    {
                        // De familia a ambos, se asigna estado pendiente vendedor
                        actualizarUsuario.IdEstadoVendedor = await _context.EstadoVendedor
                            .Where(e => e.PendienteValidacion)
                            .Select(e => e.IdEstadoVendedor)
                            .FirstOrDefaultAsync();

                        if (actualizarUsuario.IdEstadoVendedor == 0)
                        {
                            var mensaje = _firebasetranslate.Traducir("No se encontró el estado pendiente vendedor.", lenguaje);

                            return new OperationResult(false, mensaje);
                        }
                    }
                    else if (rolActual.EsAmbos && nuevoRol.EsVendedor)
                    {
                        // De ambos a vendedor, eliminar relación con la familia
                        usuario.IdRolFamilia = null;
                        usuario.IdFamilia = null;
                        usuario.IdEstadoFamilia = estadoFamilia.Where(x => x.Inactivo).Select(x => x.IdEstadoFamilia).FirstOrDefault();
                    }
                    else if (rolActual.EsAmbos && nuevoRol.EsUsuarioFamilia)
                    {
                        // Obtener los servicios del usuario
                        var serviciosDelUsuario = await _context.Servicio
                            .Where(s => s.IdUsuario == usuario.IdUsuario)
                            .Select(s => s.IdServicio)
                            .ToListAsync();

                        // Validar que los contratos no tengan ninguno de estos servicios
                        var contratosConServicios = await _context.ContratoPersonal
                            .Where(c => serviciosDelUsuario.Contains(c.IdServicioContratado) && (c.Estado == EstadoContrato.Solicitado || c.Estado == EstadoContrato.EnCurso || c.Estado == EstadoContrato.Aceptado))
                            .AnyAsync();

                        if (contratosConServicios)
                        {
                            var mensaje = _firebasetranslate.Traducir("El usuario tiene contratos con servicios relacionados y no se puede cambiar a solo familia.", lenguaje);

                            return new OperationResult(false, mensaje);
                        }

                     

                        // De ambos a familia, eliminar relación con vendedor
                        usuario.IdEstadoVendedor = estadoVendedores.Where(x => x.Inactivo).Select(x => x.IdEstadoVendedor).FirstOrDefault();
                    }

                }

                // Actualizar las propiedades del usuario
                usuario.IdRolFamilia = actualizarUsuario.IdRolFamilia; /// cuando sea ambos y se lleve a uno solo debo eliminar la relacion con la familia validar a otro como admin en caso de que lo sea 
                usuario.IdFamilia = actualizarUsuario.IdFamilia; /// cuando sea ambos y se lleve a uno solo debo eliminar la relacion con la familia
                usuario.Nombre = actualizarUsuario.Nombre;
                usuario.FechaNacimiento = actualizarUsuario.FechaNacimiento;
                usuario.Correo = actualizarUsuario.Correo;
                usuario.Sexo = actualizarUsuario.Sexo;
                usuario.IdEstadoFamilia = actualizarUsuario.IdEstadoFamilia;
                usuario.IdEstadoVendedor = actualizarUsuario.IdEstadoVendedor;
                usuario.IdRol = actualizarUsuario.IdRol;
              

                usuario.Descripcion = actualizarUsuario.Descripcion;
                usuario.Valoracion = actualizarUsuario.Valoracion;
                usuario.CodigoVerificacion = actualizarUsuario.CodigoVerificacion;
                usuario.FechaLimiteCodigo = actualizarUsuario.FechaLimiteCodigo;

                // Calcular la edad
                var edad = DateTime.Now.Year - actualizarUsuario.FechaNacimiento.Year;
                if (actualizarUsuario.FechaNacimiento > DateTime.Now.AddYears(-edad))
                {
                    edad--;
                }

                // Validar si es mayor de edad y tiene documento
                if (edad >= 18)
                {
                    if (actualizarUsuario.IdTipoDocumento == null || string.IsNullOrWhiteSpace(actualizarUsuario.Documento))
                    {
                        var mensaje = _firebasetranslate.Traducir("Los usuarios mayores de edad deben tener un tipo de documento y un documento válido.", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                }
                usuario.IdTipoDocumento = edad >= 18 ? actualizarUsuario.IdTipoDocumento : null;
                usuario.Documento = edad >= 18 ? actualizarUsuario.Documento : "";

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Actualización exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, usuario.Nombre);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        public async Task<OperationResult> ObtenerUsuarioPorId(int usuarioId, string lenguaje = "es")
        {
            // Buscar si el usuario existe

            var usuario = await _context.Usuario
              .Include(u => u.Rol)
              .Include(u => u.EstadoVendedor)
              .Include(u => u.EstadoFamilia)
              .Where(x => x.IdUsuario == usuarioId)
              .Select(x => new UsuarioDto
              {
                  IdUsuario = x.IdUsuario,
                  Nombre = x.Nombre,
                  Correo = x.Correo,
                  FechaRegistro = x.FechaRegistro,
                  FechaNacimiento = x.FechaNacimiento,
                  Sexo = x.Sexo,
                  EstadoFamilia = x.EstadoFamilia,
                  EstadoVendedor = x.EstadoVendedor,
                  Rol = x.Rol,
                  RolFamilia = x.RolFamilia,
                  IdFamilia = x.IdFamilia,
                  Descripcion = x.Descripcion,
                  Valoracion = x.Valoracion ?? 0.0m,
                  FotoUrl = x.FotoUrl,
                  IdTipoDocumento = x.IdTipoDocumento,
                  Documento = x.Documento ?? "",
                  NombreFamilia = x.IdFamilia == null
                      ? "N/A"
                      : _context.Familia.Where(f => f.IdFamilia == x.IdFamilia).Select(f => f.Nombre).FirstOrDefault()
              })
              .FirstOrDefaultAsync();



            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                return new OperationResult(false, mensaje);
            }
             
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            var mensaje2 = _firebasetranslate.Traducir("Consulta exitosa.", lenguaje);

            return new OperationResult(true, mensaje2, usuario);
        }
        public async Task<OperationResult> SolicitarCodigoOTP(string correo, string lenguaje = "es")
        {
            {
                try
                {
                    //var usuario = await _context.Usuario.Where(x => x.Correo.ToLower() == correo.ToLower()).FirstOrDefaultAsync();
                    var usuario = await _context.Usuario.Where(x => x.Correo.ToLower() == correo.ToLower()).FirstOrDefaultAsync();

                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado en el sistema", lenguaje);

                    if (usuario == null) return new OperationResult(false, mensaje);
                    Random random = new Random();
                    const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    const string numeros = "0123456789";

                    string codigo = new string(Enumerable.Range(0, 2).Select(_ => letras[random.Next(letras.Length)]).ToArray()) +
                                    new string(Enumerable.Range(0, 2).Select(_ => numeros[random.Next(numeros.Length)]).ToArray());

                    codigo = new string(codigo.ToCharArray().OrderBy(x => random.Next()).ToArray());

                    var nombreUsuario = usuario.Nombre;

                    usuario.CodigoVerificacion = codigo;
                    usuario.FechaLimiteCodigo = DateTime.Now.AddMinutes(3);
                    await _context.SaveChangesAsync();
                    var mensajes = new EmailRequestDto()
                    {
                        Mensaje = @"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Creación de Usuario En Hiffy</title>
                        </head>
                        <body>
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Creación de Usuario En Hiffy</h2>
                                <p style=""color: #666;"">¡Hola " + nombreUsuario + @"!</p>
                                <p style=""color: #666;"">Has solicitado crear un usuario en nuestra plataforma. Por favor, utiliza el siguiente código OTP para continuar con el proceso:</p>

                                <!-- Código OTP -->
                                <div style=""margin: 20px auto; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #0066cc; text-align: center; font-size: 24px; font-weight: bold; max-width: 400px; border-radius: 5px;"">
                                    " + codigo + @"
                                </div>

                                <p style=""color: #666;"">Por favor, ten en cuenta que este código es de un solo uso y caduca en un corto período de tiempo.</p>
                                <p style=""color: #666;"">Si no solicitaste este cambio, puedes ignorar este mensaje.</p>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;"">
                                    <p style=""color: #666;"">Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>
                                    <p style=""color: #666;"">Gracias,<br>HIFFY Team</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                        Encabezado = "Solicitud de Código OTP",
                        EmailDestino = usuario.Correo
                    };
                    _emailService.EnviarCorreo(mensajes);

                    var mensaje2 = _firebasetranslate.Traducir("Se ha enviado un correo con su solicitud de reinicio de contraseña", lenguaje);

                    return new OperationResult(true, mensaje2, 1);
                }
                catch (Exception ex)
                {
                    return new OperationResult(false, ex.Message);
                }
            }


        }
      public async Task<OperationResult> EnviarMensajeAyuda(AyudaEnLineaDto dto, int usuarioId, string lenguaje = "es")
        {
            try
            {

                var foto = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png"; // Imagen por defecto
                if (usuarioId != 0)
                {
                    var usuario = await _context.Usuario.FirstOrDefaultAsync(x => x.IdUsuario == usuarioId);

                    if (usuario != null)
                    {
                        dto.NombreUsuario = usuario.Nombre;
                        // Validar si FotoUrl es null o vacía
                        foto = string.IsNullOrEmpty(usuario.FotoUrl)
                            ? "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png"
                            : "https://hiffyintec-001-site1.qtempurl.com/" + usuario.FotoUrl;
                        dto.CorreoUsuario = usuario.Correo;
                    }
                }

                // Construcción del mensaje HTML
                string mensajeHtml = @"
                    <!DOCTYPE html>
                    <html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Mensaje de Ayuda en Línea</title>
                    </head>
                    <body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f8; color: #333;"">
                        <div style=""max-width: 700px; margin: 20px auto; background: #ffffff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); overflow: hidden;"">
                
                            <!-- Encabezado -->
                            <div style=""background-color: #0066cc; color: #ffffff; padding: 20px; text-align: center;"">
                                <h1 style=""margin: 0; font-size: 24px;"">Nuevo Mensaje de Ayuda</h1>
                                <p style=""margin: 0; font-size: 16px;"">Un usuario ha solicitado asistencia</p>
                            </div>
                
                            <!-- Foto del usuario -->
                            <div style=""text-align: center; padding: 20px;"">
                                <img src=""" + foto + @""" alt=""Foto del Usuario"" style=""width: 120px; height: 120px; border-radius: 50%; border: 4px solid #0066cc; object-fit: cover;"">
                            </div>

                            <!-- Información del usuario -->
                            <div style=""padding: 20px;"">
                                <p style=""margin: 0; font-size: 18px;""><strong>Nombre del Usuario:</strong> " + dto.NombreUsuario + @"</p>
                                <p style=""margin: 10px 0; font-size: 18px;""><strong>Correo Electrónico:</strong> " + dto.CorreoUsuario + @"</p>
                                <p style=""margin: 10px 0; font-size: 18px;""><strong>Asunto:</strong> " + dto.Asunto + @"</p>
                            </div>

                            <!-- Mensaje del usuario -->
                            <div style=""padding: 20px; background-color: #f4f6f8; border-top: 4px solid #0066cc; border-radius: 5px; margin: 0 20px;"">
                                <h2 style=""color: #0066cc; font-size: 20px; margin-top: 0;"">Mensaje:</h2>
                                <p style=""font-size: 16px; line-height: 1.6; margin-bottom: 0;"">" + dto.MensajeUsuario + @"</p>
                            </div>

                            <!-- Footer -->
                            <div style=""padding: 20px; text-align: center; font-size: 14px; color: #777; border-top: 1px solid #eee; margin-top: 20px;"">
                                <p style=""margin: 0;"">Este mensaje fue enviado desde el sistema de ayuda en línea de <strong>HIFFY</strong>.</p>
                                <p style=""margin: 0;"">Gracias por atender esta solicitud.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Configuración del correo
                var emailRequest = new EmailRequestDto()
                {
                    Encabezado = "Nuevo Mensaje de Ayuda - "+ dto.Asunto,
                    EmailDestino = "HiffyIntec@gmail.com", 
                    Mensaje = mensajeHtml
                };

                // Llamar al servicio de correo
                _emailService.EnviarCorreo(emailRequest);

                var mensaje = _firebasetranslate.Traducir("Mensaje enviado correctamente", lenguaje);

                return new OperationResult(true, mensaje);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        public async Task<OperationResult> ValidarUsuarioOTP(string codigoOTP, string correo, string lenguaje = "es")
 

  
     
        {
            try
            {
                var usuario = await _context.Usuario.Where(x => x.CodigoVerificacion == codigoOTP && x.Correo.ToLower() == correo.ToLower()).FirstOrDefaultAsync();
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Código incorrecto.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                if (usuario.FechaLimiteCodigo < DateTime.Now)
                {
                    var mensaje = _firebasetranslate.Traducir("Código expirado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
 

                var rolusuario = await _context.Rol.Where(x => x.IdRol == usuario.IdRol).FirstOrDefaultAsync();

                // 1. Validación para Vendedor
                if (rolusuario.EsVendedor)
                {
                    // El usuario es solo Vendedor
                    var estadoPendienteValidacion = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.PendienteValidacion == true);
                    usuario.IdEstadoVendedor = estadoPendienteValidacion.IdEstadoVendedor;

                }

                // 2. Validación para Usuario Familia
                else if (rolusuario.EsUsuarioFamilia)
                {
                    // El usuario es solo parte de una Familia
                    var estadoPendienteFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.PendienteFamilia == true);
                    usuario.IdEstadoFamilia = estadoPendienteFamilia.IdEstadoFamilia;

                }

                // 3. Validación para Ambos (Vendedor y Familia)
                else if (rolusuario.EsAmbos)
                {
                    // El usuario tiene ambos roles, primero manejar la validación del estado de Familia
                    var estadoPendienteFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.PendienteFamilia == true);
                    usuario.IdEstadoFamilia = estadoPendienteFamilia.IdEstadoFamilia;


                    var estadoPendienteValidacion = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.PendienteValidacion == true);
                    usuario.IdEstadoVendedor = estadoPendienteValidacion.IdEstadoVendedor;

                }


                usuario.CodigoVerificacion = null;
                usuario.FechaLimiteCodigo = null;

                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Se ha validado su usuario con exito.", lenguaje);

                return new OperationResult(true, mensaje2, 1);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }
        public async Task<OperationResult> ValidarCodigoExistente(string codigoOTP, string correo, string lenguaje = "es")
        {
            var usuario = await _context.Usuario.Where(x => x.CodigoVerificacion == codigoOTP && x.Correo.ToLower() == correo.ToLower()).FirstOrDefaultAsync();
            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Código incorrecto.", lenguaje);

                return new OperationResult(false, mensaje, false);
            }

            if (usuario.FechaLimiteCodigo < DateTime.Now)
            {
                var mensaje = _firebasetranslate.Traducir("Código expirado.", lenguaje);

                return new OperationResult(false, mensaje, false);
            }
            var mensaje2 = _firebasetranslate.Traducir("Se ha validado el código.", lenguaje);

            return new OperationResult(true, mensaje2, true);
        }
        public async Task<OperationResult> ReseteoClaveUsuario(ReseteoClaveDto request, string lenguaje = "es")
        {
            try
            {
                var usuario = await _context.Usuario.Where(x => x.CodigoVerificacion == request.Codigo && x.Correo.ToLower() == request.Correo.ToLower()).FirstOrDefaultAsync();
                if (usuario == null)
                {
                    var mensaje2 = _firebasetranslate.Traducir("Código incorrecto.", lenguaje);

                    return new OperationResult(false, mensaje2);
                }

                if (usuario.FechaLimiteCodigo < DateTime.Now)
                {
                    var mensaje3 = _firebasetranslate.Traducir("Código expirado.", lenguaje);

                    return new OperationResult(false, mensaje3);
                }

                if (BCrypt.Net.BCrypt.Verify(request.Password, usuario.Contraseña))
                {
                    var mensaje3 = _firebasetranslate.Traducir("La nueva contraseña no puede ser igual a la contraseña anterior.", lenguaje);

                    return new OperationResult(false, mensaje3);
                }

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Password);
                usuario.CodigoVerificacion = null;
                usuario.FechaLimiteCodigo = null;

                await _context.SaveChangesAsync();

                var mensaje = new EmailRequestDto()
                {
                    Mensaje = @"
                        <!DOCTYPE html>
                        <html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Restablecimiento de contraseña exitoso</title>
                        </head>
                        <body>
                            <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                <!-- Encabezado con imagen -->
                                <div style=""text-align: center; margin-bottom: 20px;"">
                                    <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                </div>

                                <h2 style=""color: #333; text-align: center;"">Restablecimiento de contraseña exitoso</h2>
                                <p style=""color: #666;"">Hola <strong>" + usuario.Nombre + @"</strong>,</p>
                                <p style=""color: #666;"">Te informamos que se ha restablecido exitosamente la contraseña de tu cuenta. A partir de ahora, puedes acceder a tu cuenta utilizando la nueva contraseña proporcionada.</p>
                                <p style=""color: #666;"">Si no has solicitado este cambio o tienes alguna pregunta, por favor contáctanos de inmediato.</p>

                                <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;"">
                                    <p style=""color: #666;"">Gracias,<br>HIFFY Team</p>
                                </div>
                            </div>
                        </body> 
                        </html>
                    ",
                    Encabezado = "Restablecimiento de Contraseña Exitoso",
                    EmailDestino = usuario.Correo
                };
                _emailService.EnviarCorreo(mensaje);

                var resumenNotificacion = $@"
                    Se ha reseteado su clave exitosamente el día {DateTime.Now:dd/MM/yyyy} de la familia '{usuario.Familia.Nombre}'.
                    ";

                await _notificationService.SendNotificationAsync(
                "Se ha reseteado su clave con exito",
                resumenNotificacion,
                usuario.IdUsuario);
                var mensaje4 = _firebasetranslate.Traducir("Se ha reseteado su clave con exito.", lenguaje);

                return new OperationResult(true, mensaje4, 1);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
        public async Task<OperationResult> SolicitarReseteoClave(string correo, string lenguaje = "es")
        {
            {
                try
                {

                    var usuario = await _context.Usuario.Include(u => u.Rol).Where(x => x.Correo == correo).FirstOrDefaultAsync();

                    if (usuario == null)
                    {
                        var mensaje2 = _firebasetranslate.Traducir("Usuario Inexistente.", lenguaje);

                        return new OperationResult(false, mensaje2);
                    }

                    // Obtén el estado dependiendo del rol del usuario
                    if (usuario.Rol.EsVendedor)
                    {
                        var estadoVendedor = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.IdEstadoVendedor == usuario.IdEstadoVendedor);

                        // Validar el estado del vendedor
                        if (estadoVendedor.Inactivo || estadoVendedor.Suspendida)
                        {
                            var mensaje2 = _firebasetranslate.Traducir($"Su cuenta de vendedor se encuentra {estadoVendedor.Descripcion}", lenguaje);

                            return new OperationResult(false, mensaje2);
                        }
                    }

                    if (usuario.Rol.EsUsuarioFamilia)
                    {
                        var estadoFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.IdEstadoFamilia == usuario.IdEstadoFamilia);

                        // Validar el estado de familia
                        if (estadoFamilia.Suspendida || estadoFamilia.Inactivo)
                        {
                            var mensaje2 = _firebasetranslate.Traducir($"Su cuenta de familia se encuentra {estadoFamilia.Descripcion}", lenguaje);

                            return new OperationResult(false, mensaje2);
                        }
                    }

                    // Si el rol es ambos (familia y vendedor), validar ambos estados
                    if (usuario.Rol.EsAmbos)
                    { 
                        var estadoFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.IdEstadoFamilia == usuario.IdEstadoFamilia);
                         
                        if (estadoFamilia.Suspendida || estadoFamilia.Inactivo)
                        {
                            var mensaje2 = _firebasetranslate.Traducir($"Su cuenta de familia se encuentra {estadoFamilia.Descripcion}", lenguaje);

                            return new OperationResult(false, mensaje2);
                        }
                    }

                    Random random = new Random();
                    const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    const string numeros = "0123456789";

                    string codigo = new string(Enumerable.Range(0, 2).Select(_ => letras[random.Next(letras.Length)]).ToArray()) +
                                    new string(Enumerable.Range(0, 2).Select(_ => numeros[random.Next(numeros.Length)]).ToArray());

                    codigo = new string(codigo.ToCharArray().OrderBy(x => random.Next()).ToArray());

                    usuario.CodigoVerificacion = codigo;
                    usuario.FechaLimiteCodigo = DateTime.Now.AddMinutes(10);
                    await _context.SaveChangesAsync();
                    var mensaje = new EmailRequestDto()
                    {
                        Mensaje = @"
                            <!DOCTYPE html>
                            <html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Restablecimiento de contraseña</title>
                            </head>
                            <body>
                                <div style=""max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;"">

                                    <!-- Encabezado con imagen -->
                                    <div style=""text-align: center; margin-bottom: 20px;"">
                                        <img src=""https://hiffyintec-001-site1.qtempurl.com//mailImages/hiffy_logo.png"" alt=""HIFFY Logo"" style=""max-width: 200px; height: auto;"">
                                    </div>

                                    <h2 style=""color: #333; text-align: center;"">Restablecimiento de Contraseña</h2>
                                    <p style=""color: #666;"">¡Hola " + usuario.Nombre + @"!</p>
                                    <p style=""color: #666;"">Has solicitado restablecer tu contraseña en nuestra plataforma. Por favor, utiliza el siguiente código OTP para continuar con el proceso:</p>

                                    <!-- Código OTP -->
                                    <div style=""margin: 20px auto; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #0066cc; text-align: center; font-size: 24px; font-weight: bold; max-width: 400px; border-radius: 5px;"">
                                        " + codigo + @"
                                    </div>

                                    <p style=""color: #666;"">Por favor, ten en cuenta que este código es de un solo uso y caduca en un corto período de tiempo.</p>
                                    <p style=""color: #666;"">Si no solicitaste este cambio, puedes ignorar este mensaje.</p>

                                    <div style=""margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;"">
                                        <p style=""color: #666;"">Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>
                                        <p style=""color: #666;"">Gracias,<br>HIFFY Team</p>
                                    </div>
                                </div>
                            </body>
                            </html>
                            ",
                        Encabezado = "Solicitud de Código OTP",
                        EmailDestino = usuario.Correo
                    };
                    _emailService.EnviarCorreo(mensaje);

                    var mensaje3 = _firebasetranslate.Traducir("Se ha enviado un correo con su solicitud de reinicio de contraseña", lenguaje);

                    return new OperationResult(true, mensaje3);
                }
                catch (Exception ex)
                {
                    return new OperationResult(false, ex.Message);
                }
            }


        }
        public async Task<OperationResult> ListadoRolesUsuario( string lenguaje = "es")
        {
            var rolesUsuario = await _context.Rol.Select(rol => new RolDto
            {

                Descripcion = _firebasetranslate.Traducir(rol.Descripcion, lenguaje),
                EsAdmin = rol.EsAdmin,
                Nombre = _firebasetranslate.Traducir(rol.Nombre, lenguaje),
                EsAmbos = rol.EsAmbos,
                EsUsuarioFamilia = rol.EsUsuarioFamilia,
                EsVendedor = rol.EsVendedor,
                IdRol = rol.IdRol

            }).ToListAsync();

            var mensaje = _firebasetranslate.Traducir("Roles cargados exitosamente", lenguaje);

            return new OperationResult(true, mensaje, rolesUsuario);
        }
        public async Task<OperationResult> ListadoTiposDocumentos( string lenguaje = "es")
        {
            var tipoDocumentos = await _context.TipoDocumento.Select(x => new TipoDocumento
            {
                IdTipoDocumento = x.IdTipoDocumento,
                Nombre = x.Nombre

            }).ToListAsync();

            var mensaje = _firebasetranslate.Traducir("Tipos de Documentos cargados exitosamente", lenguaje);

            return new OperationResult(true, mensaje, tipoDocumentos);
        }
        public async Task<OperationResult> GetAllUsersByRole(int roleId, string lenguaje = "es")
        {
            try
            {
                var usuarios = await _context.Usuario
                    .Include(u => u.Rol)
                    .Include(u => u.EstadoFamilia)
                    .Include(u => u.EstadoVendedor)
                    .Include(u => u.RolFamilia)
                    .Where(u => u.IdRol == roleId)
                    .Select(u => new
                    {
                        IdUsuario = u.IdUsuario,
                        Correo = u.Correo,
                        Nombre = u.Nombre,
                        FechaNacimiento = u.FechaNacimiento,
                        FechaRegistro = u.FechaRegistro,
                        Rol = u.Rol,
                        Familia = new { u.Familia.Nombre, u.Familia.CodigoFamilia },
                        TieneCertificacionesPendientes = roleId == 2
                            ? _context.CertificacionVendedor
                                .Any(c => c.IdUsuario == u.IdUsuario &&
                                          !_context.CertificacionTipoServicio
                                            .Select(ct => ct.IdCertificacion)
                                            .Contains(c.IdCertificacion))
                            : false
                    })
                    .ToListAsync();

                var mensaje = _firebasetranslate.Traducir("Usuarios obtenidos con éxito", lenguaje);

                return new OperationResult(true, mensaje, usuarios);
            }
            catch (Exception ex)
            {
                var mensajeError = _firebasetranslate.Traducir($"Error al obtener usuarios: {ex.Message}", lenguaje);
                return new OperationResult(false, mensajeError);
            }
        }

        public async Task<OperationResult> SubirFoto(IFormFile file, int userId, string lenguaje = "es")
        {
            if (file == null || file.Length == 0)
            {
                var mensaje = _firebasetranslate.Traducir("No se ha proporcionado ninguna imagen.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            try
            {
                // Definir el nombre del archivo, por ejemplo, userId + extensión del archivo.
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";

                // Definir la ruta donde se almacenará el archivo (puedes cambiar esta ruta según tu entorno).
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                // Crear el directorio si no existe
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                // Guardar el archivo en la ruta
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Guardar la ruta o el nombre de la imagen en la base de datos si es necesario
                var usuario = await _context.Usuario.FindAsync(userId);
                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                usuario.FotoUrl = $"/uploads/{fileName}";
                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Foto subida exitosamente.", lenguaje);

                return new OperationResult(true, mensaje2, usuario.FotoUrl);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al subir la imagen: {ex.Message}");
            }
        }

        public async Task<Usuario?> ObtenerUsuarioPorIdV2(int idUsuario, string lenguaje = "es")
        {
            // Obtener el usuario por su ID
            return await _context.Usuario
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

    }
}
