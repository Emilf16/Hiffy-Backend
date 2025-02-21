using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Repositorios
{
    public class NotificacionRepositorio 
    {
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebasetranslate;


        public NotificacionRepositorio(AppDbContext context, FirebaseTranslationService firebasetranslate)
        {
            _context = context;
            _firebasetranslate = firebasetranslate;

        }

        public async Task<OperationResult> ObtenerNotificacionesPorUsuario(int idUsuario, string lenguaje = "es")
        {
            // Obtener las notificaciones asociadas al usuario, ordenadas por fecha de envío descendente
            var notificaciones = await _context.Notificacion
                .Where(n => n.IdUsuarioDestino == idUsuario)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new
                {
                    n.IdNotificacion,
                    Titulo = lenguaje != "es" ? _firebasetranslate.Traducir(n.Titulo, lenguaje) : n.Titulo,
                    Mensaje = lenguaje != "es" ? _firebasetranslate.Traducir(n.Mensaje, lenguaje) : n.Mensaje,
                    n.FechaEnvio,
                    n.Estado
                })
                .ToListAsync();

            if (!notificaciones.Any())
            {
                var mensaje = _firebasetranslate.Traducir("No se encontraron notificaciones para este usuario.", lenguaje);

                return new OperationResult(false, mensaje);
            }

            var mensaje2 = _firebasetranslate.Traducir("Notificaciones obtenidas exitosamente.", lenguaje);

            return new OperationResult(true, mensaje2, notificaciones);
        }

        public async Task<OperationResult> EliminarNotificacionAsync(int idNotificacion, string lenguaje = "es")
        {
            var notificacion = await _context.Notificacion.FindAsync(idNotificacion);

            if (notificacion == null)
            {
                var mensaje = _firebasetranslate.Traducir("La notificación no existe.", lenguaje);

                return new OperationResult(false, mensaje);
                
            }

            _context.Notificacion.Remove(notificacion);
            await _context.SaveChangesAsync();

            var mensaje2 = _firebasetranslate.Traducir("Notificacion leida con correctamente", lenguaje);

            return new OperationResult(true, mensaje2);
            
        }
    }

}
