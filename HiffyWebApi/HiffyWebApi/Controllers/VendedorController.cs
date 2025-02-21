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

namespace HiffyWebApi.Controllers
{
    [Route("api/Vendor")]
    [ApiController]
    public class VendedorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly VendedorRepo _vendedorRepo;
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;


        public VendedorController(IConfiguration configuration, AppDbContext context, VendedorRepo vendedorRepo, FirebaseTranslationService firebasetranslate)
        {
            _configuration = configuration;
            _context = context;
            _vendedorRepo = vendedorRepo;
            _firebasetranslate = firebasetranslate;

        }

        /// <summary>
        /// Retrieves the list of certifications for a user, including approved, rejected, and pending certifications.
        /// </summary>
        /// <returns>An IActionResult with the result of the operation, including the certifications' status.</returns>
        [Authorize]
        [HttpGet("get-certifications/{usuarioId}")]
        public async Task<OperationResult> ObtenerCertificaciones([FromRoute]int usuarioId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Call the repository method to get the certifications for the user
                var resultado = await _vendedorRepo.ObtenerCertificacionesAprobadasOListado(usuarioId,lenguaje);

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Uploads multiple certifications for a user.
        /// </summary>
        /// <param name="certificacionDtos">The list of DTOs containing certification details including title, description, and files.</param>
        /// <returns>An IActionResult indicating the result of the upload operation.</returns>
        [Authorize]
        [HttpPost("upload-certification")]
        public async Task<OperationResult> SubirCertificacion([FromForm] List<PostCertificacionDto> certificacionDtos)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Extract the userId from the JWT token
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "usuarioId");
                if (userIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(userIdClaim.Value);

                // Call the repository method to upload the certifications
                var resultado = await _vendedorRepo.SubirCertificacionesAsync(usuarioId, certificacionDtos,lenguaje);

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Associates a certification with a list of service types, or deletes the certification if no service types are selected.
        /// </summary>
        /// <param name="certificacionTipoServicioDto">A DTO containing the certification ID, a list of service type IDs, and a flag indicating whether to approve the association or delete the certification.</param>
        /// <returns>An OperationResult indicating the success or failure of the association or deletion operation.</returns>
        [Authorize]
        [HttpPost("asociate-certification-types")]
        public async Task<OperationResult> AsociarCertificacionTipoServicio([FromBody] List<CertificacionTipoServicioDto> certificacionTipoServicioDto )
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Call the repository method to associate the certification with service types or delete it
                var resultado = await _vendedorRepo.AsociarCertificacionTipoServicio(certificacionTipoServicioDto,lenguaje );

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Checks if any certifications have been approved (i.e., associated with service types).
        /// If no certifications have been approved, it returns the list of all certifications.
        /// </summary>
        /// <returns>An OperationResult containing the list of certifications or a message indicating no approved certifications.</returns>
        [Authorize]
        [HttpGet("check-certification-status")]
        public async Task<OperationResult> ObtenerCertificacionesAprobadasOListado ()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Extract the userId from the JWT token
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "usuarioId");
                if (userIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(userIdClaim.Value);

                // Call the repository method to associate the certification with service types or delete it
                var resultado = await _vendedorRepo.ObtenerCertificacionesAprobadasOListado(usuarioId,lenguaje);

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Deletes a pending certification by its ID, including associated service types and files.
        /// </summary>
        /// <param name="certificationId">The ID of the certification to delete.</param>
        /// <returns>An OperationResult indicating the success or failure of the deletion operation.</returns>
        /// 
        [Authorize]
        [HttpDelete("eliminate-pending-certificacion/{certificationId}")]
        public async Task<OperationResult> EliminarCertificacionPendiente(int certificationId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Call the repository method to delete the pending certification
                var resultado = await _vendedorRepo.EliminarCertificacionPendiente(certificationId,lenguaje);

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Retrieves the list of service types associated with the seller's certifications.
        /// If the seller has no approved certifications (i.e., no associated service types), 
        /// it returns all certifications for the user.
        /// </summary>
        /// <returns>
        /// An <see cref="OperationResult"/> containing either:
        /// - A list of service types approved for the seller if certifications are associated with service types.
        /// - A message indicating no approved certifications if none exist.
        /// </returns>
        [Authorize]
        [HttpGet("service-types-approved")]
        public async Task<OperationResult> ObtenerTiposServiciosPermitidos()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Extract the userId from the JWT token
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "usuarioId");
                if (userIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(userIdClaim.Value);

                // Call the repository method to associate the certification with service types or delete it
                var resultado = await _vendedorRepo.ObtenerTiposServiciosPermitidos(usuarioId,lenguaje);

                return resultado; // Return the result as OperationResult
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpGet("listar-vendedores")]
        [Authorize]
        public async Task<OperationResult> ListaUsuarios()
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            var resultado = await _vendedorRepo.ObtenerVendedores(lenguaje);
            return resultado;
        }
    }
}
