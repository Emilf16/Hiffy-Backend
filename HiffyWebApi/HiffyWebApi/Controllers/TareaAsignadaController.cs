using Google.Cloud.Translation.V2;
using Hiffy_Datos;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Hiffy_Entidades.Entidades.TareaAsignada;

namespace HiffyWebApi.Controllers
{
    [Route("api/assignedtask")]
    [ApiController]
    public class TareaAsignadaController : ControllerBase
    {
        private readonly TareaAsignadaRepo _tareaAsignadaRepositorio;
        private readonly FirebaseTranslationService _firebasetranslate;


        public TareaAsignadaController(TareaAsignadaRepo tareaAsignadaRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _tareaAsignadaRepositorio = tareaAsignadaRepositorio;
            _firebasetranslate = firebasetranslate;

        }

        /// <summary>
        /// Create a new assigned task.
        /// </summary>
        /// <param name="tareaAsignadaDto">The data of the assigned task</param>
        [HttpPost]
        [Authorize]
        public async Task<OperationResult> CrearTareaAsignada(TareaAsignadaDto tareaAsignadaDto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdUsuario de los claims
                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);
                //int idUsuario = 1;

                // Llamar al repositorio para crear la tarea asignada
                var resultado = await _tareaAsignadaRepositorio.CrearTareaAsignada(idUsuario, tareaAsignadaDto, lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Delete an assigned task.
        /// </summary>
        /// <param name="id">The id of the assigned task</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<OperationResult> EliminarTareaAsignada(int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdUsuario de los claims
                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);
                //int idUsuario = 1;

                // Llamar al repositorio para crear la tarea asignada
                var resultado = await _tareaAsignadaRepositorio.EliminarTareaAsignada(idUsuario, id, lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Edit an assigned task.
        /// </summary>
        /// <param name="tareaAsignadaDto">The data of the assigned task</param>
        [HttpPut]
        [Authorize]
        public async Task<OperationResult> EditarTareaAsignada(TareaAsignadaDto tareaAsignadaDto)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdUsuario de los claims
                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (idUsuarioClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);
                //int idUsuario = 1;

                // Llamar al repositorio para crear la tarea asignada
                var resultado = await _tareaAsignadaRepositorio.EditarTareaAsignada(idUsuario, tareaAsignadaDto,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Get the tasks assigned by Family grouped by date range.
        /// </summary>
        [HttpGet("get-task-family-admin")]
        [Authorize]
        public async Task<OperationResult> ObtenerTareasPorFamilia(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdFamilia de los claims
                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);

                if (familiaIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int familiaId = int.Parse(familiaIdClaim.Value);
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);

                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int usuarioId = int.Parse(usuarioIdClaim.Value);
                // Llamar al repositorio para obtener las tareas asignadas en el rango de fechas
                var resultado = await _tareaAsignadaRepositorio.ObtenerTareasPorFamilia(usuarioId,familiaId, fechaInicio, fechaFin,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        } 

        /// <summary>
        /// Gets the tasks assigned by FamilyId, grouped by day.
        /// </summary>
        [HttpGet("get-task-calendar")]
        [Authorize]
        public async Task<OperationResult> ObtenerTareasPorFamiliaCalendario(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Obtener el IdFamilia de los claims
                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);

                if (familiaIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                int familiaId = int.Parse(familiaIdClaim.Value);

                // Llamar al repositorio para obtener las tareas asignadas en el rango de fechas
                var resultado = await _tareaAsignadaRepositorio.ObtenerTareasYContratosPorFamilia(familiaId, fechaInicio, fechaFin,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Gets a task assigned by its ID.
        /// </summary>
        /// <param name="id">ID de la tarea asignada.</param>
        /// <returns>La tarea asignada si existe.</returns>
        [HttpGet("get-task-by-id/{id}")]
        [Authorize]
        public async Task<OperationResult> ObtenerTareaPorId(int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdFamilia de los claims
                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);

                if (familiaIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                // Llamar al repositorio para obtener la tarea por ID
                var resultado = await _tareaAsignadaRepositorio.ObtenerTareaAsignadaPorId(id,lenguaje);
                  
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Completes a recurrent task and updates the main task's status if all recurrent tasks are completed.
        /// </summary>
        /// <param name="recurrentId">ID of the recurrent task to complete.</param>
        /// <param name="newStatusId">New status to assign to the recurrent task.</param> 
        [HttpPut("complete-recurrent-task")]
        [Authorize]
        public async Task<OperationResult> CompletarTareaRecurrente(int recurrentId, EstadoTarea newStatusId)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Obtener el IdFamilia de los claims
                var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);

                if (familiaIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);

                    return new OperationResult(false, mensaje);
                }
                // Llamar al repositorio para obtener la tarea por ID
                var resultado = await _tareaAsignadaRepositorio.CompletarTareaRecurrente(recurrentId, newStatusId,lenguaje);

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
        
    }
}
