using Google.Cloud.Translation.V2;
using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Repositorios
{
    public class TipoServicioRepositorio : ITipoServicio
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly FirebaseTranslationService _firebasetranslate;


        public TipoServicioRepositorio(AppDbContext context, IConfiguration configuration, FirebaseTranslationService firebasetranslate
)
        {
            _context = context;
            _configuration = configuration;
            _firebasetranslate = firebasetranslate;

        }

        public async Task<OperationResult> ActualizarTipoServicio(TipoServicio tipoServicioModel, int idTipoServicio, string lenguaje = "es")
        {
            if (idTipoServicio != tipoServicioModel.IdTipoServicio)
            {
                var mensaje = _firebasetranslate.Traducir("El Id no coincide con el modelo.", lenguaje);

                return new OperationResult(false, mensaje, tipoServicioModel.IdTipoServicio);
            }

            bool existe = await ExisteTipoServicio(tipoServicioModel.Nombre, tipoServicioModel.IdTipoServicio);
            if (!existe)
            {
                var tipoServicio = await _context.TipoServicio.Where(x => x.IdTipoServicio == idTipoServicio).FirstOrDefaultAsync();

                if (tipoServicio == null)
                {
                    var mensaje = _firebasetranslate.Traducir("El Tipo de Servicio no fue encontrado.", lenguaje);

                    return new OperationResult(false, mensaje, idTipoServicio);
                }

                tipoServicio.Nombre = tipoServicioModel.Nombre;
                tipoServicio.Descripcion = tipoServicioModel.Descripcion;
                await _context.SaveChangesAsync();

                var mensaje2 = _firebasetranslate.Traducir("Actualización Exitosa.", lenguaje);

                return new OperationResult(true, mensaje2, tipoServicio.IdTipoServicio);
            }
            else
            {
                var mensaje = _firebasetranslate.Traducir("Ya existe un registro con este nombre.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }
        }

        public async Task<OperationResult> CrearTipoServicio(TipoServicio tipoServicioModel, string lenguaje = "es")
        {
            bool existe = await ExisteTipoServicio(tipoServicioModel.Nombre, tipoServicioModel.IdTipoServicio);

            if (!existe)
            {
                var tipoServicio = new TipoServicio
                {
                    Nombre = tipoServicioModel.Nombre,
                    Descripcion = tipoServicioModel.Descripcion
                };

                await _context.TipoServicio.AddAsync(tipoServicio);
                await _context.SaveChangesAsync();

                var mensaje = _firebasetranslate.Traducir("Registro Exitoso.", lenguaje);

                return new OperationResult(true, mensaje, tipoServicio.IdTipoServicio);
            }
            else
            {
                var mensaje = _firebasetranslate.Traducir("Ya existe un registro con este nombre.", lenguaje);

                return new OperationResult(false, mensaje, 0);
            }
        }

        public async Task<bool> ExisteTipoServicio(string nombre, int idTipoServicio, string lenguaje = "es")
        {
            bool existe = await _context.TipoServicio
                .AsNoTracking()
                .Where(x => x.Nombre.Trim().ToUpper() == nombre.Trim().ToUpper()
                    && x.IdTipoServicio != idTipoServicio)
                .AnyAsync();

            return existe;
        }

        public async Task<OperationResult> GetTipoServicioPorId(int idTipoServicio, string lenguaje = "es")
        {
            var tipoServicio = await _context.TipoServicio.Where(x => x.IdTipoServicio == idTipoServicio).Select(x => new TipoServicio
            {
                IdTipoServicio = x.IdTipoServicio,
                Nombre = _firebasetranslate.Traducir( x.Nombre,lenguaje),
                Descripcion = _firebasetranslate.Traducir(x.Descripcion, lenguaje),
            }).FirstOrDefaultAsync();

            if (tipoServicio == null)
            {
                var mensaje = _firebasetranslate.Traducir("Tipo de Servicio no encontrado.", lenguaje);

                return new OperationResult(false, mensaje, null);
            }

            var mensaje2 = _firebasetranslate.Traducir("Tipo de Servicio encontrado.", lenguaje);


            return new OperationResult(true, mensaje2, tipoServicio);
        }

        public async Task<IEnumerable<TipoServicio>> GetTipoServicio(string nombre, string lenguaje = "es")
        {
            if (string.IsNullOrEmpty(nombre))
            {
                nombre = "";
            }

            var tipoServicios = await _context.TipoServicio.Where(x => x.Nombre.Trim().ToUpper().Contains(nombre.Trim().ToUpper())).Select(x => new TipoServicio
            {
                IdTipoServicio = x.IdTipoServicio,
                Nombre = _firebasetranslate.Traducir(x.Nombre, lenguaje),
                Descripcion = _firebasetranslate.Traducir(x.Descripcion, lenguaje),
            }).ToListAsync();

            return tipoServicios;
        }

        public async Task<OperationResult> EliminarTipoServicio(int idTipoServicio, string lenguaje = "es")
        {
            var tipoServicio = await _context.TipoServicio.FindAsync(idTipoServicio);

            if (tipoServicio == null)
            {
                var mensaje = _firebasetranslate.Traducir("El Tipo de Servicio no fue encontrado.", lenguaje);

                return new OperationResult(false, mensaje, idTipoServicio);
            }

            _context.TipoServicio.Remove(tipoServicio);
            await _context.SaveChangesAsync();

            var mensaje2 = _firebasetranslate.Traducir("Eliminación Exitosa.", lenguaje);

            return new OperationResult(true, mensaje2, idTipoServicio);
        }
    }
}