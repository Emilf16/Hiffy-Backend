using Hiffy_Datos;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Repositorios;
using HiffyWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HiffyWebApi.Controllers
{
    [Route("api/AreaHogar")]
    [ApiController]
    public class AreaDelHogarController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AreaDelHogarRepo _areaDelHogarRepositorio;
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;

        public AreaDelHogarController(AppDbContext context, AreaDelHogarRepo areaDelHogarRepositorio, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _areaDelHogarRepositorio = areaDelHogarRepositorio;
            _firebasetranslate = firebasetranslate;
        }

        [HttpGet("get-areaHogar")]
        [Authorize]
        public async Task<OperationResult> ListaAreasDelHogar(bool buscarPredeterminados, bool incluirActivas)
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
                    var resultado = await _areaDelHogarRepositorio.MostrarAreasDelHogar(buscarPredeterminados, familiaId, incluirActivas, lenguaje);
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
                    var resultado = await _areaDelHogarRepositorio.MostrarAreasDelHogar(buscarPredeterminados, familiaId, incluirActivas, lenguaje);
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
        public async Task<OperationResult> CrearAreaDelHogar(List<AreaDelHogarDto> nuevaArea)
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
                var usuario = await _context.Usuario.Include(u => u.RolFamilia)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    var mensaje =  _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var rolFamilia = usuario.RolFamilia?.EsAdmin;
                if (!rolFamilia.Value)
                {
                    var mensaje =  _firebasetranslate.Traducir("No tienes permisos para crear áreas del hogar.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _areaDelHogarRepositorio.CrearAreaDelHogar(usuario.IdUsuario, nuevaArea);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<OperationResult> EditarAreaDelHogar(AreaDelHogarDto areaEditada)
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
                var usuario = await _context.Usuario.Include(u => u.RolFamilia)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    var mensaje =  _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var rolFamilia = usuario.RolFamilia?.EsAdmin;
                if (!rolFamilia.Value)
                {
                    var mensaje =  _firebasetranslate.Traducir("No tienes permisos para editar áreas del hogar.", lenguaje);
                    return new OperationResult(false, mensaje);
                }

                var resultado = await _areaDelHogarRepositorio.EditarAreaDelHogar(usuario.IdUsuario, areaEditada);
                return resultado;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }
}
