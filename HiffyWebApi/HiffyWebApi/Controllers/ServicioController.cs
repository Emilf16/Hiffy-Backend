using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiffyWebApi.Controllers
{
    [Route("Service")]
    [ApiController]
    public class ServicioController : ControllerBase
    {
        private readonly ServicioRepositorio _servicioRepositorio;
        private readonly FirebaseTranslationService _firebasetranslate;


        public ServicioController(ServicioRepositorio servicioRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _servicioRepositorio = servicioRepositorio;
            _firebasetranslate = firebasetranslate;

        }

        /// <summary>
        /// Allows querying all available services with optional filters.
        /// </summary>
        /// <param name="name">Searches by service name. Leave empty to get all services.</param>
        /// <param name="vendorName">Searches by vendor's name. Leave empty to get all vendors.</param>
        /// <param name="descripcion">Searches by service description. Leave empty to get all services.</param>
        /// <param name="priceMin">Filters services with a minimum price. Pass null to ignore.</param>
        /// <param name="primeMax">Filters services with a maximum price. Pass null to ignore.</param>
        /// <param name="typeServiceId">Filters services by type. Pass null to ignore.</param>
        /// <param name="releaseDateFrom">Filters services by release date (from). Pass null to ignore.</param>
        /// <param name="releaseDateTo">Filters services by release date (to). Pass null to ignore.</param>
        [HttpGet]
        [Authorize]
        public async Task<OperationResult> GetServicios(string? name, string? vendorName, string? descripcion, decimal? priceMin, decimal? primeMax, int? typeServiceId, DateTime? releaseDateFrom, DateTime? releaseDateTo, int? ratingFrom, int? ratingTo)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var servicios = await _servicioRepositorio.GetServicios(name, vendorName, descripcion, priceMin, primeMax, typeServiceId, releaseDateFrom, releaseDateTo, ratingFrom, ratingTo,lenguaje);

                var mensaje = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje, servicios);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }


        /// <summary>
        /// Allows querying all available services by provider.
        /// </summary>
        /// <param name="nombre">Allows searching by name, leave empty to get all</param> 
        [HttpGet("my-services")]
        [Authorize]
        public async Task<OperationResult> GetServiciosDelVendedor(string? nombre)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }
                // Extract the userId from the JWT token
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(usuarioIdClaim.Value);

                var servicios = await _servicioRepositorio.GetServiciosDelVendedor(nombre, usuarioId);

                var mensaje2 = _firebasetranslate.Traducir("Listado obtenido de manera exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, servicios);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Create a service.
        /// </summary>
        /// <param name="servicioModel">The DTO containing the service data to be created</param> 
        [HttpPost]
        [Authorize]
        public async Task<OperationResult> CrearServicio([FromBody] PostServicioDto servicioModel)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Extract the userId from the JWT token
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(usuarioIdClaim.Value);

                // Call the repository method to upload the certifi

                servicioModel.IdUsuario = usuarioId;

                var servicio = await _servicioRepositorio.CrearServicio(servicioModel,lenguaje);

                return servicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Updates a service by its ID.
        /// </summary>
        /// <param name="servicioModel">The DTO containing the service data to be updated</param>
        /// <param name="id">The ID of the service to be updated</param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<OperationResult> ActualizarServicio([FromBody] PostServicioDto servicioModel, int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var servicio = await _servicioRepositorio.ActualizarServicio(servicioModel, id,lenguaje);

                return servicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Deletes a service by its ID.
        /// </summary>
        /// <param name="id">The ID of the service to be deleted</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<OperationResult> EliminarServicio(int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var result = await _servicioRepositorio.EliminarServicio(id,lenguaje);

                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a service by its ID.
        /// </summary>
        /// <param name="id">The ID of the service</param>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<OperationResult> GetServicioPorId(int id)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var servicio = await _servicioRepositorio.GetServicioPorId(id,lenguaje);

                return servicio;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Updates the location of a user by their ID extracted from the JWT token.
        /// </summary>
        /// <param name="latitud">The latitude of the user's location</param>
        /// <param name="longitud">The longitude of the user's location</param>
        [HttpPut("vendor-location/{latitud}/{longitud}")]
        [Authorize]
        public async Task<OperationResult> PutUbicacionUsuario(string latitud, string longitud)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                // Extract the userId from the JWT token
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (usuarioIdClaim == null)
                {
                    var mensaje = _firebasetranslate.Traducir("No se ha podido obtener el ID del usuario desde el token.", lenguaje);

                    return new OperationResult(false, mensaje);
                }

                // Convert the claim to an integer (assuming it is an int)
                int usuarioId = int.Parse(usuarioIdClaim.Value);


                // Llama al repositorio para actualizar la ubicación del usuario
                var resultado = await _servicioRepositorio.GuardarUbicacionVendedor(
                    usuarioId,
                    latitud,
                    longitud,
                    lenguaje
                );

                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }
}