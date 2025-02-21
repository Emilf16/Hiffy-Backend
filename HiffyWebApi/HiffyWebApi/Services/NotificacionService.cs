using Hiffy_Datos;
using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using Hiffy_Servicios.Interfaces;
using Hiffy_Servicios.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Hiffy_Servicios.Servicios
{
  

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly FirebaseTranslationService _firebaseTranslate;

        public NotificationService(
            AppDbContext context,
            FirebaseTranslationService firebaseTranslate)
        {
            _context = context;
            _firebaseTranslate = firebaseTranslate;
        }

        public async Task<OperationResult> SendNotificationAsync(string title, string message, int idUsuarioDestino)
        {
            try
            {
                // 1. Crear y guardar la notificación
                var notificacion = new Notificacion
                {
                    Titulo = title,
                    Mensaje = message,
                    IdUsuarioDestino = idUsuarioDestino,
                    FechaEnvio = DateTime.Now,
                    Estado = "No leído" // Puedes crear un enum para los estados
                };

                await _context.Notificacion.AddAsync(notificacion);

               

                await _context.SaveChangesAsync();

                return new OperationResult(true, "Notificación enviada exitosamente");
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al enviar la notificación: {ex.Message}");
            }
        }

        public async Task<OperationResult> GetNotificationsByUserAsync(int idUsuario)
        {
            try
            {
                var notifications = await _context.Notificacion
                    .Where(n => n.IdUsuarioDestino == idUsuario)
                    .OrderByDescending(n => n.FechaEnvio)
                    .ToListAsync();

                return new OperationResult(true, "Notificaciones recuperadas exitosamente", notifications);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al recuperar las notificaciones: {ex.Message}");
            }
        }

        public async Task<OperationResult> MarkNotificationAsReadAsync(int idNotificacion)
        {
            try
            {
                var notification = await _context.Notificacion
                    .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion);

                if (notification == null)
                {
                    return new OperationResult(false, "Notificación no encontrada");
                }

                notification.Estado = "Leído";
                await _context.SaveChangesAsync();

                return new OperationResult(true, "Notificación marcada como leída");
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al marcar la notificación como leída: {ex.Message}");
            }
        }
    }
}