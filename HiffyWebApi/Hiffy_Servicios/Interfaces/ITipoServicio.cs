using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Interfaces
{
    public interface ITipoServicio
    {
        Task<OperationResult> CrearTipoServicio(TipoServicio tipoServicioModel, string lenguaje = "es");
        Task<OperationResult> ActualizarTipoServicio(TipoServicio tipoServicioModel, int idTipoServicio, string lenguaje = "es");
        Task<OperationResult> EliminarTipoServicio(int idTipoServicio, string lenguaje = "es");
        Task<OperationResult> GetTipoServicioPorId(int idTipoServicio, string lenguaje = "es");
        Task<IEnumerable<TipoServicio>> GetTipoServicio(string nombre, string lenguaje = "es");
        Task<bool> ExisteTipoServicio(string nombre, int idTipoServicio, string lenguaje = "es");
    }
}
