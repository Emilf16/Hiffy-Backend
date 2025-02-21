using Hiffy_Datos;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiffyWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareaDomesticaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TareaDomesticaRepo _tareaDomesticaRepositorio;
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;

        public TareaDomesticaController(AppDbContext context, TareaDomesticaRepo tareaDomesticaRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _tareaDomesticaRepositorio = tareaDomesticaRepositorio;
            _firebasetranslate = firebasetranslate;
        }

        [HttpGet("get-tareaDomestica")]
        [Authorize]
        public async Task<OperationResult> ListaTareaDomestica(bool buscarPredeterminados, bool incluirActivas)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                int familiaId = 0;

                if (buscarPredeterminados)
                {
                    var resultado = await _tareaDomesticaRepositorio.MostrarTareasDomesticas(buscarPredeterminados, familiaId, incluirActivas, lenguaje);
                    return resultado;
                }
                else
                {
                    var familiaIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdFamilia);
                    if (familiaIdClaim == null)
                    {
                        var mensaje =  _firebasetranslate.Traducir("Error al validar usuario. No se encontró IdFamilia en los claims.", lenguaje);
                        return new OperationResult(false, mensaje);
                    }

                    familiaId = int.Parse(familiaIdClaim.Value);
                    var resultado = await _tareaDomesticaRepositorio.MostrarTareasDomesticas(buscarPredeterminados, familiaId, incluirActivas, lenguaje);
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<OperationResult> CrearTareaDomestica(TareaDomesticaPostDto nuevaTarea)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (idUsuarioClaim == null)
                {
                    var mensaje =  _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);
                var resultado = await _tareaDomesticaRepositorio.CrearTareaDomestica(idUsuario, nuevaTarea, lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<OperationResult> EditarTareaDomestica(TareaDomesticaPostDto tareaEditada)
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var idUsuarioClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsService.IdUsuario);
                if (idUsuarioClaim == null)
                {
                    var mensaje =  _firebasetranslate.Traducir("Error al validar usuario. No se encontró el IdUsuario en los claims.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                int idUsuario = int.Parse(idUsuarioClaim.Value);
                var resultado = await _tareaDomesticaRepositorio.EditarTareaDomestica(idUsuario, tareaEditada, lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpGet("get-tipos-tarea")]
        [Authorize]
        public async Task<OperationResult> ObtenerTiposTarea()
        {
            try
            {
                string lenguaje = Request.Headers["Accept-Language"];
                if (string.IsNullOrEmpty(lenguaje))
                {
                    lenguaje = "es";
                }

                var resultado = await _tareaDomesticaRepositorio.ObtenerTiposTarea(lenguaje);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al obtener tipos de tarea: {ex.Message}");
            }
        }
    }
}
