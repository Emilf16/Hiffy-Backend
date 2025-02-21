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
using System.Security.Claims;

namespace HiffyWebApi.Controllers
{
    [Route("api/Family")]
    [ApiController]
    public class FamiliaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FamiliaRepo _familiaRepositorio;
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;


        public FamiliaController(IConfiguration configuration, AppDbContext context, FamiliaRepo familiaRepo, FirebaseTranslationService firebasetranslate)
        {
            _configuration = configuration;
            _context = context;
            _familiaRepositorio = familiaRepo;
            _firebasetranslate = firebasetranslate;

        }
        /// <summary>
        /// Returns all family members.
        /// </summary> 
        [HttpGet("get-family")]
        [Authorize]
        public async Task<OperationResult> MostrarFamilia()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                if (familiaIdClaim == null)
                {
                    return new OperationResult(false, "Error al validar usuario");
                }

                int familiaId = int.Parse(familiaIdClaim.Value);

                var familia = await _familiaRepositorio.MostrarFamilia(familiaId,lenguaje);

                
                return familia;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }


        /// <summary>
        /// Returns all family relationships.
        /// </summary> 
        [HttpGet("get-family-relationships")]
        //[Authorize]
        public async Task<OperationResult> ListadoRolesFamiliares()
        {
            try
            {

                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                var rolesFamilia = await _familiaRepositorio.ListadoRolesFamiliares(lenguaje);

                return rolesFamilia;

            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }

        }

        /// <summary>
        /// Creates a new family and associates it with the authenticated user.
        /// </summary>
        /// <param name="dto">Object containing the information of the family to be created.</param>
        /// <returns>Returns an <see cref="OperationResult"/> indicating the success or failure of the operation.</returns>
        [HttpPost]
        [Authorize] // Aseguramos que solo usuarios autenticados puedan acceder
        public async Task<OperationResult> PostFamilia([FromBody] PostFamilia dto)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            if (!ModelState.IsValid)
            {
                var mensaje = _firebasetranslate.Traducir("Datos inválidos", lenguaje);

                return new OperationResult(false, mensaje, ModelState);
            }

            // Extraer el ID del usuario desde el token
            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
            if (usuarioIdClaim == null)
            {
                var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                return new OperationResult(false, mensaje);
            }

            int usuarioId = int.Parse(usuarioIdClaim.Value);
            // Extraer el rol de la familia proporcionado desde el frontend
            var userRole = dto.RolFamilia; // Asumiendo que viene en el DTO

            var result = await _familiaRepositorio.CrearFamilia(dto, usuarioId, lenguaje);

            if (result.Success)
            {
                var mensaje = _firebasetranslate.Traducir("Familia creada exitosamente", lenguaje);

                return new OperationResult(true, mensaje, result.Data);
            }

            var mensaje2 = _firebasetranslate.Traducir("Error al crear la familia", lenguaje);

            return new OperationResult(false, mensaje2, result.Data);
        }

        /// <summary>
        /// Retrieves a family by its unique code.
        /// </summary>
        /// <param name="familyCode">The unique code of the family to search for.</param> 
        [HttpGet("GetByCode/{familyCode}")]
        [Authorize] // Requires authentication to access this resource
        public async Task<OperationResult> GetFamilyByCode(string familyCode)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            if (string.IsNullOrEmpty(familyCode))
            {
                var mensaje = _firebasetranslate.Traducir("El código de la familia no puede estar vacío.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var result = await _familiaRepositorio.ConsultarFamiliaPorCodigo(familyCode,lenguaje);

            if (result.Success)
            {
                var mensaje = _firebasetranslate.Traducir("Familia encontrada", lenguaje);
 
                return new OperationResult(true, mensaje, result.Data); // Return success with family data
            }
            var mensaje2 = _firebasetranslate.Traducir("Familia no encontrada", lenguaje);


            return new OperationResult(false, mensaje2); // Return failure if family not found
 
            return new OperationResult(false, "Familia no encontrada"); // Return failure if family not found
        }  
        
        /// <summary>
           /// Retrieves a family by its unique id.
           /// </summary>
           /// <param name="familyId">The unique id of the family to search for.</param> 
        [HttpGet("GetById/{familyId}")]
        [Authorize] // Requires authentication to access this resource
        public async Task<OperationResult> ConsultarFamiliaId(int familyId)
        {
          

            var result = await _familiaRepositorio.ConsultarFamiliaId(familyId);

            if (result.Success)
            {
                return new OperationResult(true, "Familia encontrada", result.Data); // Return success with family data
            }

            return new OperationResult(false, "Familia no encontrada"); // Return failure if family not found
 
        }

        /// <summary>
        /// Retrieves a family by its unique code.
        /// </summary>
        /// <param name="latitude">The unique code of the family to search for.</param> 
        /// <param name="longitude">The unique code of the family to search for.</param> 
        [HttpPost("Update-Family-Location/{latitude}/{longitude}")]
        [Authorize] // Requires authentication to access this resource
        public async Task<OperationResult> ActualizarUbicacionHogar(string latitude, string longitude)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
            if (usuarioIdClaim == null)
            {
                var mensaje = _firebasetranslate.Traducir("Error al validar usuario.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            int usuarioId = int.Parse(usuarioIdClaim.Value);

            var result = await _familiaRepositorio.ActualizarUbicacionHogar(usuarioId, latitude, longitude,lenguaje);

            return result;
        }


        [HttpPost("join-family/{familyId}/{roleFamilyId}")]
        [Authorize] // Ensures that only authenticated users can access
        /// <summary>
        /// Requests to join an existing family by assigning a role and marking the status as pending validation.
        /// </summary>
        /// <param name="familyId">The ID of the family to join.</param>
        /// <param name="roleFamilyId">The ID of the role in the family.</param>
        /// <returns>Returns an <see cref="OperationResult"/> indicating the success or failure of the operation.</returns>
        public async Task<OperationResult> UnirseAFamilia(int familyId, int roleFamilyId)
        {
            //IMPORTANTE
            //CUANDO NOS UNIMOS A UNA FAMILIA ES A MODO DE SOLICITUD
            //LE ASIGNAMOS EL ID DE LA FAMILIA PARA QUE EL ADMIN LO PUEDA VER COMO MIEMBROS
            //PERO LE DEJAMOS EL ESTADO EN PENDIENTE VALIDACION PARA QUE UN ADMIN PUEDA CAMBIARLO Y ACEPTARLO

            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            // Validar el estado del modelo
            if (!ModelState.IsValid)
            {
                var mensaje = _firebasetranslate.Traducir("Modelo inválido", lenguaje);

                return new OperationResult(false, mensaje);
            }

            // Extraer el ID del usuario desde el token
            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
            if (usuarioIdClaim == null)
            {
                var mensaje = _firebasetranslate.Traducir("Error al validar el usuario", lenguaje);

                return new OperationResult(false, mensaje);
            }

            int usuarioId = int.Parse(usuarioIdClaim.Value);

            // Llamar al método del repositorio para unirse a la familia
            var result = await _familiaRepositorio.UnirUsuarioAFamilia(usuarioId, familyId, roleFamilyId,lenguaje);

            if (result.Success)
            {
                var mensaje = _firebasetranslate.Traducir("Solicitud enviada a la familia exitosamente", lenguaje);

                return new OperationResult(true, mensaje, result.Data);
            }
            var mensaje2 = _firebasetranslate.Traducir("Error al solicitar unirse a la familia", lenguaje);

            return new OperationResult(false, mensaje2, result.Data);
        }

        [HttpGet("usuarios-pendientes-familia")]
        [Authorize]
        public async Task<OperationResult> ObtenerUsuariosPendientesFamilia()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Obtener el IdFamilia del usuario solicitante desde los claims
                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                if (familiaIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idFamilia = int.Parse(familiaIdClaim.Value);

                // Llamar al repositorio para obtener los usuarios pendientes
                var resultado = await _familiaRepositorio.ObtenerUsuariosPendientesFamilia(idFamilia,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Approve or reject a request to join the family.
        /// </summary>
        /// <param name="mail">Email of the user requesting to join.</param>
        /// <param name="approved">Boolean indicating whether the request is approved or rejected.</param>
        /// <param name="familyRoleId">Role ID of the family for which the request is being processed.</param> 
        [HttpPost("manage-family-request")]
        [Authorize]
        public async Task<OperationResult> AprobarORechazarSolicitud(string mail, bool approved, int familyRoleId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Validar si el correo es válido
                if (string.IsNullOrWhiteSpace(mail))
                {
                    var mensaje = _firebasetranslate.Traducir("El correo no puede estar vacío.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Obtener el resultado del repositorio
                var resultado = await _familiaRepositorio.AprobarORechazarSolicitud(mail, approved, familyRoleId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
        /// <summary>
        /// Removes a user from the family
        /// </summary>
        /// <param name="idUsuario">ID of the user to be removed</param>
        /// <returns>An OperationResult indicating success or failure</returns>
        [HttpDelete("remove-user-family/{idUsuario}")]
        [Authorize]  // Assuming only admins or the user itself can perform this action
        public async Task<OperationResult> RemoverUsuarioDeFamilia(int idUsuario)
        {
            try
            {
                // Validate if the user making the request is authorized
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    return new OperationResult(false, "Error al validar usuario.");
                }

                int usuarioSolicitanteId = int.Parse(usuarioIdClaim.Value);

                // Call the repository method to remove the user
                var resultado = await _familiaRepositorio.RemoverUsuarioDeFamilia(idUsuario, usuarioSolicitanteId);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Ocurrió un error al remover al usuario: {ex.Message}");
            }
        }


        /// <summary>
        /// Allows updating a user by a family member
        /// </summary>
        /// <param name="userDto">Model that receives data for the user</param> 
        [HttpPut("update-user-family")]
        [Authorize]  // Assuming only admins can update users 
        public async Task<OperationResult> ActualizarUsuarioFamiliar(ActualizarUsuarioFamiliaDto userDto)
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

             
                var resultado = await _familiaRepositorio.ActualizarUsuario(userDto, usuarioId,lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpGet("obtener-dashboard")]
        [Authorize]
        public async Task<OperationResult> ObtenerDashboard()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                 
                // Obtener IdFamilia e IdUsuario desde los claims
                var idFamiliaClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (idFamiliaClaim == null || idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontraron los claims necesarios.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idFamilia = int.Parse(idFamiliaClaim.Value);
                int idUsuario = int.Parse(idUsuarioClaim.Value);

                // Llamar al repositorio para obtener los datos
                var resultado = await _familiaRepositorio.ObtenerDashboard(idFamilia, idUsuario,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpGet("obtener-dashboardVendedor")]
        [Authorize]
        public async Task<OperationResult> ObtenerDashboardVendedor()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                int idUsuario = int.Parse(idUsuarioClaim.Value);

                // Llamar al repositorio para obtener los datos
                var resultado = await _familiaRepositorio.ObtenerDashboardVendedor(idUsuario,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpGet("obtener-dashboardAdmin")]
        [Authorize]
        public async Task<OperationResult> ObtenerDashboardAdmin()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _familiaRepositorio.ObtenerDashboardAdmin(lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }

}

