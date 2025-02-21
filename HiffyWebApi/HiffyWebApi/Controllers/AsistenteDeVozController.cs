using Google.Cloud.Translation.V2;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace HiffyWebApi.Controllers
{
    [Route("api/VoiceAssistant")]
    [ApiController]
    public class AsistenteDeVozController : ControllerBase
    {
        private readonly AsistenteDeVozRepo _asistenteDeVozRepo;
        private readonly ContratoPersonalRepositorio _contratoPersonalRepositorio;
        private readonly FirebaseTranslationService _firebasetranslate;

        public AsistenteDeVozController(
            AsistenteDeVozRepo asistenteDeVozRepo,
            ContratoPersonalRepositorio contratoPersonalRepositorio,
            FirebaseTranslationService firebasetranslate)
        {
            _asistenteDeVozRepo = asistenteDeVozRepo;
            _contratoPersonalRepositorio = contratoPersonalRepositorio;
            _firebasetranslate = firebasetranslate;
        }

        // Lista estática para almacenar las asignaciones
        private static List<PostAsignacionDto> _asignaciones = new List<PostAsignacionDto>();

        [Authorize]
        [HttpPut("client/update-device")]
        public async Task<OperationResult> PutActualizarDatosDispositivo([FromBody] PutDispositivoFamilia device)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                int usuarioId = int.Parse(usuarioIdClaim.Value);
                var actualizar = await _asistenteDeVozRepo.PutActualizarDatosDispositivo(device, usuarioId, lenguaje);
                if (!actualizar.Success)
                {
                    actualizar.Message = _firebasetranslate.Traducir(actualizar.Message, lenguaje);
                }
                return actualizar;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [Authorize]
        [HttpDelete("client/delete-device/{deviceId}")]
        public async Task<OperationResult> DeleteDispositivoFamilia(string deviceId)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                int usuarioId = int.Parse(usuarioIdClaim.Value);
                var delete = await _asistenteDeVozRepo.DeleteDispositivoFamilia(deviceId, usuarioId, lenguaje);
                if (!delete.Success)
                {
                    delete.Message = _firebasetranslate.Traducir(delete.Message, lenguaje);
                }
                return delete;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [Authorize]
        [HttpGet("client/get-my-devices")]
        public async Task<OperationResult> GetMisDispositivos()
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

            var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
            if (familiaIdClaim == null)
            {
                var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            int familiaId = int.Parse(familiaIdClaim.Value);
            var dispositivos = await _asistenteDeVozRepo.GetMisDispositivos(familiaId, lenguaje);
            if (!dispositivos.Success)
            {
                dispositivos.Message = _firebasetranslate.Traducir(dispositivos.Message, lenguaje);
            }
            return dispositivos;
        }

        [HttpPost("register-device")]
        public async Task<OperationResult> PostRegistrarDispositivo([FromBody] PostDispositivoDto dto, [FromQuery] int familyCode)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var registrar = await _asistenteDeVozRepo.PostRegistrarDispositivo(dto, familyCode, lenguaje);
                if (!registrar.Success)
                {
                    registrar.Message = _firebasetranslate.Traducir(registrar.Message, lenguaje);
                }
                return registrar;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpGet("get-family-info/{deviceCode}")]
        public async Task<OperationResult> MostrarFamilia(string deviceCode)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var datosFamiliaries = await _asistenteDeVozRepo.GetInformacionFamiliar(deviceCode, lenguaje);
                if (!datosFamiliaries.Success)
                {
                    datosFamiliaries.Message = _firebasetranslate.Traducir(datosFamiliaries.Message, lenguaje);
                }
                return datosFamiliaries;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpGet("get-home-areas/{familyCode}/{edadUsuarioMayor}")]
        public async Task<OperationResult> GetAreasDelHogarFamiliar(string familyCode, int edadUsuarioMayor)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var resultado = await _asistenteDeVozRepo.GetAreasDelHogarFamiliar(familyCode, edadUsuarioMayor, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Ocurrió un error al obtener las áreas del hogar: " + ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpGet("validate-device/{deviceCode}")]
        public async Task<OperationResult> ValidarDispositivo(string deviceCode)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var resultado = await _asistenteDeVozRepo.ValidarDispositivo(deviceCode, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Ocurrió un error al consultar la asignación", lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpPost("post-assignment/{deviceCode}")]
        public async Task<OperationResult> PostAsignacion(string deviceCode, [FromBody] TareaAsignadaDto dto)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                var resultado = await _asistenteDeVozRepo.PostAsignacion(deviceCode, dto, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir("Ocurrió un error al registrar la asignación", lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpGet("get-assignments-by-date/{deviceCode}/{date}")]
        public async Task<OperationResult> GetAsignacionesByDate(string deviceCode, DateTime date)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                if (string.IsNullOrEmpty(deviceCode))
                {
                    var mensaje = _firebasetranslate.Traducir("Se necesita el código del dispositivo.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _asistenteDeVozRepo.GetAsignacionesPorFecha(deviceCode, date, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpGet("get-contracts-by-date/{deviceCode}/{date}/{contractStatus}")]
        public async Task<OperationResult> GetContractsByDate(string deviceCode, DateTime date, EstadoContrato contractStatus)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                if (string.IsNullOrEmpty(deviceCode))
                {
                    var mensaje = _firebasetranslate.Traducir("Se necesita el código del dispositivo.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _asistenteDeVozRepo.GetContratosPorFecha(deviceCode, date, contractStatus, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpDelete("delete-assignment/{deviceCode}/{id}")]
        public async Task<OperationResult> DeleteTask(string deviceCode, int id)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                if (string.IsNullOrEmpty(deviceCode))
                {
                    var mensaje = _firebasetranslate.Traducir("Se necesita el código del dispositivo.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _asistenteDeVozRepo.DeleteAsignacionPorId(deviceCode, id, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpDelete("delete-contract/{deviceCode}/{contractId}/{reason}")]
        public async Task<OperationResult> DeleteContract(string deviceCode, int contractId, string reason)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            try
            {
                
                if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

                if (string.IsNullOrEmpty(deviceCode))
                {
                    var mensaje = _firebasetranslate.Traducir("Se necesita el código del dispositivo.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _contratoPersonalRepositorio.CancelarContratoPersonalFamilia(contractId, reason, lenguaje);
                if (!resultado.Success)
                {
                    resultado.Message = _firebasetranslate.Traducir(resultado.Message, lenguaje);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                var mensaje = _firebasetranslate.Traducir(ex.Message, lenguaje);
                return new OperationResult(false, mensaje);
            }
        }

        [HttpDelete("clear-assignments/{deviceCode}")]
        public async Task<OperationResult> ClearAsignaciones(string deviceCode)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje)) lenguaje = "es";

            _asignaciones.RemoveAll(a => a.DeviceCode == deviceCode);
            var mensaje = _firebasetranslate.Traducir("Todas las asignaciones fueron eliminadas exitosamente para el dispositivo", lenguaje);
            return new OperationResult(true, mensaje);
        }
    }



}
