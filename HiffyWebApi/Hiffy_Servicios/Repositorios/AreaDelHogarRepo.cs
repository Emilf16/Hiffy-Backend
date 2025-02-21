using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
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
    public class AreaDelHogarRepo
    {
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;

        public AreaDelHogarRepo(AppDbContext context, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _firebasetranslate = firebasetranslate;
        }

        public async Task<OperationResult> MostrarAreasDelHogar(bool buscarPredeterminados, int IdFamilia, bool incluirActivas, string lenguaje = "es")
        {
            var query = _context.AreaDelHogar_Familia.AsQueryable();

            if (buscarPredeterminados)
            {
                query = query.Where(x => x.Predeterminado == true);
            }
            else
            {
                query = query.Where(x => x.IdFamilia == IdFamilia);
            }

            if (incluirActivas)
            {
                query = query.Where(x => x.IdEstadoAreasDelHogar == EstadoAreasDelHogar.Activo);
            }

            var areasDelHogar = await query.Select(area => new AreaDelHogarDto
            {
                IdAreaFamilia = area.IdAreaFamilia,
                Nombre = _firebasetranslate.Traducir(area.Nombre, lenguaje),
                Descripcion = _firebasetranslate.Traducir(area.Descripcion, lenguaje),
                Predeterminado = area.Predeterminado,
                IdEstadoAreasDelHogar = area.IdEstadoAreasDelHogar
            }).ToListAsync();

            var mensaje = _firebasetranslate.Traducir("Acción exitosa", lenguaje);
            return new OperationResult(true, mensaje, areasDelHogar);
        }

        public async Task<OperationResult> CrearAreaDelHogar(int idUsuario, List<AreaDelHogarDto> nuevasAreas, string lenguaje = "es")
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            var areasNoCreadas = new List<object>();

            foreach (var nuevaArea in nuevasAreas)
            {
                var areaExistente = await _context.AreaDelHogar_Familia
                    .FirstOrDefaultAsync(a => a.Nombre == nuevaArea.Nombre
                                              && (a.IdFamilia == usuario.IdFamilia.Value || a.Predeterminado));

                if (areaExistente != null)
                {
                    areasNoCreadas.Add(new
                    {
                        Nombre = nuevaArea.Nombre,
                        Mensaje = _firebasetranslate.Traducir("Nombre de área ya existe en la familia o como predeterminada.", lenguaje)
                    });
                    continue;
                }

                var areaDelHogar = new AreaDelHogar_Familia
                {
                    IdFamilia = usuario.IdFamilia.Value,
                    Nombre = nuevaArea.Nombre,
                    Descripcion = nuevaArea.Descripcion,
                    Predeterminado = false,
                    IdEstadoAreasDelHogar = EstadoAreasDelHogar.Activo
                };

                _context.AreaDelHogar_Familia.Add(areaDelHogar);
            }

            await _context.SaveChangesAsync();

            if (areasNoCreadas.Any())
            {
                var mensaje = _firebasetranslate.Traducir("Algunas áreas no se pudieron crear debido a nombres duplicados.", lenguaje);
                return new OperationResult(false, mensaje, areasNoCreadas);
            }

            var mensajeExito = _firebasetranslate.Traducir("Áreas del hogar creadas exitosamente.", lenguaje);
            return new OperationResult(true, mensajeExito);
        }

        public async Task<OperationResult> EditarAreaDelHogar(int idUsuario, AreaDelHogarDto areaEditada, string lenguaje = "es")
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                var mensaje = _firebasetranslate.Traducir("Usuario no encontrado.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            var areaExistente = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a =>
                    a.Nombre == areaEditada.Nombre &&
                    a.IdAreaFamilia != areaEditada.IdAreaFamilia &&
                    (a.IdFamilia == usuario.IdFamilia.Value || (a.Predeterminado && a.IdFamilia == null))
                );

            if (areaExistente != null)
            {
                var mensaje = _firebasetranslate.Traducir("Ya existe un área con ese nombre en la familia o como predeterminada.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            var areaDelHogar = await _context.AreaDelHogar_Familia
                .FirstOrDefaultAsync(a => a.IdAreaFamilia == areaEditada.IdAreaFamilia && a.IdFamilia == usuario.IdFamilia.Value);

            if (areaDelHogar == null)
            {
                var mensaje = _firebasetranslate.Traducir("Área no encontrada.", lenguaje);
                return new OperationResult(false, mensaje);
            }

            areaDelHogar.Nombre = areaEditada.Nombre;
            areaDelHogar.Descripcion = areaEditada.Descripcion;
            areaDelHogar.IdEstadoAreasDelHogar = areaEditada.IdEstadoAreasDelHogar;

            await _context.SaveChangesAsync();

            var mensajeExito = _firebasetranslate.Traducir("Área del hogar editada exitosamente.", lenguaje);
            return new OperationResult(true, mensajeExito);
        }
    }
}