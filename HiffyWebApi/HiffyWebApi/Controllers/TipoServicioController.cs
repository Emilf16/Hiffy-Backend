using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiffyWebApi.Controllers
{
    [Route("ServiceType")]
    [ApiController]
    public class TipoServicioController : ControllerBase
    {
        private readonly TipoServicioRepositorio _tipoServicioRepositorio;
        private readonly FirebaseTranslationService _firebasetranslate;


        public TipoServicioController(TipoServicioRepositorio tipoServicioRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _tipoServicioRepositorio = tipoServicioRepositorio;
            _firebasetranslate = firebasetranslate;

        }

        /// <summary>
        /// Allows querying all available types of services.
        /// </summary>
        /// <param name="nombre">Allows searching by name, leave empty to get all</param> 
        [HttpGet]
        [Authorize]
        public async Task<OperationResult> GetTipoServicio(string? nombre)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                var tipoServicios = await _tipoServicioRepositorio.GetTipoServicio(nombre,lenguaje);

                var mensaje = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje, tipoServicios);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Create a type of service.
        /// </summary>
        /// <param name="tipoServicioModel">The DTO containing the service type data to be created</param> 
        [HttpPost]
        [Authorize]
        public async Task<OperationResult> CrearTipoServicio([FromBody] TipoServicio tipoServicioModel)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            try
            {
                var tipoServicio = await _tipoServicioRepositorio.CrearTipoServicio(tipoServicioModel,lenguaje);

                return tipoServicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Updates a type of service by its ID.
        /// </summary>
        /// <param name="tipoServicioModel">The DTO containing the service type data to be updated</param>
        /// <param name="id">The ID of the service type to be updated</param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<OperationResult> ActualizarTipoServicio([FromBody] TipoServicio tipoServicioModel, int id)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            try
            {
                var tipoServicio = await _tipoServicioRepositorio.ActualizarTipoServicio(tipoServicioModel, id,lenguaje);

                return tipoServicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a type of service by its ID.
        /// </summary>
        /// <param name="id">The ID of the service type to be deleted</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<OperationResult> EliminarTipoServicio(int id)
        {
            string lenguaje = Request.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(lenguaje))
            {
                lenguaje = "es";
            }

            try
            {
                var result = await _tipoServicioRepositorio.EliminarTipoServicio(id,lenguaje);

                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a type of service by its ID.
        /// </summary>
        /// <param name="id">The ID of the service type</param>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<OperationResult> GetTipoServicioPorId(int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var tipoServicio = await _tipoServicioRepositorio.GetTipoServicioPorId(id,lenguaje);

                return tipoServicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }
}
