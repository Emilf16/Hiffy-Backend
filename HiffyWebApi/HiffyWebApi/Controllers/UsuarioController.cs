using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace HiffyWebApi.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UsuarioRepo _usuarioRepositorio;
        private readonly NotificacionRepositorio _notificacionRepositorio;
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;



        public UsuarioController(IConfiguration configuration, AppDbContext context, UsuarioRepo usuarioRepositorio, NotificacionRepositorio notificacionRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _configuration = configuration;
            _context = context;
            _usuarioRepositorio = usuarioRepositorio;
            _notificacionRepositorio = notificacionRepositorio;
            _firebasetranslate = firebasetranslate;

        }

        /// <summary>
        /// Allows creating a user  
        /// </summary>
        /// <param name="userDto">Model that receives data for the user</param> 
        [HttpPost("create-user")]
        public async Task<OperationResult> CrearUsuario(CrearUsuarioDto userDto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var usuario = await _usuarioRepositorio.CrearUsuario(userDto,lenguaje);

                return usuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Allows updating a user by an admin  
        /// </summary>
        /// <param name="userDto">Model that receives data for the user</param> 
        [HttpPut("update-user-admin")]
        [Authorize]  // Assuming only admins can update users
        public async Task<OperationResult> ActualizarUsuarioAdmin(ActualizarUsuarioDto userDto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Validate if the user is an admin
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int usuarioId = int.Parse(usuarioIdClaim.Value);

                // Call the repository to update the user
                var resultado = await _usuarioRepositorio.ActualizarUsuario(userDto,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
         
        /// <summary>
        /// Allows Request OTP Code  
        /// </summary>
        /// <param name="mail">User mail to receive the OTP Code</param> 
        [HttpGet("request-otp/{mail}")]
        public async Task<OperationResult> SolicitarCodigoOTP(string mail)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var otp = await _usuarioRepositorio.SolicitarCodigoOTP(mail,lenguaje);

                return otp;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }


        /// <summary>
        /// Endpoint for sending help messages
        /// </summary>
        /// <param name="dto">Data required to send the message</param>
        [HttpPost("support")]
        [Authorize]  // Assuming only admins can update users
        public async Task<OperationResult> SendEmailSupportLoggedUser([FromBody] AyudaEnLineaDto dto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Validate if the user is an admin
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    return new OperationResult(false, "Error al validar usuario.");
                }

                int usuarioId = int.Parse(usuarioIdClaim.Value);


                var resultado = await _usuarioRepositorio.EnviarMensajeAyuda(dto, usuarioId,lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Endpoint for sending help messages
        /// </summary>
        /// <param name="dto">Data required to send the message</param>
        [HttpPost("external-support")] 
        public async Task<OperationResult> SendEmailSupportExternal([FromBody] AyudaEnLineaDto dto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int usuarioId = 0;
                
                var resultado = await _usuarioRepositorio.EnviarMensajeAyuda(dto, usuarioId,lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Allows to validate user through OTP Code  
        /// </summary>
        /// <param name="otpCode">OTP Code to validate User</param> 
        /// <param name="mail">User Mail to validate</param> 
        [HttpGet("validate-otp/{otpCode}/{mail}")]
        public async Task<OperationResult> ValidarUsuarioOTP(string otpCode, string mail)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var otp = await _usuarioRepositorio.ValidarUsuarioOTP(otpCode, mail,lenguaje);

                return otp;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Allows accessing the user's account and returning the token.
        /// </summary>
        /// <param name="correo">User's email</param>
        /// <param name="password">User's email password</param> 
        [HttpPost("login")]
        public async Task<OperationResult> IniciarSesion(string correo, string password)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var usuario = await _context.Usuario.Include(u => u.Rol).Where(x => x.Correo.Trim().ToLower() == correo.Trim().ToLower()).FirstOrDefaultAsync();

                var mensaje = _firebasetranslate.Traducir("Usuario o contraseña invalidos", lenguaje);

                if (usuario == null) return new OperationResult(false, mensaje);


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
                        var mensaje3 = _firebasetranslate.Traducir($"Su cuenta de familia se encuentra {estadoFamilia.Descripcion}", lenguaje);

                        return new OperationResult(false, mensaje3);
                    }
                }


                // Si el rol es ambos (familia y vendedor), validar ambos estados
                if (usuario.Rol.EsAmbos)
                {
                    var estadoFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.IdEstadoFamilia == usuario.IdEstadoFamilia);
                    var estadoVendedor = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.IdEstadoVendedor == usuario.IdEstadoVendedor);

                    // Verificar si ambos estados están inactivos o suspendidos
                    if ((estadoFamilia.Suspendida || estadoFamilia.Inactivo) &&
                        (estadoVendedor.Inactivo || estadoVendedor.Suspendida))
                    {
                        var mensaje4 = _firebasetranslate.Traducir("Ambas cuentas (familia y vendedor) están inactivas o suspendidas. No puede iniciar sesión.", lenguaje);

                        return new OperationResult(false, mensaje4);
                    }
                }


                if (!BCrypt.Net.BCrypt.Verify(password, usuario.Contraseña))
                {
                    var mensaje5 = _firebasetranslate.Traducir("Contraseña Incorrecta", lenguaje);

                    return new OperationResult(false, mensaje5);
                }

                var token = CreateToken(usuario);

                var userDto = new UsuarioDto
                {
                    Correo = usuario.Correo,
                    Nombre = usuario.Nombre,

                    IdRol = usuario.IdRol,
                    IdFamilia = usuario.IdFamilia,
                    FechaNacimiento = usuario.FechaNacimiento,
                    Rol = _context.Rol.Where(y => y.IdRol == usuario.IdRol).FirstOrDefault(),
                    EstadoFamilia = _context.EstadoFamilia.Where(y => y.IdEstadoFamilia == usuario.IdEstadoFamilia).FirstOrDefault(),
                    EstadoVendedor = _context.EstadoVendedor.Where(y => y.IdEstadoVendedor == usuario.IdEstadoVendedor).FirstOrDefault(),
                    RolFamilia = _context.RolFamilia.Where(y => y.IdRolFamilia == usuario.IdRolFamilia).FirstOrDefault() 

                };

                var mensaje6 = _firebasetranslate.Traducir("Exito al iniciar sesión", lenguaje);


                return new OperationResult(true, mensaje6, userDto, token);

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Allows to request a password reset.
        /// </summary>
        /// <param name="email">Email of the user to request the password reset</param> 
        [HttpPost("forgot-password")]
        public async Task<OperationResult> SolicitarReseteoClave(string email)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var usuario = await _usuarioRepositorio.SolicitarReseteoClave(email,lenguaje);

                return usuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Allows resetting the password with the OTP sent to the email.
        /// </summary>
        /// <param name="request">This JSON contains the OTP and the new password</param> 
        [HttpPost("reset-password")]
        public async Task<OperationResult> ReseteoClaveUsuario(ReseteoClaveDto request)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var usuario = await _usuarioRepositorio.ReseteoClaveUsuario(request,lenguaje);

                return usuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }
        
        /// <summary>
        /// Allows to validate if otp code is valide.
        /// </summary>
        /// <param name="otpCode">OTP Code to validate User</param> 
        /// <param name="mail">User Mail to validate</param> 
        [HttpPost("validate-code-exist")]
        public async Task<OperationResult> ValidarCodigoExistente(string otpCode, string mail)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var validacion = await _usuarioRepositorio.ValidarCodigoExistente(otpCode, mail,lenguaje);

                return validacion;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Returns all user roles.
        /// </summary> 
        [HttpGet("get-user-roles")] 
        public async Task<OperationResult> ListadoRolesFamiliares()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var rolesUsuario = await _usuarioRepositorio.ListadoRolesUsuario(lenguaje);

                return rolesUsuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Returns all document types.
        /// </summary> 
        [HttpGet("get-document-types")]
        public async Task<OperationResult> ListadoTiposDocumentos()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var rolesUsuario = await _usuarioRepositorio.ListadoTiposDocumentos(lenguaje);

                return rolesUsuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Get user information based on the provided token and refresh token if needed.
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="newFamily">user has created a family</param>
        /// <returns>User information from the token</returns>
        [HttpGet("get-user-info")]
        public async Task<OperationResult> ObtenerUsuarioPorToken(string token, bool newFamily)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Decodificar y validar el token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // Aquí no validamos la expiración aún
                    ClockSkew = TimeSpan.Zero
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Verificar si el token ha expirado
                var expirationDateUnix = long.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value!);
                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expirationDateUnix).UtcDateTime;

                if (expirationDate < DateTime.UtcNow)
                {
                    var mensaje = _firebasetranslate.Traducir("El token ha expirado, por favor inicie sesión nuevamente.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Si el token expira en menos de 10 minutos, generar uno nuevo
                var timeToExpire = expirationDate - DateTime.UtcNow;
                string newToken = null;
                if (timeToExpire.TotalMinutes < 10 || newFamily)
                {
                    var correo = claimsPrincipal.FindFirst(ClaimsService.Correo)?.Value;
                    var user = await _context.Usuario.Include(u => u.Rol)
                                                     .Where(x => x.Correo.Trim().ToLower() == correo.Trim().ToLower())
                                                     .FirstOrDefaultAsync();
                    if (user != null)
                    {
                        newToken = CreateToken(user); // Generar nuevo token
                    }
                }

                // Extraer las reclamaciones (claims) del token
                var correoClaim = claimsPrincipal.FindFirst(ClaimsService.Correo)?.Value;
                var idUsuarioClaim = claimsPrincipal.FindFirst(ClaimsService.IdUsuario)?.Value;

                if (correoClaim == null || idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Token inválido o sin información de usuario", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Buscar al usuario en la base de datos
                var usuario = await _context.Usuario.Include(u => u.Rol)
                                                    .Where(x => x.Correo.Trim().ToLower() == correoClaim.Trim().ToLower())
                                                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Usuario no encontrado", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Validar el estado del vendedor o familia si aplica
                if (usuario.Rol.EsVendedor)
                {
                    var estadoVendedor = await _context.EstadoVendedor.FirstOrDefaultAsync(x => x.IdEstadoVendedor == usuario.IdEstadoVendedor);
                    if (estadoVendedor.Inactivo || estadoVendedor.Suspendida)
                    {
                        var mensaje = _firebasetranslate.Traducir($"Su cuenta de vendedor se encuentra {estadoVendedor.Descripcion}", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                }

                if (usuario.Rol.EsUsuarioFamilia || usuario.Rol.EsAmbos)
                {
                    var estadoFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(x => x.IdEstadoFamilia == usuario.IdEstadoFamilia);
                    if (estadoFamilia.Suspendida || estadoFamilia.Inactivo)
                    {
                        var mensaje = _firebasetranslate.Traducir($"Su cuenta de familia se encuentra {estadoFamilia.Descripcion}", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                }

                // Crear el DTO del usuario con la información necesaria
                var userDto = new UsuarioDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Correo = usuario.Correo,
                    Nombre = usuario.Nombre,
                    IdRol = usuario.IdRol,
                    IdFamilia = usuario.IdFamilia,
                    FechaNacimiento = usuario.FechaNacimiento,
                    FotoUrl = usuario.FotoUrl,
                    Rol = await _context.Rol.FirstOrDefaultAsync(y => y.IdRol == usuario.IdRol),
                    EstadoFamilia = await _context.EstadoFamilia.FirstOrDefaultAsync(y => y.IdEstadoFamilia == usuario.IdEstadoFamilia),
                    EstadoVendedor = await _context.EstadoVendedor.FirstOrDefaultAsync(y => y.IdEstadoVendedor == usuario.IdEstadoVendedor),
                    RolFamilia = await _context.RolFamilia.FirstOrDefaultAsync(y => y.IdRolFamilia == usuario.IdRolFamilia),
                    Sexo = usuario.Sexo,
                    Descripcion = usuario.Descripcion,
                    Documento = usuario.Documento,
                    FechaRegistro = usuario.FechaRegistro,
                    IdTipoDocumento = usuario.IdTipoDocumento,
                    TipoDocumento = usuario.TipoDocumento,
                    Valoracion = usuario.Valoracion,
                    Longitud = usuario.Longitud,
                    Latitud = usuario.Altitud
                };
                var mensaje2 = _firebasetranslate.Traducir("Usuario obtenido con éxito", lenguaje);


                return new OperationResult(true, mensaje2, userDto, newToken ?? token);

            }
            catch (SecurityTokenExpiredException)
            {
                return new OperationResult(false, "El token ha expirado, por favor inicie sesión nuevamente.");
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Returns all users by roleId.
        /// </summary>
        /// <param name="roleId">The user´s role.</param>
        [HttpGet("get-users-by-role/{roleId}")]
        //[Authorize]
        public async Task<OperationResult> GetAllUsersByRole(int roleId)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            var resultado = await _usuarioRepositorio.GetAllUsersByRole(roleId,lenguaje);
            return resultado;
        }

        /// <summary>
        /// Allows to upload a user's profile picture.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        [Authorize]
        [HttpPost("upload-profile-photo")]
        public async Task<OperationResult> SubirFoto([FromForm]IFormFile file)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Extraer el userId del token JWT

                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convertir el claim a un entero (asumiendo que es un int)
                int userId = int.Parse(usuarioIdClaim.Value);

                // Llamar al método del repositorio para subir la foto
                var resultado = await _usuarioRepositorio.SubirFoto(file, userId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
        
        /// <summary>
        /// Returns an user by id.
        /// </summary> 
        [HttpGet("get-user-by-id/{userId}")]
        public async Task<OperationResult> ObtenerUsuarioPorId(int userId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var usuario = await _usuarioRepositorio.ObtenerUsuarioPorId(userId,lenguaje);

                return usuario;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        [HttpGet("notificaciones")]
        [Authorize]
        public async Task<OperationResult> ObtenerNotificacionesPorUsuario()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdUsuario desde los claims
                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);

                // Llamar al repositorio para obtener las notificaciones
                var resultado = await _notificacionRepositorio.ObtenerNotificacionesPorUsuario(idUsuario,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpDelete("MarcarComoLeido")]
        public async Task<IActionResult> MarcarComoLeido(int idNotificacion)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            var result = await _notificacionRepositorio.EliminarNotificacionAsync(idNotificacion,lenguaje);

            if (!result.Success)
            {
                return NotFound(result); 
            }

            return Ok(result); 
        }




        private string CreateToken(Usuario user)
        {
            List<Claim> claims = new List<Claim> {
          new Claim(ClaimsService.Correo, user.Correo),
          new Claim(ClaimsService.UsuaNombre, user.Nombre),
          new Claim(ClaimsService.IdUsuario, user.IdUsuario.ToString()),
          new Claim(ClaimsService.RolId, user.IdRol.ToString()),
          new Claim(ClaimsService.IdFamilia, user.IdFamilia.ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(40),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
