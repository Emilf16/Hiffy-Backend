using Hiffy_Servicios.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Interfaces
{
    public interface INotificationService
    {
        Task<OperationResult> SendNotificationAsync(string title, string message, int idUsuarioDestino);
        Task<OperationResult> GetNotificationsByUserAsync(int idUsuario);
        Task<OperationResult> MarkNotificationAsReadAsync(int idNotificacion);
    }
}
