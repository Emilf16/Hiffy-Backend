using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiffyWebApi.Controllers
{
    [Route("Contract")]
    [ApiController]
    public class ContratoPersonalController : ControllerBase
    {
        private readonly ContratoPersonalRepositorio _contratoPersonalRepositorio;
        private readonly FirebaseTranslationService _firebasetranslate;


        public ContratoPersonalController(ContratoPersonalRepositorio contratoPersonalRepositorio, FirebaseTranslationService firebasetranslate
)
        {
            _contratoPersonalRepositorio = contratoPersonalRepositorio;
            _firebasetranslate = firebasetranslate;

        }


        /// <summary>
        /// Retrieves the details of a contract by its ID.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>An operation result containing the details of the assigned task.</returns>
        [HttpGet("get-contract-by-id/{contractId}")]
        [Authorize]
        public async Task<OperationResult> ObtenerTareaAsignadaPorId(int contractId)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {

                
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Call the service to get the assigned task by ID
                var resultado = await _contratoPersonalRepositorio.ObtenerContratoPorId(contractId, lenguaje);

                // Return the result of the operation
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Error retrieving the assigned task", lenguaje);
                return new OperationResult(false, $"{mensaje}: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows querying all contracts requested.
        /// </summary> 
        [HttpGet("my-requested-services")]
        [Authorize]
        public async Task<OperationResult> GetContratosSolicitados(bool isVendor)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int idRequerido = 0; 
                if (isVendor)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                    if (usuarioIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);
                        return new OperationResult(false, mensaje);
                    }
                    idRequerido = int.Parse(usuarioIdClaim.Value);
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);
                        return new OperationResult(false, mensaje);
                    }

                    idRequerido = int.Parse(familiaIdClaim.Value);
                }
             

                var contratos = await _contratoPersonalRepositorio.GetContratosVendedorPorEstado(idRequerido, EstadoContrato.Solicitado, isVendor, lenguaje);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);
                return new OperationResult(true, mensaje2, contratos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Allows querying all contracts accepted.
        /// </summary> 
        [HttpGet("my-accepted-services")]
        [Authorize]
        public async Task<OperationResult> GetContratosAceptados(bool isVendor)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int idRequerido = 0;
                if (isVendor)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                    if (usuarioIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                    idRequerido = int.Parse(usuarioIdClaim.Value);
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    idRequerido = int.Parse(familiaIdClaim.Value);
                }
                var contratos = await _contratoPersonalRepositorio.GetContratosVendedorPorEstado(idRequerido, EstadoContrato.Aceptado, isVendor, lenguaje);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, contratos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Allows querying all contracts accepted.
        /// </summary> 
        [HttpGet("my-ongoing-services")]
        [Authorize]
        public async Task<OperationResult> GetContratosEnCurso(bool isVendor)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int idRequerido = 0;
                if (isVendor)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                    if (usuarioIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                    idRequerido = int.Parse(usuarioIdClaim.Value);
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    idRequerido = int.Parse(familiaIdClaim.Value);
                }
                var contratos = await _contratoPersonalRepositorio.GetContratosVendedorPorEstado(idRequerido, EstadoContrato.EnCurso, isVendor, lenguaje);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, contratos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Allows querying all contracts requested.
        /// </summary> 
        [HttpGet("my-finished-services")]
        [Authorize]
        public async Task<OperationResult> GetContratosFinalizados(bool isVendor)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int idRequerido = 0;
                if (isVendor)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                    if (usuarioIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                    idRequerido = int.Parse(usuarioIdClaim.Value);
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    idRequerido = int.Parse(familiaIdClaim.Value);
                }


                var contratos = await _contratoPersonalRepositorio.GetContratosVendedorPorEstado(idRequerido, EstadoContrato.Finalizado, isVendor, lenguaje);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, contratos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
        /// <summary>
        /// Allows querying all contracts accepted.
        /// </summary> 
        [HttpGet("my-canceled-services")]
        [Authorize]
        public async Task<OperationResult> GetContratosCancelados(bool isVendor)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int idRequerido = 0;
                if (isVendor)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                    if (usuarioIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }
                    idRequerido = int.Parse(usuarioIdClaim.Value);
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                        return new OperationResult(false, mensaje);
                    }

                    idRequerido = int.Parse(familiaIdClaim.Value);
                }
                var contratos = await _contratoPersonalRepositorio.GetContratosVendedorPorEstado(idRequerido, EstadoContrato.Cancelado, isVendor, lenguaje);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, contratos);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Create a personal contract.
        /// </summary>
        /// <param name="contratoPersonal">The DTO containing the personal contract data to be created</param>
        [HttpPost]
        [Authorize]
        public async Task<OperationResult> CrearContratoPersonal([FromBody] PostContratoDto contratoPersonal)
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
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                    return new OperationResult(false, mensaje);
                }


                int familiaId = int.Parse(familiaIdClaim.Value);

                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                int idUsuario = int.Parse(usuarioIdClaim.Value);
                var resultado = await _contratoPersonalRepositorio.CrearContratoPersonal(contratoPersonal , familiaId, idUsuario, lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Accept a personal contract (assign verification code).
        /// </summary>
        /// <param name="contractId">The ID of the personal contract to be accepted</param>
        [HttpPut("Confirm/{contractId}")]
        [Authorize]
        public async Task<OperationResult> AceptarContratoPersonal(int contractId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.AceptarContratoPersonal(contractId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Start a personal contract (set state to 'In Progress' and request verification code).
        /// </summary>
        /// <param name="contractId">The ID of the personal contract to be started</param>
        /// <param name="verificationCode">The verification code provided for the contract</param>
        [HttpPut("Start/{contractId}")]
        [Authorize]
        public async Task<OperationResult> ComenzarContratoPersonal(int contractId, [FromQuery] int verificationCode)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.ComenzarContratoPersonal(contractId, verificationCode,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Cancel a personal contract (only if it is in 'Requested' or 'Accepted' state).
        /// </summary>
        /// <param name="contractId">The ID of the personal contract to be canceled</param>
        /// <param name="reason">The reason why the user canceled the contract</param>
        [HttpDelete("Cancel/vendor/{contractId}/{reason}")]
        [Authorize]
        public async Task<OperationResult> CancelarContratoPersonalVendedor(int contractId, string reason)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.CancelarContratoPersonalVendedor(contractId, reason,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Cancel a personal contract (only if it is in 'Requested' or 'Accepted' state).
        /// </summary>
        /// <param name="contractId">The ID of the personal contract to be canceled</param>
        /// <param name="reason">The reason why the user canceled the contract</param> 
        [HttpDelete("Cancel/family/{contractId}/{reason}")]
        [Authorize]
        public async Task<OperationResult> CancelarContratoPersonalFamilia(int contractId, string reason )
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.CancelarContratoPersonalFamilia(contractId, reason,lenguaje );

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

     

        /// <summary>
        /// Finalize a personal contract (only if it is in 'In Progress' state and requesting finalization code).
        /// </summary>
        /// <param name="contractId">The ID of the personal contract to be finalized</param>
        /// <param name="finalizationCode">The finalization code provided for the contract</param>
        [HttpPut("Finish/{contractId}")]
        [Authorize]
        public async Task<OperationResult> FinalizarContratoPersonal(int contractId, [FromQuery] int finalizationCode)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.FinalizarContratoPersonal(contractId, finalizationCode,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Get the verification code for a personal contract.
        /// </summary>
        /// <param name="contractId">The ID of the personal contract</param>
        [HttpGet("verificationCode/{contractId}")]
        [Authorize]
        public async Task<OperationResult> ObtenerCodigoVerificacion(int contractId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.ObtenerCodigoVerificacion(contractId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Get the finalization code for a personal contract.
        /// </summary>
        /// <param name="contractId">The ID of the personal contract</param>
        [HttpGet("finalizationCode/{contractId}")]
        [Authorize]
        public async Task<OperationResult> ObtenerCodigoFinalizacion(int contractId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _contratoPersonalRepositorio.ObtenerCodigoFinalizacion(contractId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpPost("RateContract")]
        public async Task<OperationResult> ValorarContrato( int idContrato,  int valoracion)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                if (valoracion < 1 || valoracion > 5)
                {
                    var mensaje = _firebasetranslate.Traducir("La valoración debe estar entre 1 y 5.", lenguaje);

                    return new OperationResult(false, mensaje);
                    
                }

                var resultado = await _contratoPersonalRepositorio.ValorarContratoAsync(idContrato, valoracion,lenguaje);

                return resultado;

                
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }
}
