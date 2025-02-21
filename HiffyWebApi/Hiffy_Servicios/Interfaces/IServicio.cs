using Hiffy_Entidades.Entidades;
using Hiffy_Servicios.Common;
using Hiffy_Servicios.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiffy_Servicios.Interfaces
{
    public interface IServicio
    {
        Task<OperationResult> CrearServicio(PostServicioDto servicioModel, string lenguaje = "es");
        Task<OperationResult> ActualizarServicio(PostServicioDto servicioModel, int idServicio, string lenguaje = "es");
        Task<OperationResult> EliminarServicio(int idServicio, string lenguaje = "es");
        Task<OperationResult> GetServicioPorId(int idServicio, string lenguaje = "es");
        Task<IEnumerable<GetServicioDto>> GetServicios(string? name, string? vendorName, string? descripcion, decimal? priceMin, decimal? primeMax, int? typeServiceId, DateTime? releaseDateFrom, DateTime? releaseDateTo, int? valoracionDesde, int? valoracionHasta, string lenguaje = "es");
    }
}
